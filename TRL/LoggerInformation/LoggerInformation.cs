namespace TRL
{
    public class LoggerInformation
    {
        public string SerialNumber { get; set; } = "L0001066";
        public string LoggerName { get; set; } = "G4";
        public int LoggerType { get; set; } = 6;
        public string JsonFile { get; set; } = "G4.json";
        public int MaxMemory { get; set; } = 0x04;
        public int MemoryHeaderPointer { get; set; } = 13;
        public int[] MemoryStart { get; set; } = new int[] { 0x0000, 0x0020, 0x0000, 0x0000, 0x0000 };
        public int[] MemoryMax { get; set; } = new int[] { 0x353C, 0x0100, 0x0000, 0x0000, 0x8000 };
        public int RequestMemoryStartPointer { get; set; } = 3;
        public int RequestMemoryMaxPointer { get; set; } = 1;
        public string EmailId { get; set; }
    }
}
