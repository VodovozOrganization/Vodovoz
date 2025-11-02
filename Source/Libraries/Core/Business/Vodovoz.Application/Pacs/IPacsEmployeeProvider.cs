namespace Vodovoz.Application.Pacs
{
	public interface IPacsEmployeeProvider
	{
		int? EmployeeId { get; }
		bool IsAdministrator { get; }
		bool IsOperator { get; }
	}
}
