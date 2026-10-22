namespace OKR_lr2_1.Models
{
    public class Operation
    {
        public string Id { get; set; }
        public string Op { get; set; }
        public double Number1 { get; set; }
        public double? Number2 { get; set; }
        public bool IsError { get; set; }
        public double? Result { get; set; }
        public string? Error { get; set; }
    }
}