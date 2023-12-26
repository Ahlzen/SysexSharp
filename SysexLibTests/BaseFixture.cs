using System.Text;
using Ahlzen.SysexSharp.SysexLib;
using NUnit.Framework;

namespace Ahlzen.SysexSharp.SysexLibTests;

[TestFixture]
public abstract class BaseFixture
{
    /// <summary>
    /// Path to data directory, relative to the unit test's working
    /// directory. When building and running under VS, that's typically
    /// three levels up (for example bin/Debug/net6.0/)
    /// </summary>
    protected const string DataPath = "../../../data/";

    protected Sysex LoadFile(string filename)
    {
        Sysex sysex = SysexFactory.Load(DataPath + filename);
        return sysex;
    }

    protected string FormatDetails(Sysex sysex, string indent = "")
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"{indent}Name: {sysex.Name}");
        sb.AppendLine($"{indent}Manufacturer: {ToHex(sysex.ManufacturerId)} ({sysex.ManufacturerName})");
        sb.AppendLine($"{indent}Device: {sysex.Device ?? "unknown"}");
        sb.AppendLine($"{indent}Type: {sysex.Type ?? "unknown"}");

        if (sysex is ICanParse pSysex)
        {
            sb.AppendLine($"{indent}Parameter values:");
            foreach (string parameterName in pSysex.ParameterNames)
            {
                object value = pSysex.GetParameterValue(parameterName);
                sb.AppendLine($"{indent}- {parameterName}: {value}");
            }
        }

        if (sysex is IHasItems iSysex)
        {
            sb.AppendLine($"{indent}Items:");
            sb.AppendLine();
            for (int n = 0; n < iSysex.ItemCount; n++)
            {
                Sysex child = iSysex.GetItem(n);
                sb.AppendLine($"{indent}- Item #{n + 1}:");
                sb.AppendLine(FormatDetails(child, indent + "   "));
            }
        }

        return sb.ToString();
    }

    protected string ToHex(byte[] data)
    {
        var sb = new StringBuilder("[");
        foreach (byte b in data)
            sb.Append(b.ToString("X2"));
        sb.Append("]");
        return sb.ToString();
    }
}