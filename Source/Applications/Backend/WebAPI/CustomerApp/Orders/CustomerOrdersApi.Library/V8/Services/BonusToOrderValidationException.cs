using System;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <summary>
	/// Ошибка валидации запроса расчёта бонусов
	/// </summary>
	public class BonusToOrderValidationException : Exception
	{
		/// <summary>
		/// Создаёт ошибку валидации запроса расчёта бонусов
		/// </summary>
		/// <param name="message">Текст ошибки для ответа клиенту</param>
		public BonusToOrderValidationException(string message)
			: base(message)
		{
		}
	}
}
