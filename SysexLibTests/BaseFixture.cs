using Ahlzen.SysexSharp.SysexLib;
using NUnit.Framework;

namespace Ahlzen.SysexSharp.SysexLibTests;

[TestFixture]
public abstract class BaseFixture
{
    protected const string DataPath = ".\\data\\";

    protected Sysex LoadFile(string filename)
    {
        byte[] data = File.ReadAllBytes(DataPath + filename);
        // TODO: Use SysexFactory
        Sysex sysex = new Sysex(data, filename);
        return sysex;
    }
}