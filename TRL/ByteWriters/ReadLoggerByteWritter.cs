using System.Threading.Tasks;

namespace TRL
{
    class ReadLoggerByteWritter : IByteWriter
    {
        public ReadLoggerByteWritter(AddressSection addressSection)
        {
            this.addressSection = addressSection;
        }

        readonly AddressSection addressSection;

        public byte[] WriteBytes(byte[] sendMessage)
        {
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
