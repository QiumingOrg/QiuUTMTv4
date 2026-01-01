using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib;
using UndertaleModLib.Compiler;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Models;
using UndertaleModLib.Scripting;
using UndertaleModLib.Util;
using UTMTdrid;
using static UndertaleModLib.UndertaleReader;
using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

namespace UTMTdrid;

/// <summary>
/// Main CLI Program
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                            DynamicallyAccessedMemberTypes.PublicProperties |
                            DynamicallyAccessedMemberTypes.PublicEvents |
                            DynamicallyAccessedMemberTypes.PublicConstructors)]
public partial class QiuFuncMain : IScriptInterface
{
    #region Properties

    // taken from the Linux programmer manual:
    /// <summary>
    /// Value that should be returned on a successful operation.
    /// </summary>
    private const int EXIT_SUCCESS = 0;

    /// <summary>
    /// Value that should be returned on a failed operation.
    /// </summary>
    private const int EXIT_FAILURE = 1;

    /// <summary>
    /// Value that determines if the current Program is running in interactive mode.
    /// </summary>
    private bool IsInteractive { get; }

    /// <summary>
    /// Value that determines if the current Program is running in verbose mode.
    /// </summary>
    private bool Verbose { get; }

    /// <summary>
    /// File path or directory path that determines an output for the current Program.
    /// </summary>
    private FileSystemInfo Output { get; }

    /// <summary>
    /// Constant, used to indicate that the user wants to replace everything in a replace command.
    /// </summary>
    private const string UMT_REPLACE_ALL = "UMT_REPLACE_ALL";

    /// <summary>
    /// Constant, used to indicate that the user wants to dump everything in a dump command
    /// </summary>
    private const string UMT_DUMP_ALL = "UMT_DUMP_ALL";

    //TODO: document these, these are intertwined with inherited updating methods
    private int progressValue;
    private Task updater;
    private CancellationTokenSource cTokenSource;
    private CancellationToken cToken;

    private string savedMsg, savedStatus;

    private double savedValue, savedValueMax;

    //private Page? MAUI_Page;
    private static DelegateOutput? Genouka_callback;

    /// <summary>
    /// The ScriptOptions, only used for <see cref="CSharpScript"/>, aka running C# code.
    /// </summary>
    private ScriptOptions CliScriptOptions { get; }

    /// <summary>
    /// Determines if actions should show a "this is finished" text. Gets set by <see cref="SetFinishedMessage"/>.
    /// </summary>
    private bool FinishedMessageEnabled { get; set; }

    #endregion

    /// <summary>
    /// Main entrypoint for Cli
    /// </summary>
    /// <param name="args">Arguments passed on to program.</param>
    /// <returns>Result code of the program.</returns>
    [DynamicDependency("DecompilerSettings", "ToolInfo", "UndertaleModLib")]
    public QiuFuncMain(FileInfo datafile, FileInfo[] scripts, FileInfo output, bool verbose = false,
        bool interactive = false)
    {
        this.Verbose = verbose;
        IsInteractive = false;

        GenoukaUI_WriteLine($"Trying to load file: '{datafile.FullName}'");

        this.FilePath = datafile.FullName;
        this.ExePath = FileSystem.Current.CacheDirectory;
        this.Output = output ?? new FileInfo(FileSystem.Current.CacheDirectory);

        this.Data = ReadDataFile(datafile, WarningHandler, this.Verbose ? MessageHandler : DummyHandler);

        FinishedMessageEnabled = true;
        try
        {
            var cliscript = ScriptOptions.Default;
            cliscript = cliscript.WithAllowUnsafe(true);
            cliscript = cliscript.WithEmitDebugInformation(true);
            //var t=new UndertaleModLib.ToolInfo();
            cliscript = cliscript.AddImports("UndertaleModLib", "UndertaleModLib.Models", "UndertaleModLib.Decompiler",
                "UndertaleModLib.Scripting", "UndertaleModLib.Compiler",
                "UndertaleModLib.Util", "System", "System.IO", "System.Collections.Generic",
                "System.Text.RegularExpressions");
            cliscript = cliscript.AddReferences(typeof(UndertaleObject).GetTypeInfo().Assembly,
                GetType().GetTypeInfo().Assembly,
                typeof(JsonConvert).GetTypeInfo().Assembly,
                typeof(System.Text.RegularExpressions.Regex).GetTypeInfo().Assembly,
                typeof(TextureWorker).GetTypeInfo().Assembly,
                typeof(ImageMagick.MagickImage).GetTypeInfo().Assembly,
                typeof(Underanalyzer.Decompiler.DecompileContext).Assembly);
            this.CliScriptOptions = cliscript;
        }
        catch (Exception ee)
        {
            Debug.WriteLine(ee.Message);
        }
    }

