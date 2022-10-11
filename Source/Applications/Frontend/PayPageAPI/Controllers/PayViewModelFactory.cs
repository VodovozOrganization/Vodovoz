using System;
using PayPageAPI.Models;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace PayPageAPI.Controllers
{
	public class PayViewModelFactory : IPayViewModelFactory
	{
		private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;
		private readonly IOrganizationParametersProvider _organizationParametersProvider;

		public PayViewModelFactory(
			IFastPaymentParametersProvider fastPaymentParametersProvider,
			IOrganizationParametersProvider organizationParametersProvider)
		{
			_fastPaymentParametersProvider =
				fastPaymentParametersProvider ?? throw new ArgumentNullException(nameof(fastPaymentParametersProvider));
			_organizationParametersProvider =
				organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));
		}
		
		public PayViewModel CreateNewPayViewModel(FastPayment fastPayment) =>
			new PayViewModel(_fastPaymentParametersProvider, _organizationParametersProvider, fastPayment);
	}
}
