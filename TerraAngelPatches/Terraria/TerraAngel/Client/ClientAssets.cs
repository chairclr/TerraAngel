using System;
using System.Collections.Generic;
using System.Linq;

namespace TerraAngel.Client;

public class ClientAssets
{
    private static Dictionary<float, ImFontPtr> TerrariaFonts = new Dictionary<float, ImFontPtr>();
    private static Dictionary<float, ImFontPtr> MonospaceFonts = new Dictionary<float, ImFontPtr>();

    public static readonly float[] DefaultSizes = new float[] { 14f, 16f, 18f, 22f };

    public static string TerrariaFontName => $"{ClientLoader.AssetPath}/AndyBold.ttf";
    public static string MonoFontName => $"{ClientLoader.AssetPath}/FiraCode.ttf";
    public static string IconFontName => $"{ClientLoader.AssetPath}/IconFont.ttf";
    public static string FallbackFontName => $"{ClientLoader.AssetPath}/Symbola.ttf";

    public static void LoadFonts(ImGuiIOPtr io)
    {
        for (int i = 0; i < DefaultSizes.Length; i++)
        {
            LoadMonospaceFont(DefaultSizes[i]);
            LoadTerrariaFont(DefaultSizes[i]);
        }
    }

    public static void LoadTerrariaFont(float size, bool withoutSymbols = false)
    {
        TerrariaFonts.Add(size, LoadFont(TerrariaFontName, size, 0x0020, 0x007F));

        LoadFont(FallbackFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f, 0x0080, 0x00FF, 0x0400, 0x04FF, 0x2320, 0x2330, 0x2000, 0x2020);

        if (!withoutSymbols)
        {
            LoadFont(IconFontName, size, true, new Vector2(0f, 4f), Icon.IconMin, Icon.IconMax);
        }
    }
    public static void LoadMonospaceFont(float size, bool withoutSymbols = false)
    {
        MonospaceFonts.Add(size, LoadFont(MonoFontName, size, 0x0020, 0x00FF, 0x0400, 0x04FF, 0x2020, 0x22FF));
        LoadFont(FallbackFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f, 0x2320, 0x2330, 0x2000, 0x2020);

        if (!withoutSymbols)
        {
            LoadFont(IconFontName, size, true, new Vector2(0f, 4f), Icon.IconMin, Icon.IconMax);
        }
    }

