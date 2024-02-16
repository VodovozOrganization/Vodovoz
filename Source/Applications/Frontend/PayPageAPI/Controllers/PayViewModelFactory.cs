using System;
using PayPageAPI.Models;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Parameters;
using Vodovoz.Settings.Organizations;

namespace PayPageAPI.Controllers
{
	public class PayViewModelFactory : IPayViewModelFactory
	{
		private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;
		private readonly IOrganizationSettings _organizationParametersProvider;

		public PayViewModelFactory(
			IFastPaymentParametersProvider fastPaymentParametersProvider,
			IOrganizationSettings organizationParametersProvider)
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
