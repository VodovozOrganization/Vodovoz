using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.BasicHandbooks
{
	/// <summary>
	/// Тип гарантийного талона
	/// </summary>
	public enum WarrantyCardType
	{
		[Display(Name="Нет")]
		WithoutCard,
		[Display (Name = "Гарантийный талон на кулера")]
		CoolerWarranty,
		[Display (Name = "Гарантийный талон на помпы")]
		PumpWarranty
	}
}