    public unsafe static ImFontPtr LoadFont(string path, float size)
    {
        ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();
        ImFontPtr font = LoadFont(path, size, false, config.GlyphOffset, config.GlyphExtraSpacing, config.GlyphMinAdvanceX, config.GlyphMaxAdvanceX, config.OversampleH, config.OversampleV, config.PixelSnapH, config.RasterizerMultiply, 0);
        config.Destroy();
        return font;
    }
    public unsafe static ImFontPtr LoadFont(string path, float size, params ushort[] glpyhRanges)
    {
        ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();
        ImFontPtr font = LoadFont(path, size, false, config.GlyphOffset, config.GlyphExtraSpacing, config.GlyphMinAdvanceX, config.GlyphMaxAdvanceX, config.OversampleH, config.OversampleV, config.PixelSnapH, config.RasterizerMultiply, glpyhRanges);
        config.Destroy();
        return font;
    }
    public unsafe static ImFontPtr LoadFont(string path, float size, bool merge, params ushort[] glpyhRanges)
    {
        ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();
        ImFontPtr font = LoadFont(path, size, merge, config.GlyphOffset, config.GlyphExtraSpacing, config.GlyphMinAdvanceX, config.GlyphMaxAdvanceX, config.OversampleH, config.OversampleV, config.PixelSnapH, config.RasterizerMultiply, glpyhRanges);
        config.Destroy();
        return font;
    }
    public unsafe static ImFontPtr LoadFont(string path, float size, bool merge, Vector2 glyphOffset, params ushort[] glpyhRanges)
    {
        ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();
        ImFontPtr font = LoadFont(path, size, merge, glyphOffset, config.GlyphExtraSpacing, config.GlyphMinAdvanceX, config.GlyphMaxAdvanceX, config.OversampleH, config.OversampleV, config.PixelSnapH, config.RasterizerMultiply, glpyhRanges);
        config.Destroy();
        return font;
    }
    public unsafe static ImFontPtr LoadFont(string path, float size, bool merge, Vector2 glyphOffset, Vector2 glyphExtraSpacing, params ushort[] glpyhRanges)
    {
        ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();
        ImFontPtr font = LoadFont(path, size, merge, glyphOffset, glyphExtraSpacing, config.GlyphMinAdvanceX, config.GlyphMaxAdvanceX, config.OversampleH, config.OversampleV, config.PixelSnapH, config.RasterizerMultiply, glpyhRanges);
        config.Destroy();
        return font;
    }
    public unsafe static ImFontPtr LoadFont(string path, float size, bool merge, Vector2 glyphOffset, Vector2 glyphExtraSpacing, float minAdvanceX, float maxAdvanceX, params ushort[] glpyhRanges)
    {
        ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();
        ImFontPtr font = LoadFont(path, size, merge, glyphOffset, glyphExtraSpacing, minAdvanceX, maxAdvanceX, config.OversampleH, config.OversampleV, config.PixelSnapH, config.RasterizerMultiply, glpyhRanges);
        config.Destroy();
        return font;
    }
    public unsafe static ImFontPtr LoadFont(string path, float size, bool merge, Vector2 glyphOffset, Vector2 glyphExtraSpacing, float minAdvanceX, float maxAdvanceX, float rasterizerMultiply, params ushort[] glpyhRanges)
    {
        ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();
        ImFontPtr font = LoadFont(path, size, merge, glyphOffset, glyphExtraSpacing, minAdvanceX, maxAdvanceX, config.OversampleH, config.OversampleV, config.PixelSnapH, rasterizerMultiply, glpyhRanges);
        config.Destroy();
        return font;
    }
    public unsafe static ImFontPtr LoadFont(string path, float size, bool merge, Vector2 glyphOffset, Vector2 glyphExtraSpacing, float minAdvanceX, float maxAdvanceX, int overSampleH, int overSampleV, bool pixelSnapH, float rasterizerMultiply, params ushort[] glyphRanges)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        if (glyphRanges.Length > 0)
        {

            glyphRanges = glyphRanges.Append((ushort)0).ToArray();

            fixed (ushort* glpyhRangesPtr = &glyphRanges[0])
            {
                ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();

                config.MergeMode = merge;
                config.GlyphOffset = glyphOffset;
                config.GlyphExtraSpacing = glyphExtraSpacing;
                config.GlyphMinAdvanceX = minAdvanceX;
                config.GlyphMaxAdvanceX = maxAdvanceX;
                config.OversampleH = overSampleH;
                config.OversampleV = overSampleV;
                config.PixelSnapH = pixelSnapH;
                config.RasterizerMultiply = rasterizerMultiply;

                ImFontPtr font = io.Fonts.AddFontFromFileTTF(path, size, config, (IntPtr)glpyhRangesPtr);
                config.Destroy();
                return font;
            }
        }
        else
        {
            ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();

            config.MergeMode = merge;
            config.GlyphOffset = glyphOffset;
            config.GlyphExtraSpacing = glyphExtraSpacing;
            config.GlyphMinAdvanceX = minAdvanceX;
            config.GlyphMaxAdvanceX = maxAdvanceX;
            config.OversampleH = overSampleH;
            config.OversampleV = overSampleV;
            config.PixelSnapH = pixelSnapH;
            config.RasterizerMultiply = rasterizerMultiply;

            ImFontPtr font = io.Fonts.AddFontFromFileTTF(path, size, config);
            config.Destroy();
            return font;
        }
    }


    public static ImFontPtr GetMonospaceFont(float size)
    {
        if (MonospaceFonts.ContainsKey(size))
            return MonospaceFonts[size];

        float closestFontSize = 16f;
        float closestDistance = float.MaxValue;
        foreach (float loadedFontSize in MonospaceFonts.Keys)
        {
            float dist = MathHelper.Distance(size, loadedFontSize);

            if (dist < closestDistance)
            {
                closestFontSize = loadedFontSize;
                closestDistance = dist;
            }
        }

        ClientLoader.MainRenderer?.EnqueuePreDrawAction(() => { LoadMonospaceFont(size); ClientLoader.MainRenderer?.RebuildFontAtlas(); });

        return MonospaceFonts[closestFontSize];
    }
    public static ImFontPtr GetTerrariaFont(float size)
    {

        if (TerrariaFonts.ContainsKey(size))
            return TerrariaFonts[size];

        float closestFontSize = 16f;
        float closestDistance = float.MaxValue;
        foreach (float loadedFontSize in TerrariaFonts.Keys)
        {
            float dist = MathHelper.Distance(size, loadedFontSize);

            if (dist < closestDistance)
            {
                closestFontSize = loadedFontSize;
                closestDistance = dist;
            }
        }

        ClientLoader.MainRenderer?.EnqueuePreDrawAction(() => { LoadTerrariaFont(size); ClientLoader.MainRenderer?.RebuildFontAtlas(); });

        return TerrariaFonts[closestFontSize];
    }

}
public static class Icon
{
    public static readonly ushort IconMin = 0xea60;
    public static readonly ushort IconMax = 0xec0c;

