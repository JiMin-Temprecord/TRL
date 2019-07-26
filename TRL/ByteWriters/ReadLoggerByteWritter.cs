using System.Threading.Tasks;

namespace TRL
{
    class ReadLoggerByteWritter : IByteWriter
    {
        public ReadLoggerByteWritter(AddressSection addressSection, int loggerType)
        {
            this.addressSection = addressSection;
            this.loggerType = loggerType;
        }

        readonly int loggerType;
        readonly AddressSection addressSection;

        public byte[] WriteBytes(byte[] sendMessage)
        {
            switch (loggerType)
            {
                case 1:
                    sendMessage[0] = 0x02;               
                    sendMessage[1] = 0x07;
                    sendMessage[2] = 0x02;
                    sendMessage[3] = (byte)addressSection.MemoryNumber;
                    sendMessage[4] = 0x3A;
                    sendMessage[5] = (byte)addressSection.MemoryAddress;
                    sendMessage[6] = (byte)(addressSection.MemoryAddress >> 8);
                    return Communication.AddCRC(7,sendMessage);
                    
                default:
                    sendMessage[0] = 0x02;
                    sendMessage[1] = addressSection.LengthLSB;
                    sendMessage[2] = addressSection.LengthMSB;
                    sendMessage[3] = (byte)addressSection.MemoryNumber;
                    sendMessage[4] = addressSection.MemoryAddLSB;
                    sendMessage[5] = addressSection.MemoryAddMSB;
                    sendMessage[6] = (byte)0x00;
                    sendMessage[7] = (byte)0x00;
                    return Communication.AddCRC(8, sendMessage);
            }
        }
    }
}
