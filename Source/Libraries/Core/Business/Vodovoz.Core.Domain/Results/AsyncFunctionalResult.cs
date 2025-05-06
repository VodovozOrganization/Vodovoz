using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vodovoz.Core.Domain.Results
{
	/// <summary>
	/// Функциональные методы для асинхронных результатов
	/// </summary>
	public static class AsyncFunctionalResult
	{
		/// <summary>
		/// Метод маппинга данных результата с данными типа T1 в результат с типом данных T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией mapFunc</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="mapFunc">Функция маппинга результата с данными типа T1 в результат с типом данных T2</param>
		/// <returns></returns>
		public static async Task<Result<T2>> MapAsync<T1, T2>(
			this Task<Result<T1>> result,
			Func<T1, T2> mapFunc)
			=> (await result).Map(mapFunc);

		/// <summary>
		/// Метод маппинга данных результата с данными типа T1 в результат с типом данных T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией mapFuncAsync</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="mapFuncAsync">Асинхронная функция маппинга результата с данными типа T1 в результат с типом данных T2</param>
		/// <returns></returns>
		public static async Task<Result<T2>> MapAsync<T1, T2>(
			this Result<T1> result,
			Func<T1, Task<T2>> mapFuncAsync)
			=> result.IsSuccess
				? Result.Success(await mapFuncAsync(result.Value))
				: Result.Failure<T2>(result.Errors);

		/// <summary>
		/// Метод маппинга данных результата с данными типа T1 в результат с типом данных T2
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="mapFuncAsync">Асинхронная функция маппинга результата с данными типа T1 в результат с типом данных T2</param>
		/// <returns></returns>
		public static async Task<Result<T2>> MapAsync<T1, T2>(
			this Task<Result<T1>> result,
			Func<T1, Task<T2>> mapFuncAsync)
			=> await (await result).MapAsync(mapFuncAsync);

		/// <summary>
		/// Метод для обработки результата с данными типа T1 в тип T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией bindFunc</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="bindFunc">Функция обработки  результата с данными типа T1 в тип T2</param>
		/// <returns></returns>
		public static async Task<Result<T2>> BindAsync<T1, T2>(
			this Task<Result<T1>> result,
			Func<T1, Result<T2>> bindFunc)
			=> (await result).Bind(bindFunc);

		/// <summary>
		/// Метод для обработки результата с данными типа T1 в тип T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией bindFuncAsync</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="bindFuncAsync">Асинхронная функция обработки  результата с данными типа T1 в тип T2</param>
		/// <returns></returns>
		public static async Task<Result<T2>> BindAsync<T1, T2>(
			this Result<T1> result,
			Func<T1, Task<Result<T2>>> bindFuncAsync)
			=> result.IsSuccess
				? await bindFuncAsync(result.Value)
				: Result.Failure<T2>(result.Errors);

		/// <summary>
		/// Метод для обработки результата с данными типа T1 в тип T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией bindFuncAsync</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="bindFuncAsync">Асинхронная функция обработки  результата с данными типа T1 в тип T2</param>
		/// <returns></returns>
		public static async Task<Result<T2>> BindAsync<T1, T2>(
			this Task<Result<T1>> result,
			Func<T1, Task<Result<T2>>> bindFuncAsync)
			=> await (await result).BindAsync(bindFuncAsync);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Функция обработки при успешном результате</param>
		/// <param name="onFailure">Функция обработки при неуспешном результате</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TResult>(
			this Task<Result<T>> result,
			Func<T, TResult> onSuccess,
			Func<IEnumerable<Error>, TResult> onFailure)
			=> (await result).Match(onSuccess, onFailure);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccessAsync">Функция асинхронной обработки успешного ответа</param>
		/// <param name="onFailure">Функция обработки при неуспешном результате</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TResult>(
			this Result<T> result,
			Func<T, Task<TResult>> onSuccessAsync,
			Func<IEnumerable<Error>, TResult> onFailure)
			=> result.IsSuccess
				? await onSuccessAsync(result.Value)
				: onFailure(result.Errors);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Функция обработки при успешном результате</param>
		/// <param name="onFailureAsync">Функция асинхронной обработки неуспешного ответа</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TResult>(
			this Result<T> result,
			Func<T, TResult> onSuccess,
			Func<IEnumerable<Error>, Task<TResult>> onFailureAsync)
			=> result.IsSuccess
				? onSuccess(result.Value)
				: await onFailureAsync(result.Errors);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccessAsync">Функция асинхронной обработки успешного ответа</param>
		/// <param name="onFailure">Функция обработки при неуспешном результате</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TResult>(
			this Task<Result<T>> result,
			Func<T, Task<TResult>> onSuccessAsync,
			Func<IEnumerable<Error>, TResult> onFailure)
			=> await (await result).MatchAsync(onSuccessAsync, onFailure);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Функция обработки при успешном результате</param>
		/// <param name="onFailureAsync">Функция асинхронной обработки неуспешного ответа</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TResult>(
			this Task<Result<T>> result,
			Func<T, TResult> onSuccess,
			Func<IEnumerable<Error>, Task<TResult>> onFailureAsync)
			=> await (await result).MatchAsync(onSuccess, onFailureAsync);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccessAsync">Функция асинхронной обработки успешного ответа</param>
		/// <param name="onFailureAsync">Функция асинхронной обработки неуспешного ответа</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TResult>(
			this Result<T> result,
			Func<T, Task<TResult>> onSuccessAsync,
			Func<IEnumerable<Error>, Task<TResult>> onFailureAsync)
			=> result.IsSuccess
				? await onSuccessAsync(result.Value)
				: await onFailureAsync(result.Errors);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TResult">Итоговый тип обернутый в <see cref="Task"/></typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccessAsync">Функция асинхронной обработки успешного ответа</param>
		/// <param name="onFailureAsync">Функция асинхронной обработки неуспешного ответа</param>
		/// <returns></returns>
		public static async Task<TResult> MatchAsync<T, TResult>(
			this Task<Result<T>> result,
			Func<T, Task<TResult>> onSuccessAsync,
			Func<IEnumerable<Error>, Task<TResult>> onFailureAsync)
			=> await (await result).MatchAsync(onSuccessAsync, onFailureAsync);
	}
}
