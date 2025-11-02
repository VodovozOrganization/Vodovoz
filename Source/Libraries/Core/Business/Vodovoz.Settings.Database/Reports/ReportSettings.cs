using System;
using Vodovoz.Settings.Reports;

namespace Vodovoz.Settings.Database.Reports
{
	public class ReportSettings : IReportSettings
	{
		private readonly ISettingsController _settingsController;

		public ReportSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}
		public int GetDefaultOrderChangesOrganizationId => _settingsController.GetIntValue("order_changes_default_organization_id");
	}
}
