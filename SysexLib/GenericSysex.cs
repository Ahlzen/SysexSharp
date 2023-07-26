namespace Ahlzen.SysexSharp.SysexLib
{
    /// <summary>
    /// Concrete class that represents a generic sysex
    /// (not of a known type).
    /// </summary>
    public class GenericSysex : Sysex
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawData">Raw bytes, including start/end-of-sysex</param>
        /// <param name="name">Filename, bank name etc, or null if not known.</param>
        public GenericSysex(byte[] rawData, string name = null)
            : base(rawData)
        {
            Name = name;
        }

        public override string? Name { get; }

        public override byte[] ManufacturerId =>
            Manufacturers.GetId(Data);

        public override string? ManufacturerName =>
            Manufacturers.GetName(ManufacturerId);

        public override string? Device => null;

        public override string? Type => null;

        public override bool CanParse => false;

        public override bool HasSubItems => false;
    }
}
