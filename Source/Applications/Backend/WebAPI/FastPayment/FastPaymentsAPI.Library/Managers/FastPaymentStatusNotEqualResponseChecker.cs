using System;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsAPI.Library.Models;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentStatusNotEqualResponseChecker : FastPaymentStatusCheckerBase
	{
		private readonly IFastPaymentService _fastPaymentService;
		
		public FastPaymentStatusNotEqualResponseChecker(IFastPaymentService fastPaymentService)
		{
			_fastPaymentService = fastPaymentService ?? throw new ArgumentNullException(nameof(fastPaymentService));
		}
		
		public override async Task<(bool NeedReturnResult, IFastPaymentStatusDto Result)> CheckStatus(
			IFastPaymentStatusDto result, FastPayment fastPayment, OrderInfoResponseDTO response)
		{
			if((int)response.Status != (int)fastPayment.FastPaymentStatus)
			{
				_fastPaymentService.UpdateFastPaymentStatus(fastPayment, response.Status, response.StatusDate);
			}
			
			if(NextHandler != null)
			{
				return await NextHandler.CheckStatus(result, fastPayment, response);
			}
			
			return (false, result);
		}
	}
}
