using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Responses;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public abstract class FastPaymentStatusCheckerBase
	{
		protected FastPaymentStatusCheckerBase NextHandler { get; private set; }

		public void SetNextHandler(FastPaymentStatusCheckerBase nextFastPaymentHandler)
		{
			NextHandler = nextFastPaymentHandler;
		}

		public virtual Task<(bool NeedReturnResult, IFastPaymentStatusDto Result)> CheckStatus(
			IFastPaymentStatusDto result, FastPayment fastPayment)
		{
			return default;
		}
		
		public virtual Task<(bool NeedReturnResult, IFastPaymentStatusDto Result)> CheckStatus(
			IFastPaymentStatusDto result, FastPayment fastPayment, OrderInfoResponseDTO response)
		{
			return default;
		}
	}
}
