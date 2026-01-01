using System;
using System.Collections.Generic;
using System.IO;
using UndertaleModLib.Models;
using SkiaSharp;

namespace UndertaleModLib.Util
{
    /// <summary>
    /// Helper class used to manage and cache textures (SkiaSharp version).
    /// </summary>
    public class TextureWorkerSkia : IDisposable
    {
        private Dictionary<UndertaleEmbeddedTexture, SKBitmap> embeddedDictionary = new();
        private readonly object embeddedDictionaryLock = new();

        /// <inheritdoc cref="TextureWorker.GetEmbeddedTexture"/>
        public SKBitmap GetEmbeddedTexture(UndertaleEmbeddedTexture embeddedTexture)
        {
            lock (embeddedDictionaryLock)
            {
                if (embeddedDictionary.TryGetValue(embeddedTexture, out var bmp))
                    return bmp;

                // 与原版一致：从 QoiTexture 解码 → SKBitmap
                var data = embeddedTexture.TextureData.Image.GetSkiaImage().Encode(SKEncodedImageFormat.Png, 100).ToArray();
                using var stream = new MemoryStream(data);
                var newBmp = SKBitmap.Decode(stream)
                               ?? throw new InvalidDataException("Failed to decode embedded texture.");
                embeddedDictionary[embeddedTexture] = newBmp;
                return newBmp;
            }
        }

        /// <inheritdoc cref="TextureWorker.ExportAsPNG"/>
        public void ExportAsPNG(UndertaleTexturePageItem texPageItem,
                                string filePath,
                                string imageName = null,
                                bool includePadding = false)
        {
            using var image = GetTextureFor(texPageItem,
                                            imageName ?? Path.GetFileNameWithoutExtension(filePath),
                                            includePadding);
            SaveImageToFile(image, filePath);
        }

        /// <inheritdoc cref="TextureWorker.GetTextureFor"/>
        public  SKBitmap GetTextureFor(UndertaleTexturePageItem texPageItem,
                                      string imageName,
                                      bool includePadding = false)
        {
            var pageBmp = GetEmbeddedTexture(texPageItem.TexturePage);

            int exportW = texPageItem.BoundingWidth;
            int exportH = texPageItem.BoundingHeight;

            if (includePadding &&
                (texPageItem.TargetWidth > exportW || texPageItem.TargetHeight > exportH))
            {
                throw new InvalidDataException(
                    $"{imageName}'s texture is larger than its bounding box!");
            }

            // 裁剪
            var cropInfo = new SKRectI(texPageItem.SourceX,
                                       texPageItem.SourceY,
                                       texPageItem.SourceX + texPageItem.SourceWidth,
                                       texPageItem.SourceY + texPageItem.SourceHeight);

            SKBitmap cropped;
            lock (pageBmp)
            {
                cropped = new SKBitmap(cropInfo.Width, cropInfo.Height);
                pageBmp.ExtractSubset(cropped, cropInfo);
            }

            // 缩放
            if (texPageItem.SourceWidth != texPageItem.TargetWidth ||
                texPageItem.SourceHeight != texPageItem.TargetHeight)
            {
                var resized = cropped.Resize(
                    new SKImageInfo(texPageItem.TargetWidth, texPageItem.TargetHeight),
                    SKFilterQuality.Medium);
                cropped.Dispose();
                cropped = resized;
            }

            // 加 padding（透明画布）
            if (includePadding)
            {
                var padded = new SKBitmap(exportW, exportH);
                using var canvas = new SKCanvas(padded);
                canvas.Clear(SKColors.Transparent);
                canvas.DrawBitmap(cropped,
                                  new SKPoint(texPageItem.TargetX, texPageItem.TargetY));
                cropped.Dispose();
                cropped = padded;
            }

            return cropped;
        }

        /* ---------- 静态工具方法 ---------- */

        /// <inheritdoc cref="TextureWorker.ReadBGRAImageFromFile"/>
        public static SKBitmap ReadBGRAImageFromFile(string filePath)
        {
            using var data = SKData.Create(filePath);
            var bmp = SKBitmap.Decode(data) ?? throw new FileLoadException("Cannot load image.", filePath);
            if (bmp.ColorType != SKColorType.Bgra8888)
            {
                var converted = bmp.Copy(SKColorType.Bgra8888);
                bmp.Dispose();
                return converted;
            }
            return bmp;
        }

