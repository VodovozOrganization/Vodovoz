using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Тип ошибки выполнения запроса
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ErrorTypeDto
	{
		/// <summary>
		/// Требуется предварительная авторизация
		/// </summary>
		notAuthenticated,

		/// <summary>
		/// Ошибка на стороне сервиса API
		/// </summary>
		internalError,

		/// <summary>
		/// Запрашиваемый объект не существует
		/// </summary>
		notFound,

		/// <summary>
		/// Дублирование запросов
		/// </summary>
		duplicateConflict,

		/// <summary>
		/// Доступ к запрашиваемым данным запрещен для пользователя
		/// </summary>
		accessDenied,

		/// <summary>
		/// В запросе были переданы некорректные данные
		/// </summary>
		validationFailed
	}
}
