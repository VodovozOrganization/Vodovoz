namespace Vodovoz.Settings.Complaints
{
	public interface IComplaintSettings
	{
		int SubdivisionResponsibleId { get; }
		int EmployeeResponsibleId { get; }
		int ComplaintResultOfEmployeesIsGuiltyId { get; }
		int GuiltProvenComplaintResultId { get; }
		int IncomeCallComplaintSourceId { get; }
	}
}