        /// <inheritdoc cref="TextureWorker.ResizeImage"/>
        public static SKBitmap ResizeImage(SKBitmap image,
                                           int width,
                                           int height,
                                           SKFilterQuality quality = SKFilterQuality.Medium)
        {
            if (image.Width == width && image.Height == height)
                return image.Copy(); // 保持语义：总是返回新实例

            var resized = image.Resize(new SKImageInfo(width, height), quality);
            return resized;
        }

        /// <inheritdoc cref="TextureWorker.ReadMaskData"/>
        public static byte[] ReadMaskData(string filePath, int requiredWidth, int requiredHeight)
        {
            using var bmp = ReadBGRAImageFromFile(filePath);
            if (bmp.Width != requiredWidth || bmp.Height != requiredHeight)
                throw new Exception(
                    $"{filePath} is not the proper size! Expected {requiredWidth}x{requiredHeight}.");

            var pixels = bmp.Pixels; // SKColor[]
            var bytes = new List<byte>((requiredWidth + 7) / 8 * requiredHeight);

            for (int y = 0; y < requiredHeight; y++)
            {
                for (int xByte = 0; xByte < (requiredWidth + 7) / 8; xByte++)
                {
                    byte b = 0;
                    int pxStart = xByte * 8;
                    int pxEnd = Math.Min(pxStart + 8, requiredWidth);
                    for (int x = pxStart; x < pxEnd; x++)
                    {
                        var color = pixels[y * requiredWidth + x];
                        if (color == SKColors.White)
                            b |= (byte)(1 << (7 - (x - pxStart)));
                    }
                    bytes.Add(b);
                }
            }
            return bytes.ToArray();
        }

        /// <inheritdoc cref="TextureWorker.GetCollisionMaskImage"/>
        public static SKBitmap GetCollisionMaskImage(UndertaleSprite.MaskEntry mask,
                                                     int maskWidth,
                                                     int maskHeight)
        {
            var bmp = new SKBitmap(maskWidth, maskHeight, SKColorType.Alpha8, SKAlphaType.Opaque);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.Black);

            var paint = new SKPaint { Color = SKColors.White };
            byte[] data = mask.Data;

            for (int y = 0; y < maskHeight; y++)
            {
                int rowStart = y * ((maskWidth + 7) / 8);
                for (int x = 0; x < maskWidth; x++)
                {
                    byte b = data[rowStart + (x / 8)];
                    bool bit = (b & (1 << (7 - (x % 8)))) != 0;
                    if (bit)
                        canvas.DrawPoint(x, y, paint);
                }
            }
            return bmp;
        }

        /// <inheritdoc cref="TextureWorker.ExportCollisionMaskPNG"/>
        public static void ExportCollisionMaskPNG(UndertaleSprite.MaskEntry mask,
                                                  string filePath,
                                                  int maskWidth,
                                                  int maskHeight)
        {
            using var bmp = GetCollisionMaskImage(mask, maskWidth, maskHeight);
            SaveImageToFile(bmp, filePath);
        }

        /// <inheritdoc cref="TextureWorker.SaveImageToFile"/>
        public static void SaveImageToFile(SKBitmap image, string filePath)
        {
            using var img = SKImage.FromBitmap(image);
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            using var fs = File.OpenWrite(filePath);
            data.SaveTo(fs);
        }

        /// <inheritdoc cref="TextureWorker.GetImageSizeFromFile"/>
        public static (int width, int height) GetImageSizeFromFile(string filePath)
        {
            try
            {
                using var codec = SKCodec.Create(filePath);
                return ((int)codec.Info.Width, (int)codec.Info.Height);
            }
            catch
            {
                return (-1, -1);
            }
        }

        /* ---------- IDisposable ---------- */
        public void Dispose()
        {
            if (embeddedDictionary != null)
            {
                foreach (var bmp in embeddedDictionary.Values)
                    bmp?.Dispose();
                embeddedDictionary.Clear();
                embeddedDictionary = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}