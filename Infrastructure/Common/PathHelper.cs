using System.Diagnostics;
using Validation;

namespace Infrastructure.Common;

public static class PathHelper
{
    public static string DatabasePath = Path.Combine(BasePath, "Database");

    public static string ConfigurationPath = Path.Combine(BasePath, "Configurations");
    
    public static string BasePath
    {
        get
        {
            var assemblyPath = Environment.ProcessPath;
            Assumes.NotNull(assemblyPath);

#if DEBUG
            var parentFullName = Directory.GetParent(assemblyPath)?.Parent?.Parent?.Parent?.FullName;

            return parentFullName ?? "";

#else
            var parent = Directory.GetParent(assemblyPath);
            Assumes.NotNull(parent);
            return parent.ToString();
#endif
        }
    }
}