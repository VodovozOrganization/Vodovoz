using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	/// <summary>
	/// Роли пользователей в заявке на выдачу средств
	/// </summary>
	public enum PayoutRequestUserRole
	{
		/// <summary>
		/// Заявитель
		/// </summary>
		[Display(Name = "Заявитель")]
		RequestCreator,

		/// <summary>
		/// Руководитель отдела
		/// </summary>
		[Display(Name = "Руководитель отдела")]
		SubdivisionChief,

		/// <summary>
		/// Согласователь
		/// </summary>
		[Display(Name = "Согласователь")]
		Coordinator,

		/// <summary>
		/// Финансист
		/// </summary>
		[Display(Name = "Финансист")]
		Financier,

		/// <summary>
		/// Кассир
		/// </summary>
		[Display(Name = "Кассир")]
		Cashier,

		/// <summary>
		/// Другие
		/// </summary>
		[Display(Name = "Другие")]
		Other,

		/// <summary>
		/// Бухгалтер
		/// </summary>
		[Display(Name = "Бухгалтер")]
		Accountant,

		/// <summary>
		/// Служба безопасности
		/// </summary>
		[Display(Name = "Служба безопасности")]
		SecurityService,
	}
}
