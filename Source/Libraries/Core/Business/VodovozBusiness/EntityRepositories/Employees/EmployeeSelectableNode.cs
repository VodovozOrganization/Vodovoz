using QS.DomainModel.Entity;

namespace Vodovoz.EntityRepositories.Employees
{
	public class EmployeeNode : PropertyChangedBase
	{
		public int Id { get; set; }
		public string LastName { get; set; }
		public string Name { get; set; }
		public string Patronymic { get; set; }
	}
}
