namespace SmsPaymentService.DTO
{
    public struct SendPaymentRequest
    {
        public int OrderId { get; set; }
        public string PhoneNumber { get; set; }
    }
}
