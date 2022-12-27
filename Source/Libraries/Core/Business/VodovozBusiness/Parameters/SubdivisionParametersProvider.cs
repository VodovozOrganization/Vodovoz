using System;

namespace Vodovoz.Parameters
{
	public class SubdivisionParametersProvider : ISubdivisionParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public SubdivisionParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int GetOkkId() => _parametersProvider.GetValue<int>("номер_отдела_ОКК");
		public int GetSubdivisionIdForRouteListAccept() => _parametersProvider.GetValue<int>("accept_route_list_subdivision_restrict");
		public int GetParentVodovozSubdivisionId() => _parametersProvider.GetValue<int>("Id_Главного_подразделения_Веселый_Водовоз");
		public int GetSalesSubdivisionId() => _parametersProvider.GetValue<int>("sales_subdivision_id");
		public int GetDevelopersSubdivisionId => _parametersProvider.GetValue<int>("developers_subdivision_id");
		public int GetHumanResourcesSubdivisionId => _parametersProvider.GetValue<int>("human_resources_subdivision_id");
		public int QualityServiceSubdivisionId => _parametersProvider.GetValue<int>("subdivision_quality_service_id");
		public int AuditDepartmentSubdivisionId => _parametersProvider.GetValue<int>("subdivision_audit_department_id");
	}
}
