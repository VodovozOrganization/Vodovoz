using System;
using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Results
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
		/// <param name="mapFunc">Функция маппинга результата с данными типа T1 в результат с типом данных T2</param>
		/// <returns></returns>
		public static Result<T2> Map<T1, T2>(
			this Result<T1> result,
			Func<T1, T2> mapFunc)
			=> result.IsSuccess
				? Result.Success(mapFunc(result.Value))
				: Result.Failure<T2>(result.Errors);

		/// <summary>
		/// Метод для обработки результата типа T1 в тип T2
		/// </summary>
		/// <typeparam name="T1">Тип данных результата для обработки функцией bindFunc</typeparam>
		/// <typeparam name="T2">Итоговый тип данных</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="bindFunc">Функция обработки результата с данными типа T1, возвращающая результат с данными типа T2</param>
		/// <returns>Результат с данными типа T2</returns>
		public static Result<T2> Bind<T1, T2>(
			this Result<T1> result,
			Func<T1, Result<T2>> bindFunc) 
			=> result.IsSuccess
				? bindFunc(result.Value)
				: Result.Failure<T2>(result.Errors);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TResult">Результат выполнения обработки результата</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Функция для обработки при успешном результате</param>
		/// <param name="onFailure">Функция для обработки при неуспешном результате</param>
		/// <returns>Результат с данными типа T2</returns>
		public static TResult Match<T, TResult>(
			this Result<T> result,
			Func<T, TResult> onSuccess,
			Func<IEnumerable<Error>, TResult> onFailure)
			=> result.IsSuccess
				? onSuccess(result.Value)
				: onFailure(result.Errors);

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Действие для обработки при успешном результате</param>
		/// <param name="onFailure">Действие для обработки при неуспешном результате</param>
		public static void Match<T>(
			this Result<T> result,
			Action<T> onSuccess,
			Action<IEnumerable<Error>> onFailure)
		{
			if(result.IsSuccess)
			{
				onSuccess(result.Value);
			}
			else
			{
				onFailure(result.Errors);
			}
		}

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <typeparam name="T">Тип данных результата</typeparam>
		/// <typeparam name="TResult">Результат выполнения обработки результата</typeparam>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Функция для обработки при успешном результате</param>
		/// <param name="onFailure">Действие для обработки при неуспешном результате</param>
		/// <returns>При успешном выполнении возвращает TResult, в противном случае значение по умолчанию</returns>
		public static TResult Match<T, TResult>(
			this Result<T> result,
			Func<T, TResult> onSuccess,
			Action<IEnumerable<Error>> onFailure)
		{
			if(result.IsSuccess)
			{
				return onSuccess(result.Value);
			}
			else
			{
				onFailure(result.Errors);
				return default;
			}
		}

		/// <summary>
		/// Метод для обработки результата
		/// </summary>
		/// <param name="result">Результат</param>
		/// <param name="onSuccess">Действие при успешном результате</param>
		/// <param name="onFailure">Действие при неуспешном результате</param>
		public static void Match(
			this Result result,
			Action onSuccess,
			Action<IEnumerable<Error>> onFailure)
		{
			if(result.IsSuccess)
			{
				onSuccess();
			}
			else
			{
				onFailure(result.Errors);
			}
		}
	}
}
