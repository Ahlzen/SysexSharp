using Ahlzen.SysexSharp.SysexLib;
using Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;
using NUnit.Framework;

namespace Ahlzen.SysexSharp.SysexLibTests;

[TestFixture]
public class DX7Fixture : BaseFixture
{
    [Test]
    public void TestParseDX7Bank()
    {
        Sysex sysex = LoadFile("DX7_ROM1A.SYX");

        Assert.AreEqual("Yamaha", sysex.ManufacturerName);
        Assert.AreEqual("DX7", sysex.Device);
        Assert.AreEqual("32-voice bank", sysex.Type);
        Assert.IsInstanceOf<DX7Bank>(sysex);
        
        DX7Bank bank = (DX7Bank)sysex;
        Assert.AreEqual(32, bank.ItemCount);

        List<string>? voiceNames = bank.GetItemNames()?.ToList();
        Assert.IsNotNull(voiceNames);
        Assert.AreEqual(32, voiceNames!.Count);
        Assert.AreEqual("BRASS   1", voiceNames![0]);
        Assert.AreEqual("BRASS   2", voiceNames![1]);
        Assert.AreEqual("TUB BELLS", voiceNames![25]);

        DX7Voice? voice = bank.GetItem(25) as DX7Voice;
        Assert.IsNotNull(voice);
        Assert.AreEqual("Yamaha", voice!.ManufacturerName);
        Assert.AreEqual("DX7", voice!.Device);
        Assert.AreEqual("Single voice", voice!.Type);
        Assert.AreEqual("TUB BELLS", voice!.Name);
    }
} 