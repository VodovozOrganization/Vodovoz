namespace FastPaymentsApi.Contracts.Requests
{
	public class RequestRegisterOnlineOrderDTO
	{
		public int OrderId { get; set; }
		public decimal OrderSum { get; set; }
		public string BackUrl { get; set; }
		public string BackUrlOk { get; set; }
		public string BackUrlFail { get; set; }
		public string CallbackUrl { get; set; }
	}
}