    /// <summary>
    /// Stub for QiuUTMTv4
    /// </summary>
    [DynamicDependency("DecompilerSettings", "ToolInfo", "UndertaleModLib")]
    public QiuFuncMain(String fullname,UndertaleData data, FileInfo[] scripts, FileInfo output, bool verbose = false,
        bool interactive = false)
    {
        this.Verbose = verbose;
        IsInteractive = false;

        GenoukaUI_WriteLine($"Trying to load file: '{fullname}'");

        this.FilePath = fullname;
        this.ExePath = FileSystem.Current.CacheDirectory;
        this.Output = output ?? new FileInfo(FileSystem.Current.CacheDirectory);

        this.Data = data;

        FinishedMessageEnabled = true;
        try
        {
            var cliscript = ScriptOptions.Default;
            cliscript = cliscript.WithAllowUnsafe(true);
            cliscript = cliscript.WithEmitDebugInformation(true);
            //var t=new UndertaleModLib.ToolInfo();
            cliscript = cliscript.AddImports("UndertaleModLib", "UndertaleModLib.Models", "UndertaleModLib.Decompiler",
                "UndertaleModLib.Scripting", "UndertaleModLib.Compiler",
                "UndertaleModLib.Util", "System", "System.IO", "System.Collections.Generic",
                "System.Text.RegularExpressions");
            cliscript = cliscript.AddReferences(typeof(UndertaleObject).GetTypeInfo().Assembly,
                GetType().GetTypeInfo().Assembly,
                typeof(JsonConvert).GetTypeInfo().Assembly,
                typeof(System.Text.RegularExpressions.Regex).GetTypeInfo().Assembly,
                typeof(TextureWorker).GetTypeInfo().Assembly,
                typeof(ImageMagick.MagickImage).GetTypeInfo().Assembly,
                typeof(Underanalyzer.Decompiler.DecompileContext).Assembly);
            this.CliScriptOptions = cliscript;
        }
        catch (Exception ee)
        {
            Debug.WriteLine(ee.Message);
        }
    }
    
    public QiuFuncMain(FileInfo datafile, bool verbose, DirectoryInfo output = null)
    {
        if (datafile == null) throw new ArgumentNullException(nameof(datafile));

        GenoukaUI_WriteLine($"Trying to load file: '{datafile.FullName}'");
        this.Verbose = verbose;
        this.Data = ReadDataFile(datafile, verbose ? WarningHandler : null, verbose ? MessageHandler : null);
        this.Output = output ?? new DirectoryInfo(datafile.DirectoryName);

        if (this.Verbose)
            GenoukaUI_WriteLine("Output directory has been set to " + this.Output.FullName);
    }

    /// <summary>
    /// Method that gets executed on the "new" command
    /// </summary>
    /// <param name="options">The arguments that have been provided with the "new" command</param>
    /// <returns><see cref="EXIT_SUCCESS"/> and <see cref="EXIT_FAILURE"/> for being successful and failing respectively</returns>
    private static int New(NewOptions options)
    {
        //TODO: this should probably create a new Program instance, with just the properties that it needs

        UndertaleData data = UndertaleData.CreateNew();

        // If stdout flag is set, write new data to stdout and quit
        // if (options.Stdout)
        // {
        //     if (options.Verbose) GenoukaUI_WriteLine("Attempting to write new Data file to STDOUT...");
        //     using MemoryStream ms = new MemoryStream();
        //     UndertaleIO.Write(ms, data);
        //     Console.OpenStandardOutput().Write(ms.ToArray(), 0, (int)ms.Length);
        //     Console.Out.Flush();
        //     if (options.Verbose) GenoukaUI_WriteLine("Successfully wrote new Data file to STDOUT.");
        //
        //     return EXIT_SUCCESS;
        // }

        // If not STDOUT, write to file instead. Check first if we have permission to overwrite
        if (options.Output.Exists && !options.Overwrite)
        {
            //Console.Error.WriteLine($"'{options.Output}' already exists. Pass --overwrite to overwrite");
            return EXIT_FAILURE;
        }

        // We're not writing to STDOUT, and overwrite flag was given, so we write to specified file.
        if (options.Verbose) GenoukaUI_WriteLine($"Attempting to write new Data file to '{options.Output}'...");
        using FileStream fs = options.Output.OpenWrite();
        UndertaleIO.Write(fs, data);
        if (options.Verbose) GenoukaUI_WriteLine($"Successfully wrote new Data file to '{options.Output}'.");
        return EXIT_SUCCESS;
    }

    /// <summary>
    /// Method that gets executed on the "load" command
    /// </summary>
    /// <param name="options">The arguments that have been provided with the "load" command</param>
    /// <returns><see cref="EXIT_SUCCESS"/> and <see cref="EXIT_FAILURE"/> for being successful and failing respectively</returns>
    private static int Load(LoadOptions options)
    {
        QiuFuncMain program;

        // Try to load necessary values.
        // This can throw if mandatory arguments are not given, in which case we want to exit cleanly without a stacktrace.
        try
        {
            program = new QiuFuncMain(options.Datafile, options.Scripts, options.Output, options.Verbose,
                options.Interactive);
        }
        catch (Exception e)
        {
            //Console.Error.WriteLine(e.Message);
            return EXIT_FAILURE;
        }

        // if interactive is enabled, launch the menu instead
        if (options.Interactive)
        {
            program.RunInteractiveMenu();
            return EXIT_SUCCESS;
        }

        // if we have any scripts to run, run every one of them
        if (options.Scripts != null)
        {
            foreach (FileInfo script in options.Scripts)
                program.RunCSharpFile(script.FullName);
        }

        // if line to execute was given, execute it
        if (options.Line != null)
        {
            program.ScriptPath = null;
            program.RunCSharpCode(options.Line);
        }

        // if parameter to save file was given, save the data file
        if (options.Output != null)
            program.SaveDataFile(options.Output.FullName);

        return EXIT_SUCCESS;
    }

