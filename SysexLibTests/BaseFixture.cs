using Ahlzen.SysexSharp.SysexLib;
using NUnit.Framework;

namespace Ahlzen.SysexSharp.SysexLibTests;

[TestFixture]
public abstract class BaseFixture
{
    protected const string DataPath = ".\\data\\";

    protected Sysex LoadFile(string filename)
    {
        Sysex sysex = SysexFactory.Load(DataPath + filename);
        return sysex;
    }
}