namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentStatusManagerFromDesktop : FastPaymentStatusManagerBase
	{
		private readonly FastPaymentProcessingStatusChecker _fastPaymentProcessingStatusChecker;
		private readonly FastPaymentStatusNotEqualResponseChecker _paymentStatusNotEqualResponseChecker;
		private readonly ResponseStatusPerformedChecker _responseStatusPerformedChecker;
		private readonly ResponseStatusProcessingFromDesktopChecker _responseStatusProcessingChecker;

		public FastPaymentStatusManagerFromDesktop(
			FastPaymentPerformedStatusChecker fastPaymentPerformedStatusChecker,
			FastPaymentProcessingStatusChecker fastPaymentProcessingStatusChecker,
			FastPaymentStatusNotEqualResponseChecker paymentStatusNotEqualResponseChecker,
			ResponseStatusPerformedChecker responseStatusPerformedChecker,
			ResponseStatusProcessingFromDesktopChecker responseStatusProcessingChecker
			) : base(fastPaymentPerformedStatusChecker)
		{
			_fastPaymentProcessingStatusChecker = fastPaymentProcessingStatusChecker;
			_paymentStatusNotEqualResponseChecker = paymentStatusNotEqualResponseChecker;
			_responseStatusPerformedChecker = responseStatusPerformedChecker;
			_responseStatusProcessingChecker = responseStatusProcessingChecker;
			SetHandlers();
		}

		protected override void SetHandlers()
		{
			FastPaymentStatusChecker.SetNextHandler(_fastPaymentProcessingStatusChecker);
			_fastPaymentProcessingStatusChecker.SetNextHandler(_paymentStatusNotEqualResponseChecker);
			_paymentStatusNotEqualResponseChecker.SetNextHandler(_responseStatusPerformedChecker);
			_responseStatusPerformedChecker.SetNextHandler(_responseStatusProcessingChecker);
		}
	}
}
