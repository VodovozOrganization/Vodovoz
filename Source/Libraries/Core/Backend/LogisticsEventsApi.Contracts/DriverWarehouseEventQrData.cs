using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LogisticsEventsApi.Contracts
{
	/// <summary>
	/// Данные встроеннные в QR-код
	/// </summary>
	public class DriverWarehouseEventQrData : IValidatableObject
	{
		/// <summary>
		/// Идентификатор события
		/// </summary>
		public int EventId { get; set; }

		/// <summary>
		/// Идентификатор связанного документа
		/// </summary>
		public int? DocumentId { get; set; }

		/// <summary>
		/// Широта
		/// </summary>
		public decimal? Latitude { get; set; }

		/// <summary>
		/// Долгота
		/// </summary>
		public decimal? Longitude { get; set; }

		/// <summary>
		/// Валидация
		/// </summary>
		/// <param name="validationContext">контекст валидации</param>
		/// <returns></returns>
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(EventId == default)
			{
				yield return new ValidationResult("Id события не может быть равным нулю");
			}
		}
	}
}
