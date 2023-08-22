using Ahlzen.SysexSharp.SysexLib;
using Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;
using Ahlzen.SysexSharp.SysexLibTests;
using NUnit.Framework;

namespace SysexLibTests;

[TestFixture]
public class DX7Fixture : BaseFixture
{
    [Test]
    public void TestParseDX7Bank()
    {
        Sysex sysex = LoadFile("DX7_ROM-1.SYX");
        Assert.AreEqual("Yamaha", sysex.ManufacturerName);
        Assert.AreEqual("DX7", sysex.Device);
        Assert.IsInstanceOf<DX7Bank>(sysex);
        
        DX7Bank bank = (DX7Bank)sysex;

    }
}