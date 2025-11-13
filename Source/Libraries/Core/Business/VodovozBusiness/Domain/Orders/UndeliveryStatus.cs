using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Статус недовоза
	/// </summary>
	public enum UndeliveryStatus
	{
		/// <summary>
		/// В работе
		/// </summary>
		[Display(Name = "В работе")]
		InProcess,
		/// <summary>
		/// На проверке
		/// </summary>
		[Display(Name = "На проверке")]
		Checking,
		/// <summary>
		/// Закрыт
		/// </summary>
		[Display(Name = "Закрыт")]
		Closed
	}
}
