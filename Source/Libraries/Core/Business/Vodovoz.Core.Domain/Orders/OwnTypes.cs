using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Принадлежность
	/// </summary>
	public enum OwnTypes
	{
		/// <summary>
		/// Не указано
		/// </summary>
		[Display(Name = "")]
		None,
		/// <summary>
		/// Клиент
		/// </summary>
		[Display(Name = "Клиент")]
		Client,
		/// <summary>
		/// Дежурный
		/// </summary>
		[Display(Name = "Дежурный")]
		Duty,
		/// <summary>
		/// Аренда
		/// </summary>
		[Display(Name = "Аренда")]
		Rent
	}
}
