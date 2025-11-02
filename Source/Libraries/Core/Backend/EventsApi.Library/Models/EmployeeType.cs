using System.ComponentModel.DataAnnotations;

namespace EventsApi.Library.Models
{
	public enum EmployeeType
	{
		[Display(Name = "водитель")]
		Driver,
		[Display(Name = "сотрудник склада")]
		WarehouseEmployee
	}
}
