using System;
using PayPageAPI.Models;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Parameters;

namespace PayPageAPI.Controllers
{
	public class PayViewModelFactory : IPayViewModelFactory
	{
		private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;

		public PayViewModelFactory(IFastPaymentParametersProvider fastPaymentParametersProvider)
		{
			_fastPaymentParametersProvider =
				fastPaymentParametersProvider ?? throw new ArgumentNullException(nameof(fastPaymentParametersProvider));
		}
		
		public PayViewModel CreateNewPayViewModel(FastPayment fastPayment) => new PayViewModel(_fastPaymentParametersProvider, fastPayment);
	}
}
