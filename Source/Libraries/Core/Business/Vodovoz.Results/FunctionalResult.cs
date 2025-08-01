using System;

namespace Vodovoz.Results
{
	/// <summary>
	/// Функциональные методы для результатов
	/// </summary>
	public static class FunctionalResult
	{
		/// <summary>
		/// Метод маппинга данных результата с данными типа T1 в результат с типом данных T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией mapFunc</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="map">Функция маппинга результата с данными типа T1 в результат с типом данных T2</param>
		/// <returns></returns>
		public static Result<T2, TError> Map<T1, T2, TError>(
			this Result<T1, TError> result,
			Func<T1, T2> map)
			=> result.IsSuccess
				? Result<T2, TError>.Success(map(result.Value))
				: Result<T2, TError>.Failure(result.Error);

		/// <summary>
		/// Метод для обработки результата типа T1 в тип T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией bindFunc</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="bind">Функция обработки результата с данными типа T1, возвращающая результат с данными типа T2</param>
		/// <returns>Результат с данными типа T2</returns>
		public static Result<T2, TError> Bind<T1, T2, TError>(
			this Result<T1, TError> result,
			Func<T1, Result<T2, TError>> bind)
			=> result.IsSuccess
				? bind(result.Value)
				: Result<T2, TError>.Failure(result.Error);

		/// <summary>
		/// Метод для маппинга ошибки из типа TError в тип TNewError
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TError"></typeparam>
		/// <typeparam name="TNewError"></typeparam>
		/// <param name="result"></param>
		/// <param name="map"></param>
		/// <returns></returns>
		public static Result<T, TNewError> MapError<T, TError, TNewError>(
			this Result<T, TError> result,
			Func<TError, TNewError> map)
			=> result.IsSuccess
				? Result<T, TNewError>.Success(result.Value)
				: Result<T, TNewError>.Failure(map(result.Error));

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TResult">Результат выполнения обработки результата</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Функция для обработки при успешном результате</param>
		/// <param name="onFailure">Функция для обработки при неуспешном результате</param>
		/// <returns>Результат с данными типа T2</returns>
		public static TResult Match<T, TError, TResult>(
			this Result<T, TError> result,
			Func<T, TResult> onSuccess,
			Func<TError, TResult> onFailure)
			=> result.IsSuccess
				? onSuccess(result.Value)
				: onFailure(result.Error);
	}
}
