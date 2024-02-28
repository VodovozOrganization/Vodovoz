namespace Vodovoz.Settings.Organizations
{
	public interface ISubdivisionSettings
	{
		int GetDevelopersSubdivisionId { get; }
		int GetHumanResourcesSubdivisionId { get; }
		int QualityServiceSubdivisionId { get; }
		int AuditDepartmentSubdivisionId { get; }

		int GetOkkId();
		int GetSubdivisionIdForRouteListAccept();
		int GetParentVodovozSubdivisionId();
		int GetSalesSubdivisionId();
	}
}
