namespace NetworkTypes
{
    public sealed class ResponseMessage : SerializableType
    {
        public string Response { get; set; }
        public string Message { get; set; }
    }
}
