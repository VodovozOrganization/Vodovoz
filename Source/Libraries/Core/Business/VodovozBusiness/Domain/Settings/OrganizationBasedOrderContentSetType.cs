using System.ComponentModel.DataAnnotations;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Множества для выборки организации по товарам в заказе
	/// </summary>
	public enum OrganizationBasedOrderContentSetType
	{
		/// <summary>
		/// Первое множество
		/// </summary>
		[Display(Name = "Первое множество")]
		FirstSet = 1,
		/// <summary>
		/// Второе множество
		/// </summary>
		[Display(Name = "Второе множество")]
		SecondSet = 2
	}
}
