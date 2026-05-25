using System.ComponentModel.DataAnnotations;

namespace CustomerOrders.Contracts.V5.Carts
{
	/// <summary>
	/// Типы отзвонов за
	/// </summary>
	public enum CallBeforeArrivalMinutesType
	{
		/// <summary>
		/// Не звонить
		/// </summary>
		[Display(Name = "Не звонить")]
		DontCall = -1,
		/// <summary>
		/// 15 минут
		/// </summary>
		[Display(Name = "15 минут")]
		Minutes15 = 15,
		/// <summary>
		/// 30 минут
		/// </summary>
		[Display(Name = "30 минут")]
		Minutes30 = 30,
		/// <summary>
		/// 60 минут
		/// </summary>
		[Display(Name = "час")]
		Minutes60 = 60
	}
}
