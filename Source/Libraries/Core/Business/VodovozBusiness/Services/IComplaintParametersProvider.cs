namespace Vodovoz.Services
{
	public interface IComplaintParametersProvider
	{
		int SubdivisionResponsibleId { get; }
		int EmployeeResponsibleId { get; }
		int ComplaintResultOfEmployeesIsGuiltyId { get; }
	}
}
