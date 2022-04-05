using System.Diagnostics;

namespace GCLocalServerRewrite.common;

public static class PathHelper
{
    /// <summary>
    ///     Gets the local path of html/static files.
    /// </summary>
    public static string HtmlRootPath => Path.Combine(BasePath, Configs.STATIC_FOLDER);

    /// <summary>
    ///     Root path for database, when debug, it's under source root, when release, it's the exe dir
    /// </summary>
    public static string DataBaseRootPath => Path.Combine(BasePath, Configs.DB_FOLDER);

    public static string LogRootPath => Path.Combine(BasePath, Configs.LOG_FOLDER, Configs.LOG_BASE_NAME);

    public static string ConfigFilePath => Path.Combine(BasePath, Environment.ProcessPath ?? string.Empty);

    private static string BasePath
    {
        get
        {
            var assemblyPath = AppContext.BaseDirectory;

#if DEBUG
            

            var parentFullName = Directory.GetParent(assemblyPath)?.Parent?.Parent?.Parent?.FullName;

            Debug.Assert(parentFullName != null, $"{nameof(parentFullName)} != null");

            return parentFullName;
#else
            return AppContext.BaseDirectory;
#endif
        }
    }
}