namespace UndertaleModToolAvalonia;
using System.IO;
using System.IO.Compression;

public class ZipExtractor
{
    public static void CreateDirectory(string path)
    {
        DirectoryInfo info = new DirectoryInfo(path);
        if (!info.Exists)
        {
            if (!info.Parent.Exists)
            {
                CreateDirectory(info.Parent.FullName);
            }
            info.Create();
        }
    }
    public static void ExtractZipStream(Stream zipStream, string baseDirectory)
    {
        // 确保基础目录存在
        if (!Directory.Exists(baseDirectory))
        {
            //Directory.CreateDirectory(baseDirectory);
            CreateDirectory(baseDirectory);
        }

        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: false))
        {
            foreach (var entry in archive.Entries)
            {
                // 跳过空目录条目
                if (string.IsNullOrEmpty(entry.Name) && string.IsNullOrEmpty(entry.FullName))
                    continue;

                string entryPath = Path.Combine(baseDirectory, entry.FullName);
                string directoryPath = Path.GetDirectoryName(entryPath);

                // 确保目录存在
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    CreateDirectory(directoryPath);
                }

                // 如果是文件（有文件名），则提取
                if (!string.IsNullOrEmpty(entry.Name))
                {
                    // 处理可能的安全问题：确保路径在目标目录内
                    string fullPath = Path.GetFullPath(entryPath);
                    if (!fullPath.StartsWith(Path.GetFullPath(baseDirectory)))
                    {
                        throw new IOException("试图访问基础目录之外的文件路径");
                    }

                    entry.ExtractToFile(fullPath, overwrite: true);
                }
            }
        }
    }
}