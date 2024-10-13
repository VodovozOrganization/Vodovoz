using System.ComponentModel.DataAnnotations;

namespace VodovozBusiness.Domain.Orders
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
