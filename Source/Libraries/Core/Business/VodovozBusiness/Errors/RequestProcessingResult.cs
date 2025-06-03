using System;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors
{
	[Obsolete("Грубо нарушает принцип паттерна Result, не использовать")]
	public class RequestProcessingResult
	{
		protected internal RequestProcessingResult() { }

		/// <summary>
		/// Создание успешного результата обработки запроса
		/// </summary>
		/// <typeparam name="TValue">Тип результата обработки запроса (объект, содержащий данные ответа)</typeparam>
		/// <param name="result">Результат обработки запроса</param>
		/// <returns>Успешный результат</returns>
		public static RequestProcessingResult<TValue> CreateSuccess<TValue>(Result<TValue> result) =>
			new RequestProcessingResult<TValue>(result);

		/// <summary>
		/// Создание провального результата обработки запроса
		/// </summary>
		/// <typeparam name="TValue">Тип результата обработки запроса 
		/// (данные успешного результата и доп. объект с данными провального результата)</typeparam>
		/// <param name="result">Результат обработки запроса</param>
		/// <param name="failureValue">Дополнительный объект с данными провального результата</param>
		/// <returns>Провальный результат</returns>
		public static RequestProcessingResult<TValue> CreateFailure<TValue>(Result<TValue> result, TValue failureValue) =>
			new RequestProcessingResult<TValue>(result, failureValue);
	}
}
