using System;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors
{
	[Obsolete("Грубо нарушает принцип паттерна Result, не использовать")]
	public class RequestProcessingResult<TValue> : RequestProcessingResult
	{
		private readonly Result<TValue> _result;
		private readonly TValue _failureData;

		/// <summary>
		/// Создание объекта успешного результата обработки запроса
		/// </summary>
		/// <param name="result">Результат обработки запроса</param>
		/// <exception cref="ArgumentNullException">Ошибка если результат null</exception>
		/// <exception cref="ArgumentException">Ошибка если результат провальный</exception>
		protected internal RequestProcessingResult(Result<TValue> result)
		{
			_result = result ?? throw new ArgumentNullException(nameof(result));

			if(result.IsFailure)
			{
				throw new ArgumentException("В данный контруктор разрешена передача только успешного результата", nameof(result));
			}
		}

		/// <summary>
		/// Создание объекта провального результата обработки запроса
		/// </summary>
		/// <param name="result">Результат обработки запроса</param>
		/// <param name="failureData">Дополнительный объект с данными провального результата</param>
		/// <exception cref="ArgumentNullException">Ошибка если результат null</exception>
		/// <exception cref="ArgumentException">Ошибка если результат успешный</exception>
		protected internal RequestProcessingResult(Result<TValue> result, TValue failureData)
		{
			_result = result ?? throw new ArgumentNullException(nameof(result));
			_failureData = failureData;

			if(result.IsSuccess)
			{
				throw new ArgumentException("В данный контруктор разрешена передача только провального результата", nameof(result));
			}
		}

		/// <summary>
		/// Результат обработки запроса
		/// </summary>
		public Result<TValue> Result => _result;

		/// <summary>
		/// Дополнительный объект с данными провального результата
		/// </summary>
		public TValue FailureData =>
			(Result is null || Result.IsSuccess)
			? throw new InvalidOperationException("The failure data can't be accessed")
			: _failureData;
	}
}
