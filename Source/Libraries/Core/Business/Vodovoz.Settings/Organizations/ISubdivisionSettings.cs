namespace Vodovoz.Settings.Organizations
{
	public interface ISubdivisionSettings
	{
		int GetDevelopersSubdivisionId { get; }
		int GetHumanResourcesSubdivisionId { get; }
		int QualityServiceSubdivisionId { get; }
		int AuditDepartmentSubdivisionId { get; }
		int CashSubdivisionBCId { get; }
		int CashSubdivisionBCSofiyaId { get; }

		int GetOkkId();
		int GetSubdivisionIdForRouteListAccept();
		int GetParentVodovozSubdivisionId();
		int GetSalesSubdivisionId();

		int LogisticSubdivisionSofiiskayaId { get; }
		int LogisticSubdivisionBugriId { get; }
	}
}
