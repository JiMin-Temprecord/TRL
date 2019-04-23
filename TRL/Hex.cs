namespace TRL
{
    class Hex
    {
        public Hex(string address, string reply)
        {
            Address = address;
            Reply = reply;
        }

        private string Address { get; }

        private string Reply { get; }

        public override string ToString()
        {
            return $"{Address}:{Reply}";
        }
    }
}
