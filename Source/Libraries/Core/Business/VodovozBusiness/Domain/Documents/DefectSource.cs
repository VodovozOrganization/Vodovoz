using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents
{
	public enum DefectSource
	{
		[Display(Name = "")]
		None,
		[Display(Name = "Водитель")]
		Driver,
		[Display(Name = "Клиент")]
		Client,
		[Display(Name = "Производство")]
		Production,
		[Display(Name = "Склад")]
		Warehouse
	}
}

