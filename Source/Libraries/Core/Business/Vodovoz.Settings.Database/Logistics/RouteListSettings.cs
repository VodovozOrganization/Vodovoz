using System;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.Settings.Database.Logistics
{
	public class RouteListSettings : IRouteListSettings
	{
		private readonly ISettingsController _settingsController;

		public RouteListSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int CashSubdivisionSofiiskayaId => _settingsController.GetIntValue("cashsubdivision_sofiiskaya_id");
		public int CashSubdivisionParnasId => _settingsController.GetIntValue("cashsubdivision_parnas_id");
		public int WarehouseSofiiskayaId => _settingsController.GetIntValue("warehouse_sofiiskaya_id");
		public int WarehouseParnasId => _settingsController.GetIntValue("warehouse_parnas_id");

		//Склад Бугры
		public int WarehouseBugriId => _settingsController.GetIntValue("warehouse_bugri_id");
		public int SouthGeographicGroupId => _settingsController.GetIntValue("south_geographic_group_id");
	}
}
