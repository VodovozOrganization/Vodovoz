using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class ExpenseParametersProvider : IExpenseParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;
		private readonly IOrganizationParametersProvider _organizationParametersProvider;

		public ExpenseParametersProvider(IParametersProvider parametersProvider, IOrganizationParametersProvider organizationParametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
			_organizationParametersProvider = organizationParametersProvider;
		}

		public string ChangeCategoryName => "Сдача клиенту";

		public int DefaultChangeOrganizationId => _organizationParametersProvider.SosnovcevOrganizationId;
	}
}
