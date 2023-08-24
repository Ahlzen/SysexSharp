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
}