using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class UserSettingsMap : ClassMap<UserSettings>
	{
		public UserSettingsMap()
		{
			Table("user_settings");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.ToolbarStyle).Column("toolbar_style");
			Map(x => x.ToolBarIconsSize).Column("toolbar_icons_size");
			Map(x => x.DefaultSaleCategory).Column("default_sale_category");
			Map(x => x.LogisticDeliveryOrders).Column("logistic_delivery_orders");
			Map(x => x.LogisticServiceOrders).Column("logistic_service_orders");
			Map(x => x.LogisticChainStoreOrders).Column("logistic_chainstore_orders");
			Map(x => x.UseEmployeeSubdivision).Column("use_employee_subdivision");
			Map(x => x.DefaultComplaintStatus).Column("default_complaint_status");
			Map(x => x.ReorderTabs).Column("reorder_tabs");
			Map(x => x.HighlightTabsWithColor).Column("highlight_tabs_with_color");
			Map(x => x.KeepTabColor).Column("keep_tab_color");
			Map(x => x.HideComplaintNotification).Column("hide_complaint_notification");
			Map(x => x.SalesBySubdivisionsAnalitycsReportWarehousesString).Column("sales_by_subdivisions_analitycs_report_warehouses");
			Map(x => x.SalesBySubdivisionsAnalitycsReportSubdivisionsString).Column("sales_by_subdivisions_analitycs_report_subdivisions");
			Map(x => x.MovementDocumentsNotificationUserSelectedWarehousesString).Column("movement_documents_notification_user_selected_warehouses");
			Map(x => x.CarIsNotAtLineReportIncludedEventTypeIdsString).Column("car_is_not_at_line_report_included_event_type_ids");
			Map(x => x.CarIsNotAtLineReportExcludedEventTypeIdsString).Column("car_is_not_at_line_report_excluded_event_type_ids");
			Map(x => x.FuelControlApiLogin).Column("fuel_control_api_login");
			Map(x => x.FuelControlApiPassword).Column("fuel_control_api_password");
			Map(x => x.FuelControlApiKey).Column("fuel_control_api_key");
			Map(x => x.FuelControlApiSessionId).Column("fuel_control_api_session_id");
			Map(x => x.FuelControlApiSessionExpirationDate).Column("fuel_control_api_session_expiration_date");
			Map(x => x.DefaultSubdivisionId).Column("default_subdivision_id");
			Map(x => x.DefaultCounterpartyId).Column("default_counterparty_id");

			References(x => x.User).Column("user_id");
			References(x => x.DefaultWarehouse).Column("default_warehouse_id");

			HasMany(x => x.CashSubdivisionSortingSettings).KeyColumn("user_settings_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad().OrderBy("sorting_index");

			HasMany(x => x.DocumentPrinterSettings).KeyColumn("user_settings_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad().OrderBy("id");
		}
	}
}
