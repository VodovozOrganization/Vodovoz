using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class EmployeeSettings : IEmployeeSettings
	{
		private readonly IParametersProvider _parametersProvider;

		public EmployeeSettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int WorkingClothesFineTemplateId => _parametersProvider.GetIntValue("working_clothes_fine_template_id");

		public int MaxDaysForNewbieDriver => _parametersProvider.GetIntValue("max_days_for_newbie_driver");
	}
}
