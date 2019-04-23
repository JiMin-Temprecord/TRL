namespace TRL
{
    public class AddressSection
    {
        public AddressSection (byte lengthLSB, byte lengthMSB, int memoryNumber, byte memoryAddLSB, byte memoryAddMSB, int memoryAddress)
        {
            LengthLSB = lengthLSB;
            LengthMSB = lengthMSB;
            MemoryNumber = memoryNumber;
            MemoryAddLSB = memoryAddLSB;
            MemoryAddMSB = memoryAddMSB;
            MemoryAddress = MemoryAddress;
        }

        public byte LengthLSB { get; set; }
        public byte LengthMSB { get; set; }
        public int MemoryNumber { get; set; }
        public byte MemoryAddLSB { get; set; }
        public byte MemoryAddMSB { get; set; }
        public int MemoryAddress { get; set; }
    }
}
