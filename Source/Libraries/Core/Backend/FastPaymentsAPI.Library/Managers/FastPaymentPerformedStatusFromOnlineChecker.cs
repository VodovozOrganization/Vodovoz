using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Responses;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentPerformedStatusFromOnlineChecker : FastPaymentStatusCheckerBase
	{
		public override async Task<(bool NeedReturnResult, IFastPaymentStatusDto Result)> CheckStatus(
			IFastPaymentStatusDto result, FastPayment fastPayment)
		{
			if(fastPayment.FastPaymentStatus == FastPaymentStatus.Performed)
			{
				result.ErrorMessage = "Онлайн-заказ уже оплачен";
				return (true, result);
			}

			if(NextHandler != null)
			{
				return await NextHandler.CheckStatus(result, fastPayment);
			}

			return (false, result);
		}
	}
}
