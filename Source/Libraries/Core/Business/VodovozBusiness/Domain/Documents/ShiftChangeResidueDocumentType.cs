using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents
{
	public enum ShiftChangeResidueDocumentType
	{
		[Display(Name = "По складу")]
		Warehouse,
		[Display(Name = "По автомобилю")]
		Car
	}
}
