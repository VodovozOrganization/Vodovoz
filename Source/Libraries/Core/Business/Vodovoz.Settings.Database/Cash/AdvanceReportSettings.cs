using System;
using Vodovoz.Settings.Cash;

namespace Vodovoz.Settings.Database.Cash
{
	public class AdvanceReportSettings : IAdvanceReportSettings
	{
		private readonly ISettingsController _settingsController;

		public AdvanceReportSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int DefaultAdvanceReportOrganizationId => _settingsController.GetValue<int>(nameof(DefaultAdvanceReportOrganizationId).FromPascalCaseToSnakeCase());
	}
}