    /// <summary>
    /// Method that gets executed on the "info" command
    /// </summary>
    /// <param name="options">The arguments that have been provided with the "info" command</param>
    /// <returns><see cref="EXIT_SUCCESS"/> and <see cref="EXIT_FAILURE"/> for being successful and failing respectively</returns>
    private static int Info(InfoOptions options)
    {
        QiuFuncMain program;
        try
        {
            program = new QiuFuncMain(options.Datafile, options.Verbose);
        }
        catch (FileNotFoundException e)
        {
            //Console.Error.WriteLine(e.Message);
            return EXIT_FAILURE;
        }

        program.CliQuickInfo();
        return EXIT_SUCCESS;
    }

    /// <summary>
    /// Method that gets executed on the "dump" command
    /// </summary>
    /// <param name="options">The arguments that have been provided with the "dump" command</param>
    /// <returns><see cref="EXIT_SUCCESS"/> and <see cref="EXIT_FAILURE"/> for being successful and failing respectively</returns>
    private static int Dump(DumpOptions options)
    {
        QiuFuncMain program;
        try
        {
            program = new QiuFuncMain(options.Datafile, options.Verbose, options.Output);
        }
        catch (FileNotFoundException e)
        {
            //Console.Error.WriteLine(e.Message);
            return EXIT_FAILURE;
        }

        if (program.Data.IsYYC())
        {
            GenoukaUI_WriteLine(
                "The game was made with YYC (YoYo Compiler), which means that the code was compiled into the executable. " +
                "There is thus no code to dump. Exiting.");
            return EXIT_SUCCESS;
        }

        // If user provided code to dump, dump code
        if ((options.Code?.Length > 0) && (program.Data.Code?.Count > 0))
        {
            // If user wanted to dump everything, do that, otherwise only dump what user provided
            string[] codeArray;
            if (options.Code.Contains(UMT_DUMP_ALL))
                codeArray = program.Data.Code.Select(c => c.Name.Content).ToArray();
            else
                codeArray = options.Code;

            foreach (string code in codeArray)
                program.DumpCodeEntry(code);
        }

        // If user wanted to dump strings, dump all of them in a text file
        if (options.Strings)
            program.DumpAllStrings();

        // If user wanted to dump embedded textures, dump all of them
        if (options.Textures)
            program.DumpAllTextures();

        return EXIT_SUCCESS;
    }

    /// <summary>
    /// Method that gets executed on the "replace" command
    /// </summary>
    /// <param name="options">The arguments that have been provided with the "replace" command</param>
    /// <returns><see cref="EXIT_SUCCESS"/> and <see cref="EXIT_FAILURE"/> for being successful and failing respectively</returns>
    private static int Replace(ReplaceOptions options)
    {
        QiuFuncMain program;
        try
        {
            program = new QiuFuncMain(options.Datafile, null, options.Output, options.Verbose);
        }
        catch (FileNotFoundException e)
        {
            //Console.Error.WriteLine(e.Message);
            return EXIT_FAILURE;
        }

        // If user provided code to replace, replace them
        if ((options.Code?.Length > 0) && (program.Data.Code.Count > 0))
        {
            // get the values and put them into a dictionary for ease of use
            Dictionary<string, FileInfo> codeDict = new Dictionary<string, FileInfo>();
            foreach (string code in options.Code)
            {
                string[] splitText = code.Split('=');

                if (splitText.Length != 2)
                {
                    //Console.Error.WriteLine($"{code} is malformed! Should be of format 'name_of_code=./newCode.gml' instead!");
                    return EXIT_FAILURE;
                }

                codeDict.Add(splitText[0], new FileInfo(splitText[1]));
            }

            // If user wants to replace all, we'll be handling it differently. Replace every file from the provided directory
            if (codeDict.ContainsKey(UMT_REPLACE_ALL))
            {
                string directory = codeDict[UMT_REPLACE_ALL].FullName;
                foreach (FileInfo file in new DirectoryInfo(directory).GetFiles())
                    program.ReplaceCodeEntryWithFile(Path.GetFileNameWithoutExtension(file.Name), file);
            }
            // Otherwise, just replace every file which was given
            else
            {
                foreach (KeyValuePair<string, FileInfo> keyValue in codeDict)
                    program.ReplaceCodeEntryWithFile(keyValue.Key, keyValue.Value);
            }
        }

        // If user provided texture to replace, replace them
        if (options.Textures?.Length > 0)
        {
            // get the values and put them into a dictionary for ease of use
            Dictionary<string, FileInfo> textureDict = new Dictionary<string, FileInfo>();
            foreach (string texture in options.Textures)
            {
                string[] splitText = texture.Split('=');

                if (splitText.Length != 2)
                {
                    //Console.Error.WriteLine($"{texture} is malformed! Should be of format 'Name=./new.png' instead!");
                    return EXIT_FAILURE;
                }

                textureDict.Add(splitText[0], new FileInfo(splitText[1]));
            }

            // If user wants to replace all, we'll be handling it differently. Replace every file from the provided directory
            if (textureDict.ContainsKey(UMT_REPLACE_ALL))
            {
                string directory = textureDict[UMT_REPLACE_ALL].FullName;
                foreach (FileInfo file in new DirectoryInfo(directory).GetFiles())
                    program.ReplaceTextureWithFile(Path.GetFileNameWithoutExtension(file.Name), file);
            }
            // Otherwise, just replace every file which was given
            else
            {
                foreach ((string key, FileInfo value) in textureDict)
                    program.ReplaceTextureWithFile(key, value);
            }
        }

        // if parameter to save file was given, save the data file
        if (options.Output != null)
            program.SaveDataFile(options.Output.FullName);

        return EXIT_SUCCESS;
    }

