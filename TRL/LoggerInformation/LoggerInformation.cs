namespace TRL
{
    public class LoggerInformation
    {
        public string SerialNumber { get; set; }
        public string LoggerName { get; set; } 
        public int LoggerType { get; set; }
        public string JsonFile { get; set; }
        public int MaxMemory { get; set; }
        public int MemoryHeaderPointer { get; set; } 
        public int[] MemoryStart { get; set; }
        public int[] MemoryMax { get; set; } 
        public int RequestMemoryStartPointer { get; set; } 
        public int RequestMemoryMaxPointer { get; set; }
        public string EmailId { get; set; }
    }
}