    public static readonly string Account = "\ueb99";
    public static readonly string ActivateBreakpoints = "\uea97";
    public static readonly string Add = "\uea60";
    public static readonly string Archive = "\uea98";
    public static readonly string ArrowBoth = "\uea99";
    public static readonly string ArrowCircleDown = "\uebfc";
    public static readonly string ArrowCircleLeft = "\uebfd";
    public static readonly string ArrowCircleRight = "\uebfe";
    public static readonly string ArrowCircleUp = "\uebff";
    public static readonly string ArrowDown = "\uea9a";
    public static readonly string ArrowLeft = "\uea9b";
    public static readonly string ArrowRight = "\uea9c";
    public static readonly string ArrowSmallDown = "\uea9d";
    public static readonly string ArrowSmallLeft = "\uea9e";
    public static readonly string ArrowSmallRight = "\uea9f";
    public static readonly string ArrowSmallUp = "\ueaa0";
    public static readonly string ArrowSwap = "\uebcb";
    public static readonly string ArrowUp = "\ueaa1";
    public static readonly string AzureDevops = "\uebe8";
    public static readonly string Azure = "\uebd8";
    public static readonly string BeakerStop = "\uebe1";
    public static readonly string Beaker = "\uea79";
    public static readonly string BellDot = "\ueb9a";
    public static readonly string BellSlashDot = "\uec09";
    public static readonly string BellSlash = "\uec08";
    public static readonly string Bell = "\ueaa2";
    public static readonly string Blank = "\uec03";
    public static readonly string Bold = "\ueaa3";
    public static readonly string Book = "\ueaa4";
    public static readonly string Bookmark = "\ueaa5";
    public static readonly string BracketDot = "\uebe5";
    public static readonly string BracketError = "\uebe6";
    public static readonly string Briefcase = "\ueaac";
    public static readonly string Broadcast = "\ueaad";
    public static readonly string Browser = "\ueaae";
    public static readonly string Bug = "\ueaaf";
    public static readonly string Calendar = "\ueab0";
    public static readonly string CallIncoming = "\ueb92";
    public static readonly string CallOutgoing = "\ueb93";
    public static readonly string CaseSensitive = "\ueab1";
    public static readonly string CheckAll = "\uebb1";
    public static readonly string Check = "\ueab2";
    public static readonly string Checklist = "\ueab3";
    public static readonly string ChevronDown = "\ueab4";
    public static readonly string ChevronLeft = "\ueab5";
    public static readonly string ChevronRight = "\ueab6";
    public static readonly string ChevronUp = "\ueab7";
    public static readonly string ChromeClose = "\ueab8";
    public static readonly string ChromeMaximize = "\ueab9";
    public static readonly string ChromeMinimize = "\ueaba";
    public static readonly string ChromeRestore = "\ueabb";
    public static readonly string CircleFilled = "\uea71";
    public static readonly string CircleLargeFilled = "\uebb4";
    public static readonly string CircleLargeOutline = "\uebb5";
    public static readonly string CircleOutline = "\ueabc";
    public static readonly string CircleSlash = "\ueabd";
    public static readonly string CircleSmallFilled = "\ueb8a";
    public static readonly string CircleSmall = "\uec07";
    public static readonly string CircuitBoard = "\ueabe";
    public static readonly string ClearAll = "\ueabf";
    public static readonly string Clippy = "\ueac0";
    public static readonly string CloseAll = "\ueac1";
    public static readonly string Close = "\uea76";
    public static readonly string CloudDownload = "\ueac2";
    public static readonly string CloudUpload = "\ueac3";
    public static readonly string Cloud = "\uebaa";
    public static readonly string Code = "\ueac4";
    public static readonly string CollapseAll = "\ueac5";
    public static readonly string ColorMode = "\ueac6";
    public static readonly string Combine = "\uebb6";
    public static readonly string CommentDiscussion = "\ueac7";
    public static readonly string CommentUnresolved = "\uec0a";
    public static readonly string Comment = "\uea6b";
    public static readonly string CompassActive = "\uebd7";
    public static readonly string CompassDot = "\uebd6";
    public static readonly string Compass = "\uebd5";
    public static readonly string Copy = "\uebcc";
    public static readonly string CreditCard = "\ueac9";
    public static readonly string Dash = "\ueacc";
    public static readonly string Dashboard = "\ueacd";
    public static readonly string Database = "\ueace";
    public static readonly string DebugAll = "\uebdc";
    public static readonly string DebugAltSmall = "\ueba8";
    public static readonly string DebugAlt = "\ueb91";
    public static readonly string DebugBreakpointConditionalUnverified = "\ueaa6";
    public static readonly string DebugBreakpointConditional = "\ueaa7";
    public static readonly string DebugBreakpointDataUnverified = "\ueaa8";
    public static readonly string DebugBreakpointData = "\ueaa9";
    public static readonly string DebugBreakpointFunctionUnverified = "\ueb87";
    public static readonly string DebugBreakpointFunction = "\ueb88";
    public static readonly string DebugBreakpointLogUnverified = "\ueaaa";
    public static readonly string DebugBreakpointLog = "\ueaab";
    public static readonly string DebugBreakpointUnsupported = "\ueb8c";
    public static readonly string DebugConsole = "\ueb9b";
    public static readonly string DebugContinueSmall = "\uebe0";
    public static readonly string DebugContinue = "\ueacf";
    public static readonly string DebugCoverage = "\uebdd";
    public static readonly string DebugDisconnect = "\uead0";
    public static readonly string DebugLineByLine = "\uebd0";
    public static readonly string DebugPause = "\uead1";
    public static readonly string DebugRerun = "\uebc0";
    public static readonly string DebugRestartFrame = "\ueb90";
    public static readonly string DebugRestart = "\uead2";
    public static readonly string DebugReverseContinue = "\ueb8e";
    public static readonly string DebugStackframeActive = "\ueb89";
    public static readonly string DebugStackframe = "\ueb8b";
    public static readonly string DebugStart = "\uead3";
    public static readonly string DebugStepBack = "\ueb8f";
    public static readonly string DebugStepInto = "\uead4";
    public static readonly string DebugStepOut = "\uead5";
    public static readonly string DebugStepOver = "\uead6";
    public static readonly string DebugStop = "\uead7";
    public static readonly string Debug = "\uead8";
    public static readonly string DesktopDownload = "\uea78";
    public static readonly string DeviceCameraVideo = "\uead9";
    public static readonly string DeviceCamera = "\ueada";
    public static readonly string DeviceMobile = "\ueadb";
    public static readonly string DiffAdded = "\ueadc";
    public static readonly string DiffIgnored = "\ueadd";
    public static readonly string DiffModified = "\ueade";
    public static readonly string DiffRemoved = "\ueadf";
    public static readonly string DiffRenamed = "\ueae0";
    public static readonly string Diff = "\ueae1";
    public static readonly string Discard = "\ueae2";
    public static readonly string Edit = "\uea73";
    public static readonly string EditorLayout = "\ueae3";
    public static readonly string Ellipsis = "\uea7c";
    public static readonly string EmptyWindow = "\ueae4";
    public static readonly string ErrorSmall = "\uebfb";
    public static readonly string Error = "\uea87";
    public static readonly string Exclude = "\ueae5";
    public static readonly string ExpandAll = "\ueb95";
    public static readonly string Export = "\uebac";
    public static readonly string Extensions = "\ueae6";
    public static readonly string EyeClosed = "\ueae7";
    public static readonly string Eye = "\uea70";
    public static readonly string Feedback = "\ueb96";
    public static readonly string FileBinary = "\ueae8";
    public static readonly string FileCode = "\ueae9";
    public static readonly string FileMedia = "\ueaea";
    public static readonly string FilePdf = "\ueaeb";
    public static readonly string FileSubmodule = "\ueaec";
    public static readonly string FileSymlinkDirectory = "\ueaed";
    public static readonly string FileSymlinkFile = "\ueaee";
    public static readonly string FileZip = "\ueaef";
    public static readonly string File = "\uea7b";
    public static readonly string Files = "\ueaf0";
    public static readonly string FilterFilled = "\uebce";
    public static readonly string Filter = "\ueaf1";
    public static readonly string Flame = "\ueaf2";
    public static readonly string FoldDown = "\ueaf3";
    public static readonly string FoldUp = "\ueaf4";
    public static readonly string Fold = "\ueaf5";
    public static readonly string FolderActive = "\ueaf6";
    public static readonly string FolderLibrary = "\uebdf";
    public static readonly string FolderOpened = "\ueaf7";
    public static readonly string Folder = "\uea83";
    public static readonly string Gear = "\ueaf8";
    public static readonly string Gift = "\ueaf9";
    public static readonly string GistSecret = "\ueafa";
    public static readonly string GitCommit = "\ueafc";
    public static readonly string GitCompare = "\ueafd";
    public static readonly string GitMerge = "\ueafe";
    public static readonly string GitPullRequestClosed = "\uebda";
    public static readonly string GitPullRequestCreate = "\uebbc";
    public static readonly string GitPullRequestDraft = "\uebdb";
    public static readonly string GitPullRequestGoToChanges = "\uec0b";
    public static readonly string GitPullRequestNewChanges = "\uec0c";
    public static readonly string GitPullRequest = "\uea64";
    public static readonly string GithubAction = "\ueaff";
    public static readonly string GithubAlt = "\ueb00";
    public static readonly string GithubInverted = "\ueba1";
    public static readonly string Github = "\uea84";
    public static readonly string Globe = "\ueb01";
    public static readonly string GoToFile = "\uea94";
    public static readonly string Grabber = "\ueb02";
    public static readonly string GraphLeft = "\uebad";
    public static readonly string GraphLine = "\uebe2";
    public static readonly string GraphScatter = "\uebe3";
    public static readonly string Graph = "\ueb03";
    public static readonly string Gripper = "\ueb04";
    public static readonly string GroupByRefType = "\ueb97";
    public static readonly string HeartFilled = "\uec04";
    public static readonly string Heart = "\ueb05";
    public static readonly string History = "\uea82";
    public static readonly string Home = "\ueb06";
    public static readonly string HorizontalRule = "\ueb07";
    public static readonly string Hubot = "\ueb08";
    public static readonly string Inbox = "\ueb09";
    public static readonly string Indent = "\uebf9";
    public static readonly string Info = "\uea74";
    public static readonly string Inspect = "\uebd1";
    public static readonly string IssueDraft = "\uebd9";
    public static readonly string IssueReopened = "\ueb0b";
    public static readonly string Issues = "\ueb0c";
    public static readonly string Italic = "\ueb0d";
    public static readonly string Jersey = "\ueb0e";
    public static readonly string Json = "\ueb0f";
    public static readonly string KebabVertical = "\ueb10";
    public static readonly string Key = "\ueb11";
    public static readonly string Law = "\ueb12";
    public static readonly string LayersActive = "\uebd4";
    public static readonly string LayersDot = "\uebd3";
    public static readonly string Layers = "\uebd2";
    public static readonly string LayoutActivitybarLeft = "\uebec";
    public static readonly string LayoutActivitybarRight = "\uebed";
    public static readonly string LayoutCentered = "\uebf7";
    public static readonly string LayoutMenubar = "\uebf6";
    public static readonly string LayoutPanelCenter = "\uebef";
    public static readonly string LayoutPanelJustify = "\uebf0";
    public static readonly string LayoutPanelLeft = "\uebee";
    public static readonly string LayoutPanelOff = "\uec01";
    public static readonly string LayoutPanelRight = "\uebf1";
    public static readonly string LayoutPanel = "\uebf2";
    public static readonly string LayoutSidebarLeftOff = "\uec02";
    public static readonly string LayoutSidebarLeft = "\uebf3";
    public static readonly string LayoutSidebarRightOff = "\uec00";
    public static readonly string LayoutSidebarRight = "\uebf4";
    public static readonly string LayoutStatusbar = "\uebf5";
    public static readonly string Layout = "\uebeb";
    public static readonly string Library = "\ueb9c";
    public static readonly string LightbulbAutofix = "\ueb13";
    public static readonly string Lightbulb = "\uea61";
    public static readonly string LinkExternal = "\ueb14";
    public static readonly string Link = "\ueb15";
    public static readonly string ListFilter = "\ueb83";
    public static readonly string ListFlat = "\ueb84";
    public static readonly string ListOrdered = "\ueb16";
    public static readonly string ListSelection = "\ueb85";
    public static readonly string ListTree = "\ueb86";
    public static readonly string ListUnordered = "\ueb17";
    public static readonly string LiveShare = "\ueb18";
    public static readonly string Loading = "\ueb19";
    public static readonly string Location = "\ueb1a";
    public static readonly string LockSmall = "\uebe7";
    public static readonly string Lock = "\uea75";
    public static readonly string Magnet = "\uebae";
    public static readonly string MailRead = "\ueb1b";
    public static readonly string Mail = "\ueb1c";
    public static readonly string MapFilled = "\uec06";
    public static readonly string Map = "\uec05";
    public static readonly string Markdown = "\ueb1d";
    public static readonly string Megaphone = "\ueb1e";
    public static readonly string Mention = "\ueb1f";
    public static readonly string Menu = "\ueb94";
    public static readonly string Merge = "\uebab";
    public static readonly string Milestone = "\ueb20";
    public static readonly string Mirror = "\uea69";
    public static readonly string MortarBoard = "\ueb21";
    public static readonly string Move = "\ueb22";
    public static readonly string MultipleWindows = "\ueb23";
    public static readonly string Mute = "\ueb24";
    public static readonly string NewFile = "\uea7f";
    public static readonly string NewFolder = "\uea80";
    public static readonly string Newline = "\uebea";
    public static readonly string NoNewline = "\ueb25";
    public static readonly string Note = "\ueb26";
    public static readonly string NotebookTemplate = "\uebbf";
    public static readonly string Notebook = "\uebaf";
    public static readonly string Octoface = "\ueb27";
    public static readonly string OpenPreview = "\ueb28";
    public static readonly string Organization = "\uea7e";
    public static readonly string Output = "\ueb9d";
    public static readonly string Package = "\ueb29";
    public static readonly string Paintcan = "\ueb2a";
    public static readonly string PassFilled = "\uebb3";
    public static readonly string Pass = "\ueba4";
    public static readonly string PersonAdd = "\uebcd";
    public static readonly string Person = "\uea67";
    public static readonly string PieChart = "\uebe4";
    public static readonly string Pin = "\ueb2b";
    public static readonly string PinnedDirty = "\uebb2";
    public static readonly string Pinned = "\ueba0";
    public static readonly string PlayCircle = "\ueba6";
    public static readonly string Play = "\ueb2c";
    public static readonly string Plug = "\ueb2d";
    public static readonly string PreserveCase = "\ueb2e";
    public static readonly string Preview = "\ueb2f";
    public static readonly string PrimitiveSquare = "\uea72";
    public static readonly string Project = "\ueb30";
    public static readonly string Pulse = "\ueb31";
    public static readonly string Question = "\ueb32";
    public static readonly string Quote = "\ueb33";
    public static readonly string RadioTower = "\ueb34";
    public static readonly string Reactions = "\ueb35";
    public static readonly string RecordKeys = "\uea65";
    public static readonly string RecordSmall = "\uebfa";
    public static readonly string Record = "\ueba7";
    public static readonly string Redo = "\uebb0";
    public static readonly string References = "\ueb36";
    public static readonly string Refresh = "\ueb37";
    public static readonly string Regex = "\ueb38";
    public static readonly string RemoteExplorer = "\ueb39";
    public static readonly string Remote = "\ueb3a";
    public static readonly string Remove = "\ueb3b";
    public static readonly string ReplaceAll = "\ueb3c";
    public static readonly string Replace = "\ueb3d";
    public static readonly string Reply = "\uea7d";
    public static readonly string RepoClone = "\ueb3e";
    public static readonly string RepoForcePush = "\ueb3f";
    public static readonly string RepoForked = "\uea63";
    public static readonly string RepoPull = "\ueb40";
    public static readonly string RepoPush = "\ueb41";
    public static readonly string Repo = "\uea62";
    public static readonly string Report = "\ueb42";
    public static readonly string RequestChanges = "\ueb43";
    public static readonly string Rocket = "\ueb44";
    public static readonly string RootFolderOpened = "\ueb45";
    public static readonly string RootFolder = "\ueb46";
    public static readonly string Rss = "\ueb47";
    public static readonly string Ruby = "\ueb48";
    public static readonly string RunAbove = "\uebbd";
    public static readonly string RunAll = "\ueb9e";
    public static readonly string RunBelow = "\uebbe";
    public static readonly string RunErrors = "\uebde";
    public static readonly string SaveAll = "\ueb49";
    public static readonly string SaveAs = "\ueb4a";
    public static readonly string Save = "\ueb4b";
    public static readonly string ScreenFull = "\ueb4c";
    public static readonly string ScreenNormal = "\ueb4d";
    public static readonly string SearchStop = "\ueb4e";
    public static readonly string Search = "\uea6d";
    public static readonly string ServerEnvironment = "\ueba3";
    public static readonly string ServerProcess = "\ueba2";
    public static readonly string Server = "\ueb50";
    public static readonly string SettingsGear = "\ueb51";
    public static readonly string Settings = "\ueb52";
    public static readonly string Shield = "\ueb53";
    public static readonly string SignIn = "\uea6f";
    public static readonly string SignOut = "\uea6e";
    public static readonly string Smiley = "\ueb54";
    public static readonly string SortPrecedence = "\ueb55";
    public static readonly string SourceControl = "\uea68";
    public static readonly string SplitHorizontal = "\ueb56";
    public static readonly string SplitVertical = "\ueb57";
    public static readonly string Squirrel = "\ueb58";
    public static readonly string StarEmpty = "\uea6a";
    public static readonly string StarFull = "\ueb59";
    public static readonly string StarHalf = "\ueb5a";
    public static readonly string StopCircle = "\ueba5";
    public static readonly string SymbolArray = "\uea8a";
    public static readonly string SymbolBoolean = "\uea8f";
    public static readonly string SymbolClass = "\ueb5b";
    public static readonly string SymbolColor = "\ueb5c";
    public static readonly string SymbolConstant = "\ueb5d";
    public static readonly string SymbolEnumMember = "\ueb5e";
    public static readonly string SymbolEnum = "\uea95";
    public static readonly string SymbolEvent = "\uea86";
    public static readonly string SymbolField = "\ueb5f";
    public static readonly string SymbolFile = "\ueb60";
    public static readonly string SymbolInterface = "\ueb61";
    public static readonly string SymbolKey = "\uea93";
    public static readonly string SymbolKeyword = "\ueb62";
    public static readonly string SymbolMethod = "\uea8c";
    public static readonly string SymbolMisc = "\ueb63";
    public static readonly string SymbolNamespace = "\uea8b";
    public static readonly string SymbolNumeric = "\uea90";
    public static readonly string SymbolOperator = "\ueb64";
    public static readonly string SymbolParameter = "\uea92";
    public static readonly string SymbolProperty = "\ueb65";
    public static readonly string SymbolRuler = "\uea96";
    public static readonly string SymbolSnippet = "\ueb66";
    public static readonly string SymbolString = "\ueb8d";
    public static readonly string SymbolStructure = "\uea91";
    public static readonly string SymbolVariable = "\uea88";
    public static readonly string SyncIgnored = "\ueb9f";
    public static readonly string Sync = "\uea77";
    public static readonly string Table = "\uebb7";
    public static readonly string Tag = "\uea66";
    public static readonly string Target = "\uebf8";
    public static readonly string Tasklist = "\ueb67";
    public static readonly string Telescope = "\ueb68";
    public static readonly string TerminalBash = "\uebca";
    public static readonly string TerminalCmd = "\uebc4";
    public static readonly string TerminalDebian = "\uebc5";
    public static readonly string TerminalLinux = "\uebc6";
    public static readonly string TerminalPowershell = "\uebc7";
    public static readonly string TerminalTmux = "\uebc8";
    public static readonly string TerminalUbuntu = "\uebc9";
    public static readonly string Terminal = "\uea85";
    public static readonly string TextSize = "\ueb69";
    public static readonly string ThreeBars = "\ueb6a";
    public static readonly string Thumbsdown = "\ueb6b";
    public static readonly string Thumbsup = "\ueb6c";
    public static readonly string Tools = "\ueb6d";
    public static readonly string Trash = "\uea81";
    public static readonly string TriangleDown = "\ueb6e";
    public static readonly string TriangleLeft = "\ueb6f";
    public static readonly string TriangleRight = "\ueb70";
    public static readonly string TriangleUp = "\ueb71";
    public static readonly string Twitter = "\ueb72";
    public static readonly string TypeHierarchySub = "\uebba";
    public static readonly string TypeHierarchySuper = "\uebbb";
    public static readonly string TypeHierarchy = "\uebb9";
    public static readonly string Unfold = "\ueb73";
    public static readonly string UngroupByRefType = "\ueb98";
    public static readonly string Unlock = "\ueb74";
    public static readonly string Unmute = "\ueb75";
    public static readonly string Unverified = "\ueb76";
    public static readonly string VariableGroup = "\uebb8";
    public static readonly string VerifiedFilled = "\uebe9";
    public static readonly string Verified = "\ueb77";
    public static readonly string Versions = "\ueb78";
    public static readonly string VmActive = "\ueb79";
    public static readonly string VmConnect = "\ueba9";
    public static readonly string VmOutline = "\ueb7a";
    public static readonly string VmRunning = "\ueb7b";
    public static readonly string Vm = "\uea7a";
    public static readonly string Wand = "\uebcf";
    public static readonly string Warning = "\uea6c";
    public static readonly string Watch = "\ueb7c";
    public static readonly string Whitespace = "\ueb7d";
    public static readonly string WholeWord = "\ueb7e";
    public static readonly string Window = "\ueb7f";
    public static readonly string WordWrap = "\ueb80";
    public static readonly string WorkspaceTrusted = "\uebc1";
    public static readonly string WorkspaceUnknown = "\uebc3";
    public static readonly string WorkspaceUntrusted = "\uebc2";
    public static readonly string ZoomIn = "\ueb81";
    public static readonly string ZoomOut = "\ueb82";
}