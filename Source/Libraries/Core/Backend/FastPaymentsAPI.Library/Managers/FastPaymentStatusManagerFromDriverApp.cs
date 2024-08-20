namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentStatusManagerFromDriverApp : FastPaymentStatusManagerBase
	{
		private readonly FastPaymentProcessingStatusChecker _fastPaymentProcessingStatusChecker;
		private readonly FastPaymentStatusNotEqualResponseChecker _paymentStatusNotEqualResponseChecker;
		private readonly ResponseStatusPerformedChecker _responseStatusPerformedChecker;
		private readonly ResponseStatusProcessingFromDesktopChecker _responseStatusProcessingFromDesktopChecker;
		private readonly ResponseStatusProcessingFromDriverAppChecker _responseStatusProcessingFromDriverAppChecker;

		public FastPaymentStatusManagerFromDriverApp(
			FastPaymentPerformedStatusChecker fastPaymentPerformedStatusChecker,
			FastPaymentProcessingStatusChecker fastPaymentProcessingStatusChecker,
			FastPaymentStatusNotEqualResponseChecker paymentStatusNotEqualResponseChecker,
			ResponseStatusPerformedChecker responseStatusPerformedChecker,
			ResponseStatusProcessingFromDriverAppChecker responseStatusProcessingFromDriverAppChecker,
			ResponseStatusProcessingFromDesktopChecker responseStatusProcessingFromDesktopChecker
			) : base(fastPaymentPerformedStatusChecker)
		{
			_fastPaymentProcessingStatusChecker = fastPaymentProcessingStatusChecker;
			_paymentStatusNotEqualResponseChecker = paymentStatusNotEqualResponseChecker;
			_responseStatusPerformedChecker = responseStatusPerformedChecker;
			_responseStatusProcessingFromDriverAppChecker = responseStatusProcessingFromDriverAppChecker;
			_responseStatusProcessingFromDesktopChecker = responseStatusProcessingFromDesktopChecker;
			SetHandlers();
		}

		protected override void SetHandlers()
		{
			FastPaymentStatusChecker.SetNextHandler(_fastPaymentProcessingStatusChecker);
			_fastPaymentProcessingStatusChecker.SetNextHandler(_paymentStatusNotEqualResponseChecker);
			_paymentStatusNotEqualResponseChecker.SetNextHandler(_responseStatusPerformedChecker);
			_responseStatusPerformedChecker.SetNextHandler(_responseStatusProcessingFromDriverAppChecker);
			_responseStatusProcessingFromDriverAppChecker.SetNextHandler(_responseStatusProcessingFromDesktopChecker);
		}
	}
}
