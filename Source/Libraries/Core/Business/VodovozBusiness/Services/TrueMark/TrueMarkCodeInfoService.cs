using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;
using TrueMarkApi.Client;

namespace VodovozBusiness.Services.TrueMark
{
	/// <summary>
	/// Сервис получения текущей информации о коде маркировки из Честного знака.
	/// </summary>
	public interface ITrueMarkCodeInfoService
	{
		/// <summary>
		/// Запрашивает информацию об экземпляре товара в Честном знаке.
		/// </summary>
		/// <param name="identificationCodes">Коды идентификации экземпляров.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Информация о статусах экземпляров товара.</returns>
		Task<ProductInstancesInfoResponse> GetProductInstancesInfoAsync(
			IEnumerable<string> identificationCodes,
			CancellationToken cancellationToken);

		/// <summary>
		/// Запрашивает информацию о коде и рекурсивно обо всех его дочерних кодах.
		/// </summary>
		/// <param name="identificationCodes">Корневые коды идентификации.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Информация о статусах корневого кода и всех дочерних кодов.</returns>
		Task<ProductInstancesInfoResponse> GetProductInstancesInfoWithChildrenAsync(
			IEnumerable<string> identificationCodes,
			CancellationToken cancellationToken);
	}

	/// <inheritdoc cref="ITrueMarkCodeInfoService"/>
	public class TrueMarkCodeInfoService : ITrueMarkCodeInfoService
	{
		private const int BatchSize = 100;
		private const int MaxCodesCount = 1000;

		private readonly ITrueMarkApiClient _trueMarkApiClient;

		public TrueMarkCodeInfoService(ITrueMarkApiClient trueMarkApiClient)
		{
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
		}

		/// <inheritdoc/>
		public Task<ProductInstancesInfoResponse> GetProductInstancesInfoAsync(
			IEnumerable<string> identificationCodes,
			CancellationToken cancellationToken)
		{
			return _trueMarkApiClient.GetProductInstanceInfoAsync(identificationCodes, cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<ProductInstancesInfoResponse> GetProductInstancesInfoWithChildrenAsync(
			IEnumerable<string> identificationCodes,
			CancellationToken cancellationToken)
		{
			var requestedCodes = new HashSet<string>(StringComparer.Ordinal);
			var enqueuedCodes = new HashSet<string>(StringComparer.Ordinal);
			var statuses = new Dictionary<string, ProductInstanceStatus>(StringComparer.Ordinal);
			var pendingCodes = new Queue<string>();
			var errors = new List<string>();
			var noCodesFound = false;

			foreach(var identificationCode in identificationCodes ?? Enumerable.Empty<string>())
			{
				if(!string.IsNullOrWhiteSpace(identificationCode) && enqueuedCodes.Add(identificationCode))
				{
					pendingCodes.Enqueue(identificationCode);
				}
			}

			while(pendingCodes.Any())
			{
				var batch = new List<string>();

				while(pendingCodes.Any() && batch.Count < BatchSize)
				{
					var code = pendingCodes.Dequeue();

					if(requestedCodes.Add(code))
					{
						batch.Add(code);
					}
				}

				if(!batch.Any())
				{
					continue;
				}

				if(statuses.Count + batch.Count > MaxCodesCount)
				{
					errors.Add($"Превышено максимальное количество кодов для отображения ({MaxCodesCount})");
					break;
				}

				try
				{
					var response = await _trueMarkApiClient.GetProductInstanceInfoAsync(batch, cancellationToken);

					if(response is null)
					{
						errors.Add("ЧЗ вернул пустой ответ");
						continue;
					}

					if(response.NoCodesFound)
					{
						noCodesFound = true;
					}

					if(!string.IsNullOrWhiteSpace(response.ErrorMessage))
					{
						errors.Add(response.ErrorMessage);
					}

					foreach(var status in response.InstanceStatuses ?? Enumerable.Empty<ProductInstanceStatus>())
					{
						if(string.IsNullOrWhiteSpace(status.IdentificationCode))
						{
							continue;
						}

						if(!statuses.ContainsKey(status.IdentificationCode))
						{
							statuses.Add(status.IdentificationCode, status);
						}

						foreach(var childCode in status.Childs ?? Enumerable.Empty<string>())
						{
							if(!string.IsNullOrWhiteSpace(childCode) && enqueuedCodes.Add(childCode))
							{
								pendingCodes.Enqueue(childCode);
							}
						}
					}
				}
				catch(Exception ex)
				{
					errors.Add(ex.Message);
				}
			}

			return new ProductInstancesInfoResponse
			{
				InstanceStatuses = statuses.Values.ToList(),
				NoCodesFound = noCodesFound && !statuses.Any(),
				ErrorMessage = errors.Any()
					? string.Join("; ", errors.Distinct())
					: null
			};
		}
	}
}
