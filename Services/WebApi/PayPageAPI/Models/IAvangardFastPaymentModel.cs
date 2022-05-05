using System;
using Vodovoz.Domain.FastPayments;

namespace PayPageAPI.Models
{
	public interface IAvangardFastPaymentModel
	{
		FastPayment GetFastPaymentByGuid(Guid fastPaymentGuid);
	}
}
