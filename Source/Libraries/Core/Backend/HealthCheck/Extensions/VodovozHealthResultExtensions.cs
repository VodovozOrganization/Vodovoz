using System.Collections.Generic;
using System.Linq;
using VodovozHealthCheck.Dto;

namespace VodovozHealthCheck.Extensions
{
	/// <summary>
	///		Набор расширений для удобной работы с результатами проверки работоспособности (`VodovozHealthResultDto`).
	///		Содержит методы преобразования результата в формат ответа и удобные методы добавления ошибок.
	/// </summary>
	public static class VodovozHealthResultExtensions
	{
		/// <summary>
		///		Преобразует результат проверки в словарь для неуспешного ответа (например, для отправки в системы мониторинга вроде Zabbix).
		/// </summary>
		/// <param name="healthResult">Результат проверки работоспособности.</param>
		/// <returns>
		///		Словарь с ключом "results" и коллекцией дополнительных сообщений об ошибках,
		///		или <c>null</c>, если дополнительных сообщений нет.
		/// </returns>
		public static Dictionary<string, object> ToUnhealthyDataInfoResponse(this VodovozHealthResultDto healthResult)
		{
			Dictionary<string, object> dictionary = null;

			if(Enumerable.Any(healthResult.AdditionalUnhealthyResults))
			{
				dictionary = new Dictionary<string, object>
				{
					{
						"results",
						healthResult.AdditionalUnhealthyResults
					}
				};
			}

			return dictionary;
		}

		/// <summary>
		///		Добавляет в результат проверки дополнительное сообщение об ошибке, если проверка уже помечена как неуспешная.
		/// </summary>
		/// <param name="healthResult">Результат проверки работоспособности.</param>
		/// <param name="errorMessage">Сообщение об ошибке, добавляемое в коллекцию. По умолчанию: "Не прошёл валидацию".</param>
		/// <returns>Тот же экземпляр <see cref="VodovozHealthResultDto"/> с добавленным сообщением (если применимо).</returns>
		public static VodovozHealthResultDto AddUnhealthyValidationMessage(
			this VodovozHealthResultDto healthResult,
			string errorMessage = "Не прошёл валидацию")
		{
			if(!healthResult.IsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add(errorMessage);
			}

			return healthResult;
		}

		/// <summary>
		///		Добавляет одно сообщение об ошибке в результат проверки и помечает результат как неуспешный.
		/// </summary>
		/// <param name="result">Результат проверки работоспособности.</param>
		/// <param name="errorMessage">Сообщение об ошибке. По умолчанию: "Не прошёл валидацию".</param>
		/// <returns>Тот же экземпляр <see cref="VodovozHealthResultDto"/> с добавленным сообщением и <see cref="VodovozHealthResultDto.IsHealthy"/> = false.</returns>
		public static VodovozHealthResultDto WithError(this VodovozHealthResultDto result, string errorMessage = "Не прошёл валидацию")
		{
			result.AdditionalUnhealthyResults.Add(errorMessage);
			result.IsHealthy = false;

			return result;
		}

		/// <summary>
		///		Добавляет коллекцию сообщений об ошибках в результат проверки и помечает результат как неуспешный.
		/// </summary>
		/// <param name="result">Результат проверки работоспособности.</param>
		/// <param name="errors">Коллекция сообщений об ошибках для добавления.</param>
		/// <returns>Тот же экземпляр <see cref="VodovozHealthResultDto"/> с добавленными сообщениями и <see cref="VodovozHealthResultDto.IsHealthy"/> = false.</returns>
		public static VodovozHealthResultDto WithErrors(this VodovozHealthResultDto result, IEnumerable<string> errors)
		{
			foreach(var error in errors)
			{
				result.AdditionalUnhealthyResults.Add(error);
			}
			result.IsHealthy = false;

			return result;
		}
	}
}
