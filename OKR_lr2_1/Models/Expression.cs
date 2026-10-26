namespace OKR_lr2_1.Models
{
    public class Expression
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ExpressionText { get; set; }
        public bool IsError { get; set; } = false;
        public double? Result { get; set; }
        public string? Error { get; set; }
    }
}