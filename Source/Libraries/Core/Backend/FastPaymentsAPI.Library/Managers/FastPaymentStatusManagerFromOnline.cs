namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentStatusManagerFromOnline : FastPaymentStatusManagerBase
	{
		private readonly FastPaymentProcessingStatusChecker _fastPaymentProcessingStatusChecker;
		private readonly FastPaymentStatusNotEqualResponseChecker _paymentStatusNotEqualResponseChecker;
		private readonly ResponseStatusPerformedFromOnlineChecker _responseStatusPerformedFromOnlineChecker;
		private readonly ResponseStatusProcessingFromOnlineChecker _responseStatusProcessingFromOnlineChecker;

		public FastPaymentStatusManagerFromOnline(
			FastPaymentPerformedStatusFromOnlineChecker fastPaymentPerformedStatusFromOnlineChecker,
			FastPaymentProcessingStatusChecker fastPaymentProcessingStatusChecker,
			FastPaymentStatusNotEqualResponseChecker paymentStatusNotEqualResponseChecker,
			ResponseStatusPerformedFromOnlineChecker responseStatusPerformedFromOnlineChecker,
			ResponseStatusProcessingFromOnlineChecker responseStatusProcessingFromDesktopChecker
			) : base(fastPaymentPerformedStatusFromOnlineChecker)
		{
			_fastPaymentProcessingStatusChecker = fastPaymentProcessingStatusChecker;
			_paymentStatusNotEqualResponseChecker = paymentStatusNotEqualResponseChecker;
			_responseStatusPerformedFromOnlineChecker = responseStatusPerformedFromOnlineChecker;
			_responseStatusProcessingFromOnlineChecker = responseStatusProcessingFromDesktopChecker;
			SetHandlers();
		}

		protected override void SetHandlers()
		{
			FastPaymentStatusChecker.SetNextHandler(_fastPaymentProcessingStatusChecker);
			_fastPaymentProcessingStatusChecker.SetNextHandler(_paymentStatusNotEqualResponseChecker);
			_paymentStatusNotEqualResponseChecker.SetNextHandler(_responseStatusPerformedFromOnlineChecker);
			_responseStatusPerformedFromOnlineChecker.SetNextHandler(_responseStatusProcessingFromOnlineChecker);
		}
	}
}
