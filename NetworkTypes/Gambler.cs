namespace NetworkTypes
{
    public sealed class Gambler : SerializableType
    {
        public int Id { get; set; }
        public int Slot { get; set; }
        public string Name { get; set; }
        public string Response { get; set; }
    }
}
