namespace FastPaymentsApi.Contracts.Requests
{
	public class FastPaymentRequestDTO
	{
		public int OrderId { get; set; }
		public string PhoneNumber { get; set; }
		public bool IsQr { get; set; } = true;
	}
}
