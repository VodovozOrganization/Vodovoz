using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class IncomeParametersProvider : IIncomeParametersProvider
	{
		private readonly IOrganizationParametersProvider _organizationParametersProvider;

		public IncomeParametersProvider(IOrganizationParametersProvider organizationParametersProvider)
		{
			_organizationParametersProvider = organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));
		}

		public int DefaultIncomeOrganizationId => _organizationParametersProvider.SosnovcevOrganizationId;
	}
}
