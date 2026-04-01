namespace Vodovoz.Core.Application.FastPayment
{
	public class FastPaymentResult
	{
		public ResultStatus Status { get; set; }
		public string ErrorMessage { get; set; }
		public bool OrderAlreadyPaied { get; set; }
	}
}
