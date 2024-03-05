using System;
using Vodovoz.Settings.Reports;

namespace Vodovoz.Settings.Database.Reports
{
	public class ProductionWarehouseMovementReportSettings : IProductionWarehouseMovementReportSettings
	{
		private readonly ISettingsController _settingsController;

		public ProductionWarehouseMovementReportSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int DefaultProductionWarehouseId => _settingsController.GetIntValue("production_warehouse_movement_report_warehouse_id");
	}
}
