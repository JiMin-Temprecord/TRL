namespace TRL
{
    class WakeUpByteWritter : IByteWriter
    {
        public byte[] WriteBytes(byte[] sendMessage)
        {
            sendMessage[0] = 0X00;              //Opcode
            sendMessage[1] = 0X55;              //Opcode
            sendMessage[2] = 0x01;               //READ OPCODE
            sendMessage[3] = 0x02;               //Length
            sendMessage[4] = 0x7c;               //Length
            sendMessage[5] = 0x0e;               //Memory
            sendMessage[6] = 0x0d;               //Address
            return sendMessage;
        }
    }
}
