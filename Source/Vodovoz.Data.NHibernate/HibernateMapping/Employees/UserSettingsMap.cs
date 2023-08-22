using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class UserSettingsMap : ClassMap<UserSettings>
	{
		public UserSettingsMap()
		{
			Table("user_settings");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.ToolbarStyle).Column("toolbar_style").CustomType<ToolbarStyleStringType>();
			Map(x => x.ToolBarIconsSize).Column("toolbar_icons_size").CustomType<ToolBarIconsSizeStringType>();
			Map(x => x.DefaultSaleCategory).Column("default_sale_category");
			Map(x => x.LogisticDeliveryOrders).Column("logistic_delivery_orders");
			Map(x => x.LogisticServiceOrders).Column("logistic_service_orders");
			Map(x => x.LogisticChainStoreOrders).Column("logistic_chainstore_orders");
			Map(x => x.UseEmployeeSubdivision).Column("use_employee_subdivision");
			Map(x => x.DefaultComplaintStatus).Column("default_complaint_status").CustomType<ComplaintStatusesStringType>();
			Map(x => x.ReorderTabs).Column("reorder_tabs");
			Map(x => x.HighlightTabsWithColor).Column("highlight_tabs_with_color");
			Map(x => x.KeepTabColor).Column("keep_tab_color");
			Map(x => x.HideComplaintNotification).Column("hide_complaint_notification");
			Map(x => x.SalesBySubdivisionsAnalitycsReportWarehousesString).Column("sales_by_subdivisions_analitycs_report_warehouses");
			Map(x => x.SalesBySubdivisionsAnalitycsReportSubdivisionsString).Column("sales_by_subdivisions_analitycs_report_subdivisions");
			References(x => x.User).Column("user_id");
			References(x => x.DefaultWarehouse).Column("default_warehouse_id");
			References(x => x.DefaultSubdivision).Column("default_subdivision_id");
			References(x => x.DefaultCounterparty).Column("default_counterparty_id");

			HasMany(x => x.CashSubdivisionSortingSettings).KeyColumn("user_settings_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad().OrderBy("sorting_index");
		}
	}
}
