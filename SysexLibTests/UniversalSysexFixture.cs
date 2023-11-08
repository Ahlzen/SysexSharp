﻿using Ahlzen.SysexSharp.SysexLib;
using Ahlzen.SysexSharp.SysexLib.Manufacturers.MIDI;
using NUnit.Framework;

namespace Ahlzen.SysexSharp.SysexLibTests;

[TestFixture]
public class UniversalSysexFixture
{
    // SDS (sample dump) messages
    public static byte[] SampleDumpRequest = {
        0xf0, 0x7e, 0x00, 0x03, 0x01, 0x05, 0xf7
    };
    public static byte[] SampleDataPacket = {
        0xf0, 0x7e, 0x00, 0x02, 0x00,
        // data bytes
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, // checksum (xor of data bytes & 0x7f)
        0xf7
    };
    public static byte[] SampleDataACK = {
        0xf0, 0x7e, 0x00, 0x7f, 0x00, 0xf7
    };

    [Test]
    public void TestSDSMessages()
    {
        Sysex dumpRequest = SysexFactory.Create(SampleDumpRequest);
        Assert.IsInstanceOf<UniversalSysex>(dumpRequest);
        Assert.IsNull(dumpRequest.ManufacturerName);
        Assert.AreEqual("Sample Dump Request", dumpRequest.Type);

        Sysex sampleDataPacket = SysexFactory.Create(SampleDataPacket);
        Assert.IsInstanceOf<UniversalSysex>(sampleDataPacket);
        Assert.IsNull(sampleDataPacket.ManufacturerName);
        Assert.AreEqual("Sample Data Packet", sampleDataPacket.Type);
        
        Sysex sampleDataACK = SysexFactory.Create(SampleDataACK);
        Assert.IsInstanceOf<UniversalSysex>(sampleDataACK);
        Assert.IsNull(sampleDataACK.ManufacturerName);
        Assert.AreEqual("ACK", sampleDataACK.Type);
    }

}
