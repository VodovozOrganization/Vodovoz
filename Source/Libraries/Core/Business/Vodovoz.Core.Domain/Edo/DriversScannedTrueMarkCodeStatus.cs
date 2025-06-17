using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Статус обработки отсканированного водителем кода ЧЗ
	/// </summary>
	public enum DriversScannedTrueMarkCodeStatus
	{
		/// <summary>
		/// Код не обрабатывался
		/// </summary>
		[Display(Name = "Код не обрабатывался")]
		None,
		/// <summary>
		/// Обработка кода завершена успешно
		/// </summary>
		[Display(Name = "Обработка кода завершена успешно")]
		Succeed,
		/// <summary>
		/// Обработка кода завершена с ошибкой
		/// </summary>
		[Display(Name = "Обработка кода завершена с ошибкой")]
		Error
	}
}
