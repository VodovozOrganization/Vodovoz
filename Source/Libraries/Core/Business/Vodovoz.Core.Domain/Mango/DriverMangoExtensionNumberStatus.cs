using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Mango
{
	/// <summary>
	/// Статус добавочного номера Манго для водителя
	/// </summary>
	public enum DriverMangoExtensionNumberStatus
	{
		/// <summary>
		/// Активен
		/// </summary>
		[Display(Name = "Активен")]
		Active,

		/// <summary>
		/// Деактивирован
		/// </summary>
		[Display(Name = "Деактивирован")]
		Deactivated
	}
}
