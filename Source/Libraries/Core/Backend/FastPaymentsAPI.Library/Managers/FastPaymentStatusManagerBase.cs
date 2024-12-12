using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Responses;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public abstract class FastPaymentStatusManagerBase
	{
		protected FastPaymentStatusManagerBase(FastPaymentStatusCheckerBase fastPaymentStatusCheckerBase)
		{
			FastPaymentStatusChecker =
				fastPaymentStatusCheckerBase ?? throw new ArgumentNullException(nameof(fastPaymentStatusCheckerBase));
		}
		
		protected FastPaymentStatusCheckerBase FastPaymentStatusChecker { get; }
		protected abstract void SetHandlers();

		public virtual async Task<(bool NeedReturnResult, IFastPaymentStatusDto Result)> CheckAllOrderFastPayments(
			IEnumerable<FastPayment> fastPayments, IFastPaymentStatusDto result)
		{
			if(!fastPayments.Any())
			{
				return (false, result);
			}

			return await FastPaymentStatusChecker.CheckStatus(result, fastPayments.First());
		}
	}
}
