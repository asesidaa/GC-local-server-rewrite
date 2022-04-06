using Swan;
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

    public static string LogRootPath => Path.Combine(BasePath, Configs.LOG_FOLDER);

    public static string CertRootPath => Path.Combine(BasePath, Configs.CERT_FOLDER);

    public static string ConfigFilePath => Environment.ProcessPath ?? string.Empty;

    private static string BasePath
    {
        get
        {
            var assemblyPath = Environment.ProcessPath;
            if (assemblyPath == null)
            {
                throw SelfCheck.Failure("Cannot get assembly path!!!");
            }

#if DEBUG
            

            var parentFullName = Directory.GetParent(assemblyPath)?.Parent?.Parent?.Parent?.FullName;

            Debug.Assert(parentFullName != null, $"{nameof(parentFullName)} != null");

            return parentFullName;
#else
            var parent = Directory.GetParent(assemblyPath);
            if (parent == null)
            {
                throw SelfCheck.Failure("Cannot get assembly parent path!!!");
            }
            return parent.ToString();
#endif
        }
    }
}