using Ahlzen.SysexSharp.SysexLib;
using Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;
using NUnit.Framework;
using System;

namespace Ahlzen.SysexSharp.SysexLibTests;

[TestFixture]
public class YamahaFixture : BaseFixture
{
    ///// DX7
    
    [Test]
    public void TestParseDX7Bank()
    {
        Sysex sysex = LoadFile("Yamaha DX7 ROM1A.syx");

        Assert.AreEqual("Yamaha", sysex.ManufacturerName);
        Assert.AreEqual("DX7", sysex.Device);
        Assert.AreEqual("32-voice bank", sysex.Type);
        Assert.IsInstanceOf<DX7Bank>(sysex);
        
        DX7Bank bank = (DX7Bank)sysex;
        Assert.AreEqual(32, bank.ItemCount);

        // List voice names
        List<string>? voiceNames = bank.GetItemNames()?.ToList();
        Assert.IsNotNull(voiceNames);
        Assert.AreEqual(32, voiceNames!.Count);
        Assert.AreEqual("BRASS   1", voiceNames![0]);
        Assert.AreEqual("BRASS   2", voiceNames![1]);
        Assert.AreEqual("TUB BELLS", voiceNames![25]);

        // Extract single-voice sysex
        DX7Voice? voice = bank.GetItem(25) as DX7Voice;
        Assert.IsNotNull(voice);
        Assert.AreEqual("Yamaha", voice!.ManufacturerName);
        Assert.AreEqual("DX7", voice!.Device);
        Assert.AreEqual("Single voice", voice!.Type);
        Assert.AreEqual("TUB BELLS", voice!.Name);
    }


    ///// DX21/DX27/DX100

    private const string DX21BankFilename = "Yamaha DX21 Presets 1.syx";

    [Test]
    public void TestParseDX21Bank()
    {
        Sysex sysex = LoadFile(DX21BankFilename);

        Assert.AreEqual("Yamaha", sysex.ManufacturerName);
        Assert.AreEqual("DX21/DX27/DX100", sysex.Device);
        Assert.AreEqual("32-voice bank", sysex.Type);
        Assert.IsInstanceOf<DX21Bank>(sysex);

        DX21Bank bank = (DX21Bank)sysex;
        Assert.AreEqual(32, bank.ItemCount);

        // List voice names
        List<string>? voiceNames = bank.GetItemNames()?.ToList();
        Assert.IsNotNull(voiceNames);
        Assert.AreEqual(32, voiceNames!.Count);
        Assert.AreEqual("Deep Grand", voiceNames![0]);
        Assert.AreEqual("Uprt Piano", voiceNames![1]);
        Assert.AreEqual("HonkeyTonk", voiceNames![2]);

        // Extract single-voice sysex
        DX21Voice? voice = bank.GetItem(2) as DX21Voice;
        Assert.IsNotNull(voice);
        Assert.AreEqual("Yamaha", voice!.ManufacturerName);
        Assert.AreEqual("DX21/DX27/DX100", voice!.Device);
        Assert.AreEqual("Single voice", voice!.Type);
        Assert.AreEqual("HonkeyTonk", voice!.Name);
    }


    ///// TX81Z

    private const string TX81ZVoiceFilename = "Yamaha TX81Z FilterBass.syx";
    private const string TX81ZBankFilename = "Yamaha TX81Z 01A.syx";

    [Test]
    public void TestParseTX81ZVoice()
    {
        Sysex sysex = LoadFile(TX81ZVoiceFilename);

        // TX81Z voice consists of DX21 Voice Data + TX81Z Additional Voice Data
        Assert.IsTrue(sysex is CompositeSysex);
        Assert.IsTrue(sysex is TX81ZVoice);
        Assert.IsTrue(((TX81ZVoice)sysex).GetItem(0) is TX81ZAdditionalVoiceData);
        Assert.IsTrue(((TX81ZVoice)sysex).GetItem(1) is DX21Voice);
        Assert.AreEqual("Yamaha", sysex!.ManufacturerName);
        Assert.AreEqual("TX81Z", sysex!.Device);
        Assert.AreEqual("Single voice", sysex!.Type);
        Assert.AreEqual("FilterBass", sysex!.Name);
    }

    [Test]
    public void TestParseTX81ZBank()
    {
        Sysex sysex = LoadFile(TX81ZBankFilename);
        Assert.AreEqual("Yamaha", sysex.ManufacturerName);
        Assert.AreEqual("TX81Z", sysex.Device);
        Assert.IsInstanceOf<TX81ZVoiceBank>(sysex);

        TX81ZVoiceBank bank = (TX81ZVoiceBank)sysex;
        Assert.AreEqual(32, bank.ItemCount);

        // Extract single-voice sysex
        TX81ZVoice? voice = bank.GetItem(3) as TX81ZVoice;
        Assert.IsNotNull(voice);
        Assert.AreEqual("Yamaha", voice!.ManufacturerName);
        Assert.AreEqual("TX81Z", voice!.Device);
        Assert.AreEqual("Single voice", voice!.Type);
        Assert.AreEqual("<<Bass.1>>", voice!.Name);
    }


    ///// List sysex details (for debugging)
    
    [Test]
    [Explicit]
    [TestCase(DX21BankFilename)]
    [TestCase(TX81ZBankFilename)]
    [TestCase(TX81ZVoiceFilename)]
    public void ListDetails(string filename)
    {
        Sysex sysex = LoadFile(filename);
        Console.WriteLine(FormatDetails(sysex));
    }
}