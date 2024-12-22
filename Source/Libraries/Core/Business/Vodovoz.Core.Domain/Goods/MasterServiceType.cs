using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Тип выезда мастера
	/// </summary>
	public enum MasterServiceType
	{
		[Display(Name = "Санобработка")]
		Cleaning,
		[Display(Name = "Ремонт")]
		Repair
	}
}
