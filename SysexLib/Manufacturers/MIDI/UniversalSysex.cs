using System;
using System.Collections.Generic;
using System.Linq;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.MIDI;

/// <summary>
/// Universal (non manufacturer-exclusive) Sysex message types, defined by the
/// MIDI association.
/// </summary>
/// <remarks>
/// https://www.midi.org/specifications-old/item/table-4-universal-system-exclusive-messages
/// http://www.muzines.co.uk/articles/everything-you-ever-wanted-to-know-about-system-exclusive/4558
/// </remarks>
public class UniversalSysex : Sysex
{
    // Format:
    // [f0 7f <channel> <sub ID 1> <sub ID 2> <data bytes> f7] (Realtime)
    // [f0 7e <channel> <sub ID 1> <sub ID 2> <data bytes> f7] (Non-realtime)

    internal class UniversalSysexId
    {
        public byte SubId1;
        public string Type;
        public Dictionary<byte, string>? Subtypes; // indexed by SubId2
    }

	internal static List<UniversalSysexId> NonRealtimeMessages = new List<UniversalSysexId>()
	{
		// Non-realtime types (0x7e) defined by the MIDI Association

		new(){SubId1 = 0x01, Type = "Sample Dump Header" },
		new(){SubId1 = 0x02, Type = "Sample Data Packet" },
		new(){SubId1 = 0x03, Type = "Sample Dump Request" },
		new(){SubId1 = 0x04, Type = "MIDI Time Code",
			Subtypes = new Dictionary<byte, string>{
			{ 0x00, "Special" },
			{ 0x01, "Punch In Points" },
			{ 0x02, "Punch Out Points" },
			{ 0x03, "Delete Punch In Point" },
			{ 0x04, "Delete Punch Out Point" },
			{ 0x05, "Event Start Point" },
			{ 0x06, "Event Stop Point" },
			{ 0x07, "Event Start Points with additional info" },
			{ 0x08, "Event Stop Points with additional info" },
			{ 0x09, "Delete Event Start Point" },
			{ 0x0a, "Delete Event Stop Point" },
			{ 0x0b, "Cue Points" },
			{ 0x0c, "Cue Points with Additional Info" },
			{ 0x0d, "Delete Cue Point" },
			{ 0x0e, "Event Name in Additional Info" },
		}},
		new(){SubId1 = 0x05, Type = "Sample Dump Extensions",
			Subtypes = new Dictionary<byte, string>{
			{ 0x01, "Loop Points Transmission" },
			{ 0x02, "Loop Points Request" },
			{ 0x03, "Sample Name Transmission" },
			{ 0x04, "Sample Name Request" },
			{ 0x05, "Extended Dump Header" },
			{ 0x06, "Extended Loop Points Transmission" },
			{ 0x07, "Extended Loop Points Request" },
		}},
		new(){SubId1 = 0x06, Type = "General Information",
			Subtypes = new Dictionary<byte, string>{
			{ 0x01, "Identity Request" },
			{ 0x02, "Identity Reply" },
		}},
		new(){SubId1 = 0x07, Type = "File Dump",
			Subtypes = new Dictionary<byte, string>{
			{ 0x01, "Header" },
			{ 0x02, "Data Packet" },
			{ 0x03, "Request" },
		}},
		new(){SubId1 = 0x08, Type = "MIDI Tuning Standard",
			Subtypes = new Dictionary<byte, string>{
			{ 0x00, "Bulk Dump Request" },
			{ 0x01, "Bulk Dump Reply" },
			{ 0x03, "Tuning Dump Request" },
			{ 0x04, "Key-Based Tuning Dump" },
			{ 0x05, "Scale/Octave Tuning Dump, 1 byte format" },
			{ 0x06, "Scale/Octave Tuning Dump, 2 byte format" },
			{ 0x07, "Single Note Tuning Change with Bank Select" },
			{ 0x08, "Scale/Octave Tuning, 1 byte format" },
			{ 0x09, "Scale/Octave Tuning, 2 byte format" },
		}},
		new(){SubId1 = 0x09, Type = "General MIDI",
			Subtypes = new Dictionary<byte, string>{
			{ 0x01, "General MIDI 1 System On" },
			{ 0x02, "General MIDI System Off" },
			{ 0x03, "General MIDI 2 System On" },
		}},
		new(){SubId1 = 0x0a, Type = "Downloadable Sounds",
			Subtypes = new Dictionary<byte, string>{
			{ 0x01, "Turn DLS On" },
			{ 0x02, "Turn DLS Off" },
			{ 0x03, "Turn DLS Voice Allocation Off" },
			{ 0x04, "Turn DLS Voice Allocation On" },
		}},
		new(){SubId1 = 0x0b, Type = "File Reference Message",
			Subtypes = new Dictionary<byte, string>{
			{ 0x01, "Open File" },
			{ 0x02, "Select or Reselect Contents" },
			{ 0x03, "Open File and Select Contents" },
			{ 0x04, "Close File" },
		}},
		new(){SubId1 = 0x0c, Type = "MIDI Visual Control" }, // TODO: Add MVC commands
		new(){SubId1 = 0x0d, Type = "MIDI Capability Inquiry" }, // TODO: Add inquiry/response messages https://nagasm.org/ASL/Basic/fig2/midi2.pdf
		new(){SubId1 = 0x7b, Type = "End of File" },
		new(){SubId1 = 0x7c, Type = "Wait" },
		new(){SubId1 = 0x7d, Type = "Cancel" },
		new(){SubId1 = 0x7e, Type = "NAK" },
		new(){SubId1 = 0x7f, Type = "ACK" },
	};
	internal static Dictionary<byte, UniversalSysexId> NonRealtimeMessagesById; // Indexed by SubId1


