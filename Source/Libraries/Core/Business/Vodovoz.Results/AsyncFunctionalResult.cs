using System;
using System.Threading.Tasks;

namespace Vodovoz.Results
{
	/// <summary>
	/// Функциональные методы для асинхронных результатов
	/// </summary>
	public static class AsyncFunctionalResult
	{
		/// <summary>
		/// Метод маппинга данных результата с данными типа T1 в результат с типом данных T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией map</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="map">Функция маппинга результата с данными типа T1 в результат с типом данных T2</param>
		/// <returns></returns>
		public static async Task<Result<T2, TError>> MapAsync<T1, T2, TError>(
			this Task<Result<T1, TError>> result,
			Func<T1, T2> map)
			=> (await result).Map(map);

		/// <summary>
		/// Метод маппинга данных результата с данными типа T1 в результат с типом данных T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией mapAsync</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="mapAsync">Асинхронная функция маппинга результата с данными типа T1 в результат с типом данных T2</param>
		/// <returns></returns>
		public static async Task<Result<T2, TError>> MapAsync<T1, T2, TError>(
			this Result<T1, TError> result,
			Func<T1, Task<T2>> mapAsync)
			=> result.IsSuccess
				? Result<T2, TError>.Success(await mapAsync(result.Value))
				: Result<T2, TError>.Failure(result.Error);

		/// <summary>
		/// Метод маппинга данных результата с данными типа T1 в результат с типом данных T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией mapAsync</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="mapAsync">Асинхронная функция маппинга результата с данными типа T1 в результат с типом данных T2</param>
		/// <returns></returns>
		public static async Task<Result<T2, TError>> MapAsync<T1, T2, TError>(
			this Task<Result<T1, TError>> result,
			Func<T1, Task<T2>> mapAsync)
			=> await (await result).MapAsync(mapAsync);

		/// <summary>
		/// Метод для обработки результата с данными типа T1 в тип T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией bind</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="bind">Функция обработки  результата с данными типа T1 в тип T2</param>
		/// <returns></returns>
		public static async Task<Result<T2, TError>> BindAsync<T1, T2, TError>(
			this Task<Result<T1, TError>> result,
			Func<T1, Result<T2, TError>> bind)
			=> (await result).Bind(bind);

		/// <summary>
		/// Метод для обработки результата с данными типа T1 в тип T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией bindAsync</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="bindAsync">Асинхронная функция обработки  результата с данными типа T1 в тип T2</param>
		/// <returns></returns>
		public static async Task<Result<T2, TError>> BindAsync<T1, T2, TError>(
			this Result<T1, TError> result,
			Func<T1, Task<Result<T2, TError>>> bindAsync)
			=> result.IsSuccess
				? await bindAsync(result.Value)
				: Result<T2, TError>.Failure(result.Error);

		/// <summary>
		/// Метод для обработки результата с данными типа T1 в тип T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией bindAsync</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="bindAsync">Асинхронная функция обработки  результата с данными типа T1 в тип T2</param>
		/// <returns></returns>
		public static async Task<Result<T2, TError>> BindAsync<T1, T2, TError>(
			this Task<Result<T1, TError>> result,
			Func<T1, Task<Result<T2, TError>>> bindAsync)
			=> await (await result).BindAsync(bindAsync);

		/// <summary>
		/// Метод для маппинга ошибки из типа TError в тип TNewError
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <typeparam name="TNewError">Новый тип ошибки</typeparam>
		/// <param name="result"></param>
		/// <param name="map"></param>
		/// <returns></returns>
		public static async Task<Result<T, TNewError>> MapErrorAsync<T, TError, TNewError>(
			this Task<Result<T, TError>> result,
			Func<TError, TNewError> map)
			=> (await result).MapError(map);

		/// <summary>
		/// Метод для маппинга ошибки из типа TError в тип TNewError
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <typeparam name="TNewError">Новый тип ошибки</typeparam>
		/// <param name="result"></param>
		/// <param name="mapAsync"></param>
		/// <returns></returns>
		public static async Task<Result<T, TNewError>> MapErrorAsync<T, TError, TNewError>(
			this Result<T, TError> result,
			Func<TError, Task<TNewError>> mapAsync)
			=> result.IsSuccess
				? Result<T, TNewError>.Success(result.Value)
				: Result<T, TNewError>.Failure(await mapAsync(result.Error));

		/// <summary>
		/// Метод для маппинга ошибки из типа TError в тип TNewError
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <typeparam name="TNewError">Новый тип ошибки</typeparam>
		/// <param name="result"></param>
		/// <param name="mapAsync"></param>
		/// <returns></returns>
		public static async Task<Result<T, TNewError>> MapErrorAsync<T, TError, TNewError>(
			this Task<Result<T, TError>> result,
			Func<TError, Task<TNewError>> mapAsync)
			=> await (await result).MapErrorAsync(mapAsync);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Функция обработки при успешном результате</param>
		/// <param name="onFailure">Функция обработки при неуспешном результате</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TError, TResult>(
			this Task<Result<T, TError>> result,
			Func<T, TResult> onSuccess,
			Func<TError, TResult> onFailure)
			=> (await result).Match(onSuccess, onFailure);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccessAsync">Функция асинхронной обработки успешного ответа</param>
		/// <param name="onFailure">Функция обработки при неуспешном результате</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TError, TResult>(
			this Result<T, TError> result,
			Func<T, Task<TResult>> onSuccessAsync,
			Func<TError, TResult> onFailure)
			=> result.IsSuccess
				? await onSuccessAsync(result.Value)
				: onFailure(result.Error);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Функция обработки при успешном результате</param>
		/// <param name="onFailureAsync">Функция асинхронной обработки неуспешного ответа</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TError, TResult>(
			this Result<T, TError> result,
			Func<T, TResult> onSuccess,
			Func<TError, Task<TResult>> onFailureAsync)
			=> result.IsSuccess
				? onSuccess(result.Value)
				: await onFailureAsync(result.Error);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccessAsync">Функция асинхронной обработки успешного ответа</param>
		/// <param name="onFailure">Функция обработки при неуспешном результате</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TError, TResult>(
			this Task<Result<T, TError>> result,
			Func<T, Task<TResult>> onSuccessAsync,
			Func<TError, TResult> onFailure)
			=> await (await result).MatchAsync(onSuccessAsync, onFailure);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Функция обработки при успешном результате</param>
		/// <param name="onFailureAsync">Функция асинхронной обработки неуспешного ответа</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TError, TResult>(
			this Task<Result<T, TError>> result,
			Func<T, TResult> onSuccess,
			Func<TError, Task<TResult>> onFailureAsync)
			=> await (await result).MatchAsync(onSuccess, onFailureAsync);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccessAsync">Функция асинхронной обработки успешного ответа</param>
		/// <param name="onFailureAsync">Функция асинхронной обработки неуспешного ответа</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TError, TResult>(
			this Result<T, TError> result,
			Func<T, Task<TResult>> onSuccessAsync,
			Func<TError, Task<TResult>> onFailureAsync)
			=> result.IsSuccess
				? await onSuccessAsync(result.Value)
				: await onFailureAsync(result.Error);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TError">Тип ошибки</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccessAsync">Функция асинхронной обработки успешного ответа</param>
		/// <param name="onFailureAsync">Функция асинхронной обработки неуспешного ответа</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TError, TResult>(
			this Task<Result<T, TError>> result,
			Func<T, Task<TResult>> onSuccessAsync,
			Func<TError, Task<TResult>> onFailureAsync)
			=> await (await result).MatchAsync(onSuccessAsync, onFailureAsync);
	}
}
