using PayPageAPI.Models;
using System;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Settings.FastPayments;
using Vodovoz.Settings.Organizations;

namespace PayPageAPI.Controllers
{
	public class PayViewModelFactory : IPayViewModelFactory
	{
		private readonly IFastPaymentSettings _fastPaymentSettings;
		private readonly IOrganizationSettings _organizationSettings;

		public PayViewModelFactory(
			IFastPaymentSettings fastPaymentSettings,
			IOrganizationSettings organizationSettings)
		{
			_fastPaymentSettings =
				fastPaymentSettings ?? throw new ArgumentNullException(nameof(fastPaymentSettings));
			_organizationSettings =
				organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
		}
		
		public PayViewModel CreateNewPayViewModel(FastPayment fastPayment) =>
			new PayViewModel(_fastPaymentSettings, _organizationSettings, fastPayment);
	}
}