	// Realtime types (0x7f) defined by the MIDI Association

    internal static List<UniversalSysexId> RealtimeMessages = new List<UniversalSysexId>() {
        new(){SubId1 = 0x01, Type = "MIDI Time Code",
            Subtypes = new Dictionary<byte, string>{
            { 0x01, "Full Message" },
            { 0x02, "User Bits" },
        }},
        new(){SubId1 = 0x02, Type = "MIDI Show Control" }, // TODO: Add MSC Command and Extensions
		new(){SubId1 = 0x03, Type = "Notation Information",
            Subtypes = new Dictionary<byte, string>{
            { 0x01, "Bar Number" },
            { 0x02, "Time Signature (Immediate)" },
            { 0x42, "Time Signature (Delayed)" },
        }},
        new(){SubId1 = 0x04, Type = "Device Control",
            Subtypes = new Dictionary<byte, string>{
            { 0x01, "Master Volume" },
            { 0x02, "Master Balance" },
            { 0x03, "Master Fine Tuning" },
            { 0x04, "Master Coarse Tuning" },
            { 0x05, "Global Parameter Control" },
        }},
        new(){SubId1 = 0x05, Type = "Real Time MTC Cueing",
            Subtypes = new Dictionary<byte, string>{
            { 0x00, "Special" },
            { 0x01, "Punch In Points" },
            { 0x02, "Punch Out Points" },
            { 0x05, "Event Start Points" },
            { 0x06, "Event Stop Points" },
            { 0x07, "Event Start Points with additional info" },
            { 0x08, "Event Stop Points with additional info" },
            { 0x0b, "Cue Points" },
            { 0x0c, "Cue Points with Additional Info" },
            { 0x0e, "Event Name in Additional Info" },
        }},
        new(){SubId1 = 0x06, Type = "MIDI Machine Control Commands" }, // TODO: Add MMC commands
        new(){SubId1 = 0x07, Type = "MIDI Machine Control Responses" }, // TODO: Add MMC responses

    };
    internal static Dictionary<byte, UniversalSysexId> RealtimeMessagesById; // Indexed by SubId1


    static UniversalSysex()
    {
        NonRealtimeMessagesById = NonRealtimeMessages.ToDictionary(e => e.SubId1);
        RealtimeMessagesById = RealtimeMessages.ToDictionary(e => e.SubId1);
    }


    internal UniversalSysex(byte[] data, string? name = null, int? expectedLength = null)
        : base(data, name, expectedLength)
    {
        if (data.Length <= 6)
            throw new ArgumentException(nameof(data), "Not enough data for a valid Universal System Exclusive message.");

        byte universalMarker = data[1];
        byte channel = data[2];
        byte subId1 = data[3];
        byte subId2 = data[4];

        _type = Parse(universalMarker, subId1, subId2);
    }

    private string? _type = null;
    public override string? Type => _type;

    /// <returns>Brief description of type.</returns>
    private string Parse(
        byte universalMarker, byte subId1, byte subId2)
    {
        string category, type, subtype;
        Dictionary<byte, UniversalSysexId> messages;
        switch (universalMarker)
        {
            case 0x7e:
                messages = NonRealtimeMessagesById;
                category = "Non-realtime";
                break;
            case 0x7f:
                messages = RealtimeMessagesById;
                category = "Realtime";
                break;
            default:
                throw new ArgumentException(
                    nameof(universalMarker),
                    "Universal System Exclusive marker not found.");
        }

        if (messages.ContainsKey(subId1))
        {
            UniversalSysexId messageInfo = messages[subId1];
            type = messageInfo.Type;
            if (messageInfo.Subtypes != null) {
                type += ": ";
                type += messageInfo.Subtypes.GetValueOrDefault(subId2, "Unknown type");
            }
        }
        else
        {
            return $"Universal {category}: Unknown type";
        }
        return type;
    }
}