    /// <summary>
    /// Runs the interactive menu indefinitely until user quits out of it.
    /// </summary>
    private void RunInteractiveMenu()
    {
        while (true)
        {
            GenoukaUI_WriteLine("Interactive Menu:");
            GenoukaUI_WriteLine("1 - Run script.");
            GenoukaUI_WriteLine("2 - Run C# string.");
            GenoukaUI_WriteLine("3 - Save and overwrite.");
            GenoukaUI_WriteLine("4 - Save to different place.");
            GenoukaUI_WriteLine("5 - Display quick info.");
            //TODO: add dumping and replacing options
            GenoukaUI_WriteLine("6 - Quit without saving.");

            Console.Write("Input, please: ");
            ConsoleKey k = Console.ReadKey().Key;

            switch (k)
            {
                // 1 - run script
                case ConsoleKey.NumPad1:
                case ConsoleKey.D1:
                {
                    Console.Write("File path (you can drag and drop)? ");
                    string path = RemoveQuotes(Console.ReadLine());
                    GenoukaUI_WriteLine("Trying to run script {0}", path);
                    try
                    {
                        RunCSharpFile(path);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    break;
                }

                // 2 - run c# string
                case ConsoleKey.NumPad2:
                case ConsoleKey.D2:
                {
                    Console.Write("C# code line? ");
                    string line = Console.ReadLine();
                    ScriptPath = null;
                    RunCSharpCode(line);
                    break;
                }

                // Save and overwrite data file
                case ConsoleKey.NumPad3:
                case ConsoleKey.D3:
                {
                    SaveDataFile(FilePath);
                    break;
                }

                // Save data file to different path
                case ConsoleKey.NumPad4:
                case ConsoleKey.D4:
                {
                    Console.Write("Where to save? ");
                    string path = RemoveQuotes(Console.ReadLine());
                    SaveDataFile(path);
                    break;
                }

                // Print out Quick Info
                case ConsoleKey.NumPad5:
                case ConsoleKey.D5:
                {
                    CliQuickInfo();
                    break;
                }

                // Quit
                case ConsoleKey.NumPad6:
                case ConsoleKey.D6:
                {
                    GenoukaUI_WriteLine(
                        "Are you SURE? You can press 'n' and save before the changes are gone forever!!!");
                    GenoukaUI_WriteLine("(Y/N)? ");
                    bool isInputYes = Console.ReadKey(false).Key == ConsoleKey.Y;
                    //GenoukaUI_WriteLine();
                    if (isInputYes) return;

                    break;
                }

                default:
                {
                    GenoukaUI_WriteLine("Unknown input. Try using the upper line of digits on your keyboard.");
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Prints some basic info about the loaded data file.
    /// </summary>
    private void CliQuickInfo()
    {
        GenoukaUI_WriteLine("Quick Information:");
        GenoukaUI_WriteLine("Project Name - {0}", Data.GeneralInfo.Name);
        GenoukaUI_WriteLine("Is GMS2 - {0}", Data.IsGameMaker2());
        GenoukaUI_WriteLine("Is YYC - {0}", Data.IsYYC());
        GenoukaUI_WriteLine("Bytecode version - {0}", Data.GeneralInfo.BytecodeVersion);
        GenoukaUI_WriteLine("Configuration name - {0}", Data.GeneralInfo.Config);

        GenoukaUI_WriteLine(
            $"{Data.Sounds.Count} Sounds, {Data.Sprites.Count} Sprites, {Data.Backgrounds.Count} Backgrounds");
        GenoukaUI_WriteLine($"{Data.Paths.Count} Paths, {Data.Scripts.Count} Scripts, {Data.Shaders.Count} Shaders");
        GenoukaUI_WriteLine(
            $"{Data.Fonts.Count} Fonts, {Data.Timelines.Count} Timelines, {Data.GameObjects.Count} Game Objects");
        GenoukaUI_WriteLine(
            $"{Data.Rooms.Count} Rooms, {Data.Extensions.Count} Extensions, {Data.TexturePageItems.Count} Texture Page Items");
        if (!Data.IsYYC())
        {
            GenoukaUI_WriteLine(
                $"{Data.Code.Count} Code Entries, {Data.Variables.Count} Variables, {Data.Functions.Count} Functions");
            var codeLocalsInfo = Data.CodeLocals is not null ? $"{Data.CodeLocals.Count} Code locals, " : "";
            GenoukaUI_WriteLine(
                $"{codeLocalsInfo}{Data.Strings.Count} Strings, {Data.EmbeddedTextures.Count} Embedded Textures");
        }
        else
        {
            GenoukaUI_WriteLine("Unknown amount of Code entries and Code locals");
        }

        GenoukaUI_WriteLine($"{Data.Strings.Count} Strings");
        GenoukaUI_WriteLine($"{Data.EmbeddedTextures.Count} Embedded Textures");
        GenoukaUI_WriteLine($"{Data.EmbeddedAudio.Count} Embedded Audio");

        if (IsInteractive) Pause();
    }

    public static string StringWriteln([StringSyntax("CompositeFormat")] string format, params object?[] args)
    {
        return string.Format(format, args) + "\n";
    }

    public string GetObjectDetails(object obj, int indentLevel = 0)
    {
        if (obj == null) return "";

        string info = "";
        string indent = new string(' ', indentLevel * 4); // 每层缩进4个空格

        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
        {
            object value = descriptor.GetValue(obj);
            string name = descriptor.Name;

            if (value == null)
            {
                info += $"{indent}{name} = null\n";
            }
            else if (value.GetType().IsPrimitive || value is string || value is DateTime)
            {
                info += $"{indent}{name} = {value}\n";
            }
            else if (value is IEnumerable enumerable && !(value is string))
            {
                info += $"{indent}{name} = [集合]\n";
                int index = 0;
                foreach (var item in enumerable)
                {
                    info += $"{indent}    [{index}] = {GetObjectDetails(item, indentLevel + 2)}";
                    index++;
                }
            }
            else
            {
                info += $"{indent}{name} = \n";
                info += GetObjectDetails(value, indentLevel + 1);
            }
        }

        return info;
    }

    public string getQuickInfo()
    {
        string info = "";
        info += StringWriteln("[基本信息]");
        info += StringWriteln("项目名称 - {0}", Data.GeneralInfo.Name);
        info += StringWriteln("GMS2项目 - {0}", Data.IsGameMaker2());
        info += StringWriteln("YYC代码 - {0}", Data.IsYYC());
        info += StringWriteln("字节码版本 - {0}", Data.GeneralInfo.BytecodeVersion);
        info += StringWriteln("配置名称 - {0}", Data.GeneralInfo.Config);
        info += StringWriteln("[区块信息]");
        info += StringWriteln($"{Data.Sounds.Count} 个声音, {Data.Sprites.Count} 个精灵, {Data.Backgrounds.Count} 个背景");
        info += StringWriteln($"{Data.Paths.Count} 个路径, {Data.Scripts.Count} 个脚本, {Data.Shaders.Count} 个着色器");
        info += StringWriteln($"{Data.Fonts.Count} 个字体, {Data.Timelines.Count} 个时间线, {Data.GameObjects.Count} 个游戏对象");
        info += StringWriteln(
            $"{Data.Rooms.Count} 个房间, {Data.Extensions.Count} 个扩展, {Data.TexturePageItems.Count} 个纹理页子项");
        if (!Data.IsYYC())
        {
            info += StringWriteln($"{Data.Code.Count} 个代码项, {Data.Variables.Count} 个变量, {Data.Functions.Count} 个函数");
            var codeLocalsInfo = Data.CodeLocals is not null ? $"{Data.CodeLocals.Count} 个本地变量, " : "";
            info += StringWriteln($"{codeLocalsInfo}{Data.Strings.Count} 个字符串, {Data.EmbeddedTextures.Count} 个嵌入式纹理");
        }
        else
        {
            info += StringWriteln("[警告]代码项和代码本地变量数目未知");
        }

        info += StringWriteln($"{Data.Strings.Count} 个字符串");
        info += StringWriteln($"{Data.EmbeddedTextures.Count} 个嵌入式纹理");
        info += StringWriteln($"{Data.EmbeddedAudio.Count} 个嵌入式音频");
        //info += StringWriteln("[完整信息]");
        // foreach(PropertyDescriptor descriptor in TypeDescriptor.GetProperties(Data))
        // {
        //     string name = descriptor.Name;
        //     object value = descriptor.GetValue(Data);
        //     info += StringWriteln("{0}={1}", name, value);
        // }
        //info += GetObjectDetails(Data, 2);
        return info;
    }

    /// <summary>
    /// Dumps a code entry from a data file.
    /// </summary>
    /// <param name="codeEntry">The code entry that should get dumped</param>
    private void DumpCodeEntry(string codeEntry)
    {
        UndertaleCode code = Data.Code.ByName(codeEntry);


        if (code == null)
        {
            //Console.Error.WriteLine($"Data file does not contain a code entry named {codeEntry}!");
            return;
        }

        string directory = $"{Output.FullName}/CodeEntries/";

        Directory.CreateDirectory(directory);

        if (Verbose)
            GenoukaUI_WriteLine($"Dumping {codeEntry}");

        File.WriteAllText($"{directory}/{codeEntry}.gml", GetDecompiledText(code));
    }

    /// <summary>
    /// Dumps all strings in a data file.
    /// </summary>
    public void DumpAllStrings()
    {
        string directory = Output.FullName;

        Directory.CreateDirectory(directory);

        StringBuilder combinedText = new StringBuilder();
        foreach (UndertaleString dataString in Data.Strings)
        {
            if (Verbose)
                GenoukaUI_WriteLine($"Added {dataString.Content}");
            combinedText.Append($"{dataString.Content}\n");
        }

        if (Verbose)
            GenoukaUI_WriteLine("Writing all strings to disk");
        File.WriteAllText($"{directory}/strings.txt", combinedText.ToString());
    }

    /// <summary>
    /// Dumps all embedded textures in a data file.
    /// </summary>
    public void DumpAllTextures()
    {
        string directory = $"{Output.FullName}/EmbeddedTextures/";

        Directory.CreateDirectory(directory);

        foreach (UndertaleEmbeddedTexture texture in Data.EmbeddedTextures)
        {
            if (Verbose)
                GenoukaUI_WriteLine($"Dumping {texture.Name}");
            if (texture.TextureData.Image is not GMImage image)
            {
                GenoukaUI_WriteLine($"{texture.Name} has no image assigned, skipping");
                continue;
            }

            using FileStream fs = new($"{directory}/{texture.Name.Content}.png", FileMode.Create);
            texture.TextureData.Image.SavePng(fs);
        }
    }

    /// <summary>
    /// Replaces a code entry with text from another file.
    /// </summary>
    /// <param name="codeEntry">The code entry to replace</param>
    /// <param name="fileToReplace">File path which should replace the code entry.</param>
    private void ReplaceCodeEntryWithFile(string codeEntry, FileInfo fileToReplace)
    {
        if (Verbose)
            GenoukaUI_WriteLine("Replacing " + codeEntry);

        // Read source code from file
        string gmlCode = File.ReadAllText(fileToReplace.FullName);

        // Link code to object events manually only if collision events are used
        CompileResult result = CompileResult.UnsuccessfulResult;
        bool manualLink = false;
        const string objectPrefix = "gml_Object_";
        if (codeEntry.StartsWith(objectPrefix, StringComparison.Ordinal))
        {
            // Parse object event. First, find positions of last two underscores in name.
            int lastUnderscore = codeEntry.LastIndexOf('_');
            int secondLastUnderscore = codeEntry.LastIndexOf('_', lastUnderscore - 1);
            if (lastUnderscore <= 0 || secondLastUnderscore <= 0)
            {
                //Console.Error.WriteLine($"Failed to parse object code entry name: \"{codeEntry}\"");
                return;
            }

            // Extract object name, event type, and event subtype
            ReadOnlySpan<char> objectName = codeEntry.AsSpan(new Range(objectPrefix.Length, secondLastUnderscore));
            ReadOnlySpan<char> eventType = codeEntry.AsSpan(new Range(secondLastUnderscore + 1, lastUnderscore));
            if (!uint.TryParse(codeEntry.AsSpan(lastUnderscore + 1), out uint eventSubtype))
            {
                // No number at the end of the name; parse it out as best as possible (may technically be ambiguous sometimes...).
                // It should be a collision event, though.
                manualLink = true;
                ReadOnlySpan<char> nameAfterPrefix = codeEntry.AsSpan(objectPrefix.Length);
                const string collisionSeparator = "_Collision_";
                int collisionSeparatorPos = nameAfterPrefix.LastIndexOf(collisionSeparator);
                if (collisionSeparatorPos != -1)
                {
                    // Split out the actual object name and the collision subtype
                    objectName = nameAfterPrefix[0..collisionSeparatorPos];
                    ReadOnlySpan<char> collisionSubtype =
                        nameAfterPrefix[(collisionSeparatorPos + collisionSeparator.Length)..];

                    if (Data.IsVersionAtLeast(2, 3))
                    {
                        // GameMaker 2.3+ uses the object name for the collision subtype
                        int objectIndex = Data.GameObjects.IndexOfName(collisionSubtype);
                        if (objectIndex >= 0)
                        {
                            // Object already exists; use its ID as a subtype
                            eventSubtype = (uint)objectIndex;
                        }
                        else
                        {
                            // Need to create a new object
                            eventSubtype = (uint)Data.GameObjects.Count;
                            Data.GameObjects.Add(new()
                            {
                                Name = Data.Strings.MakeString(collisionSubtype.ToString())
                            });
                        }
                    }
                    else
                    {
                        // Pre-2.3 GMS2 versions use GUIDs... need to resolve it
                        eventSubtype = ReduceCollisionValue(GetCollisionValueFromCodeNameGUID(codeEntry));
                        ReassignGUIDs(collisionSubtype.ToString(),
                            ReduceCollisionValue(GetCollisionValueFromCodeNameGUID(codeEntry)));
                    }
                }
                else
                {
                    //Console.Error.WriteLine($"Failed to parse event type and subtype for \"{codeEntry}\".");
                    return;
                }
            }
            else if (eventType.SequenceEqual("Collision"))
            {
                // Handle collision events with object ID at the end of the name
                manualLink = true;
                if (eventSubtype >= Data.GameObjects.Count)
                {
                    if (ScriptQuestion(
                            $"Object of ID {eventSubtype} was not found.\nAdd new object? (will be ID {Data.GameObjects.Count})"))
                    {
                        // Create new object at end of game object list
                        eventSubtype = (uint)Data.GameObjects.Count;
                        Data.GameObjects.Add(new()
                        {
                            Name = Data.Strings.MakeString(
                                SimpleTextInput("Enter object name", $"Enter object name for ID {eventSubtype}", "",
                                    false))
                        });
                    }
                    else
                    {
                        // It *needs* to have a valid value, make the user specify one
                        eventSubtype = ReduceCollisionValue([uint.MaxValue]);
                    }
                }
            }

            // If manually linking, do so
            if (manualLink)
            {
                // Create new object if necessary
                UndertaleGameObject obj = Data.GameObjects.ByName(objectName);
                if (obj is null)
                {
                    obj = new()
                    {
                        Name = Data.Strings.MakeString(objectName.ToString())
                    };
                    Data.GameObjects.Add(obj);
                }

                // Link to object's event with a blank code entry
                UndertaleCode manualCode = UndertaleCode.CreateEmptyEntry(Data, codeEntry);
                CodeImportGroup.LinkEvent(obj, manualCode, EventType.Collision, eventSubtype);

                // Perform code import using manual code entry
                CodeImportGroup group = new(Data);
                group.QueueReplace(manualCode, gmlCode);
                result = group.Import();
            }
        }

        // When not manually linking, just let a code import group do it during importing
        if (!manualLink)
        {
            CodeImportGroup group = new(Data);
            group.QueueReplace(codeEntry, gmlCode);
            result = group.Import();
        }

        // Error if import failed
        if (!result.Successful)
        {
            //Console.Error.WriteLine("Code import unsuccessful:\n" + result.PrintAllErrors(false));
        }
    }

    /// <summary>
    /// Replaces an embedded texture with contents from another file.
    /// </summary>
    /// <param name="textureEntry">Embedded texture to replace</param>
    /// <param name="fileToReplace">File path which should replace the embedded texture.</param>
    private void ReplaceTextureWithFile(string textureEntry, FileInfo fileToReplace)
    {
        UndertaleEmbeddedTexture texture = Data.EmbeddedTextures.ByName(textureEntry);

        if (texture == null)
        {
            //Console.Error.WriteLine($"Data file does not contain an embedded texture named {textureEntry}!");
            return;
        }

        if (Verbose)
            GenoukaUI_WriteLine("Replacing " + textureEntry);

        texture.TextureData.Image = GMImage.FromPng(File.ReadAllBytes(fileToReplace.FullName));
    }

    /// <summary>
    /// Evaluates and executes the contents of a file as C# Code.
    /// </summary>
    /// <param name="path">Path to file which contents to interpret as C# code</param>
    private void RunCSharpFile(string path)
    {
        string lines;
        try
        {
            lines = File.ReadAllText(path, Encoding.UTF8);
        }
        catch (Exception exc)
        {
            // rethrow as otherwise this will get interpreted as success
            //Console.Error.WriteLine(exc.Message);
            throw;
        }

        lines = $"#line 1 \"{path}\"\n" + lines;
        ScriptPath = path;
        RunCSharpCode(lines, ScriptPath);
    }

    public delegate void DelegateOutput(string line);

    public bool RunCSharpFilePublic(string path, DelegateOutput callback, Page page)
    {
        string lines;
        try
        {
            lines = File.ReadAllText(path, Encoding.UTF8);
        }
        catch (Exception exc)
        {
            // rethrow as otherwise this will get interpreted as success
            callback(exc.Message);
            return false;
        }

        lines = $"#line 1 \"{path}\"\n" + lines;
        ScriptPath = path;
        RunCSharpCodePublic2(callback, page, lines, ScriptPath);
        return true;
    }

    public void RunCSharpCodePublic2(DelegateOutput callback, Page page, string code, string scriptFile = null)
    {
        Genouka_callback = callback;
        //this.MAUI_Page = page;

        //静态预检查脚本可能存在的问题
        string lintString = "";
        if (code.Contains("System.Windows.Media"))
        {
            lintString += "* [严重]包含仅限Windows可用的System.Windows.Media，强行运行可能会崩溃\n";
        }

        if (code.Contains("System.Windows.Forms"))
        {
            lintString += "* [警告]包含仅限Windows可用的System.Windows.Forms，强行运行可能会崩溃\n";
        }

        if (code.Contains("UndertaleModTool."))
        {
            lintString += "* [严重]这个脚本只支持在UndertaleModTool GUI运行\n";
        }

        if (code.Contains("TextureWorker") && !code.Contains("TextureWorkerSkia"))
        {
            lintString += "* [警告]这个脚本引用了不兼容的TextureWorker，请手动在代码中替换为TextureWorkerSkia即可解决\n";
        }

        if (lintString != "")
        {
            callback("***********************\n"+lintString+"\n***********************\n");
            lintString += "发现该脚本存在以上问题，可能无法正常运行，是否要坚持运行？";
            if (!MAUIBridge.AskDialog("预检查问题", lintString).Result)
            {
                callback("用户手动取消了运行该脚本\n");
                return;
            }
        }

        if (Verbose)
            callback($"尝试执行 '{scriptFile ?? "代码段"}'...\n");

        var task = CSharpScript
            .EvaluateAsync(code, CliScriptOptions.WithFilePath(scriptFile ?? "").WithFileEncoding(Encoding.UTF8),
                this, typeof(IScriptInterface));
        try
        {
            task.GetAwaiter().GetResult();
            ScriptExecutionSuccess = true;
            ScriptErrorMessage = "";
        }
        catch (Exception exc)
        {
            if (cTokenSource != null) cTokenSource.Cancel();
            ScriptExecutionSuccess = false;
            ScriptErrorMessage = exc.ToString();
            ScriptErrorType = "Exception";
        }

        if (!FinishedMessageEnabled) return;

        if (ScriptExecutionSuccess)
        {
            if (Verbose)
                callback($"执行 '{scriptFile ?? "代码段"}' 完毕");
        }
        else
        {
            callback(ScriptErrorMessage);
        }
    }

    /// <summary>
    /// Evaluates and executes given C# code.
    /// </summary>
    /// <param name="code">The C# string to execute</param>
    /// <param name="scriptFile">The path to the script file where <paramref name="code"/> was loaded from.
    /// Leave as null, if it wasn't executed from a script file.</param>
    private void RunCSharpCode(string code, string scriptFile = null)
    {
        if (Verbose)
            GenoukaUI_WriteLine($"Attempting to execute '1{scriptFile ?? code}'...");

        try
        {
            CSharpScript
                .EvaluateAsync(code, CliScriptOptions.WithFilePath(scriptFile ?? "").WithFileEncoding(Encoding.UTF8),
                    this, typeof(IScriptInterface)).GetAwaiter().GetResult();
            ScriptExecutionSuccess = true;
            ScriptErrorMessage = "";
        }
        catch (Exception exc)
        {
            ScriptExecutionSuccess = false;
            ScriptErrorMessage = exc.ToString();
            ScriptErrorType = "Exception";
        }

        if (!FinishedMessageEnabled) return;

        if (ScriptExecutionSuccess)
        {
            if (Verbose)
                GenoukaUI_WriteLine($"Finished executing '{scriptFile ?? code}'");
        }
        else
        {
            //Console.Error.WriteLine(ScriptErrorMessage);
        }
    }

    /// <summary>
    /// Saves the currently loaded <see cref="Data"/> to an output path.
    /// </summary>
    /// <param name="outputPath">The path where to save the data.</param>
    /// <exception cref="IOException">If saving fails</exception>
    public void SaveDataFile(string outputPath)
    {
        if (Verbose)
            GenoukaUI_WriteLine($"Saving new data file to '{outputPath}'");
        try
        {
            // Save data.win to temp file
            using (FileStream fs = new(outputPath + "temp", FileMode.Create, FileAccess.Write))
            {
                UndertaleIO.Write(fs, Data, MessageHandler);
            }

            // If we're executing this, the saving was successful. So we can replace the new temp file
            // with the older file, if it exists.
            File.Move(outputPath + "temp", outputPath, true);

            if (Verbose)
                GenoukaUI_WriteLine($"Saved data file to '{outputPath}'");
        }
        catch (Exception e)
        {
            // Delete the temporary file in case we partially wrote it
            if (File.Exists(outputPath + "temp"))
                File.Delete(outputPath + "temp");
            throw new IOException($"Could not save data file: {e.Message}");
        }
    }

    /// <summary>
    /// Read supplied filename and return the data file.
    /// </summary>
    /// <param name="datafile">The datafile to read</param>
    /// <param name="warningHandler">Handler for warnings</param>
    /// <param name="messageHandler">Handler for messages</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">If the data file cannot be found</exception>
    private static UndertaleData ReadDataFile(FileInfo datafile, WarningHandlerDelegate warningHandler = null,
        MessageHandlerDelegate messageHandler = null)
    {
        try
        {
            using FileStream fs = datafile.OpenRead();
            UndertaleData gmData = UndertaleIO.Read(fs, warningHandler, messageHandler);
            return gmData;
        }
        catch (FileNotFoundException e)
        {
            throw new FileNotFoundException($"Data file '{e.FileName}' does not exist");
        }
    }

    // need this on Windows when drag and dropping files.
    /// <summary>
    /// Trims <c>"</c> or <c>'</c> from the beginning and end of a string.
    /// </summary>
    /// <param name="s"><see cref="String"/> to remove <c>"</c> and/or <c>'</c> from</param>
    /// <returns>A new <see cref="String"/> that can be directly passed onto a FileInfo Constructor</returns>
    //TODO: needs some proper testing on how it behaves on Linux/MacOS and might need to get expanded
    private static string RemoveQuotes(string s)

    {
        return s.Trim('"', '\'');
    }

    /// <summary>
    /// Replicated the CMD Pause command. Waits for any key to be pressed before continuing.
    /// </summary>
    private static void Pause()
    {
        //Console.Write("Press any key to continue . . . ");
        //Console.ReadKey(true);
    }

    /// <summary>
    /// A simple warning handler that prints warnings to console.
    /// </summary>
    /// <param name="warning">The warning to print</param>
    /// <param name="isImportant">Whether the warning is important (may lead to data corruption)</param>
    private static void WarningHandler(string warning, bool isImportant) =>
        GenoukaUI_WriteLine($"[WARNING]: {warning}");

    /// <summary>
    /// A simple message handler that prints messages to console.
    /// </summary>
    /// <param name="message">The message to print</param>
    private static void MessageHandler(string message) => GenoukaUI_WriteLine($"[MESSAGE]: {message}");

    /// <summary>
    /// A dummy handler that does nothing.
    /// </summary>
    /// <param name="message">Not used.</param>
    private static void DummyHandler(string message)
    {
    }

    //TODO: document these as well
    private void ProgressUpdater()
    {
        DateTime prevTime = default;
        int prevValue = 0;

        while (true)
        {
            if (cToken.IsCancellationRequested)
            {
                if (prevValue >= progressValue) //if reached maximum
                    return;

                if (prevTime == default)
                    prevTime = DateTime.UtcNow; //begin measuring
                else if (DateTime.UtcNow.Subtract(prevTime).TotalMilliseconds >= 500) //timeout - 0.5 seconds
                    return;
            }

            UpdateProgressValue(progressValue);

            prevValue = progressValue;

            Thread.Sleep(100); //10 times per second
        }
    }

    private static void GenoukaUI_WriteLine([StringSyntax("CompositeFormat")] string format, object? arg0 = null)
    {
        var thing = String.Format(format, arg0);
        if (Genouka_callback is not null) Genouka_callback(thing + "\n");
        else Debug.WriteLine(thing);
    }

    private static void GenoukaUI_Write([StringSyntax("CompositeFormat")] string format, object? arg0 = null)
    {
        var thing = String.Format(format, arg0);
        if (Genouka_callback is not null) Genouka_callback(thing);
        else Debug.Write(thing);
    }

    public static void clearCallbacks()
    {
        Genouka_callback = null;
    }
}