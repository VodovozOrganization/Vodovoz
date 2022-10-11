namespace SmsPaymentService
{
    public struct RequestBody
    {
        public int ExternalId { get; set; }
        public int Status { get; set; }
        public string PaidDate { get; set; }
        public int OrderId { get; set; }
    }
}
