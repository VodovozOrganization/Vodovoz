using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	public enum DriverType
	{
		[Display(Name = "Управляет ТС компании")]
		companydriver,
		[Display(Name = "Управляет ТС в раскате")]
		raskat,
		[Display(Name = "Управляет ТС личным")]
		hireddriver
	}
}
