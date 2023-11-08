using System.IO;
using System.Reflection;

namespace Ahlzen.SysexSharp.SysexLib.Utils;

public static class EmbeddedResourceHelper
{
    private const string ResourceNamePrefix = "Ahlzen.SysexSharp.SysexLib.Data.";

    /// <summary>
    /// Return the specified embedded resource data as a string.
    /// </summary>
    /// <param name="resourceName">
    /// Name of resource. Assumed to be under Data/.
    /// Use namespace rules (use '.' to separate subfolders).
    /// </param>
    /// <returns></returns>
    public static string GetAsText(string resourceName)
    {
        // Based on https://stackoverflow.com/questions/3314140/how-to-read-embedded-resource-text-file

        string fullName = ResourceNamePrefix + resourceName;
        var assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream(fullName))
        using (StreamReader reader = new StreamReader(stream))
        {
            string data = reader.ReadToEnd();
            return data;
        }
    }
}