using System;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Settings.Database.Organizations
{
	public class SubdivisionSettings : ISubdivisionSettings
	{
		private readonly ISettingsController _settingsController;

		public SubdivisionSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int GetOkkId() => _settingsController.GetValue<int>("номер_отдела_ОКК");
		public int GetSubdivisionIdForRouteListAccept() => _settingsController.GetValue<int>("accept_route_list_subdivision_restrict");
		public int GetParentVodovozSubdivisionId() => _settingsController.GetValue<int>("Id_Главного_подразделения_Веселый_Водовоз");
		public int GetSalesSubdivisionId() => _settingsController.GetValue<int>("sales_subdivision_id");
		public int GetDevelopersSubdivisionId => _settingsController.GetValue<int>("developers_subdivision_id");
		public int GetHumanResourcesSubdivisionId => _settingsController.GetValue<int>("human_resources_subdivision_id");
		public int QualityServiceSubdivisionId => _settingsController.GetValue<int>("subdivision_quality_service_id");
		public int AuditDepartmentSubdivisionId => _settingsController.GetValue<int>("subdivision_audit_department_id");
		public int CashSubdivisionBCId => _settingsController.GetValue<int>("Subdivision.CashSubdivisionBCId");
		public int CashSubdivisionBCSofiyaId => _settingsController.GetValue<int>("Subdivision.CashSubdivisionBCSofiyaId");

		public int LogisticSubdivisionSofiiskayaId => _settingsController.GetValue<int>("LogisticSubdivisionSofiiskayaId");
		public int LogisticSubdivisionBugriId => _settingsController.GetValue<int>("LogisticSubdivisionBugriId");
	}
}
