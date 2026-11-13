using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxBalanceService : IMindboxBalanceService
	{
		private const string SuccessStatus = "Success";

		private readonly IMindboxClient _client;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxBalanceService> _logger;

		public MindboxBalanceService(
			IMindboxClient client,
			IMindboxSettings settings,
			ILogger<MindboxBalanceService> logger)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task<MindboxBalanceInfo> GetAsync(int erpCounterpartyId, CancellationToken cancellationToken)
		{
			var request = new MindboxGetBalanceHistoryRequest
			{
				Customer = new MindboxGetBalanceHistoryCustomer
				{
					Ids = new Dictionary<string, string>(StringComparer.Ordinal)
					{
						["externalid"] = erpCounterpartyId.ToString()
					}
				}
			};

			var result = await _client.GetBalanceHistoryAsync(request, cancellationToken).ConfigureAwait(false);

			if(result.Outcome == MindboxSendOutcome.ValidationError)
			{
				_logger.LogWarning(
					"Клиент {ErpCounterpartyId} не найден в Mindbox: httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}",
					erpCounterpartyId, result.HttpStatusCode, result.Status, result.ErrorId, result.DurationMs);

				return MindboxBalanceInfo.Zero();
			}

			if(!result.IsSuccess || !string.Equals(result.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase))
			{
				_logger.LogError(
					"Ошибка GetBalanceHistory Mindbox: erpCounterpartyId={ErpCounterpartyId}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}",
					erpCounterpartyId, result.HttpStatusCode, result.Status, result.ErrorId, result.DurationMs);

				return MindboxBalanceInfo.Unavailable();
			}

			MindboxGetBalanceHistoryResponse response;

			try
			{
				response = JsonSerializer.Deserialize<MindboxGetBalanceHistoryResponse>(result.RawResponse);
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Не удалось разобрать ответ GetBalanceHistory Mindbox для клиента {ErpCounterpartyId}", erpCounterpartyId);
				return MindboxBalanceInfo.Unavailable();
			}

			var balance = GetBalance(response?.Balances);

			if(balance is null)
			{
				_logger.LogInformation(
					"Бонусный счёт {SystemName} не найден в Mindbox для клиента {ErpCounterpartyId}",
					_settings.BonusBalanceSystemName, erpCounterpartyId);

				return MindboxBalanceInfo.Zero();
			}

			var expiringAmount = balance.NearestExpiration is null || balance.NearestExpiration.Total <= 0
				? null
				: (int?)Math.Round(balance.NearestExpiration.Total, MidpointRounding.AwayFromZero);

			var info = new MindboxBalanceInfo
			{
				IsAvailable = true,
				Balance = (int)Math.Round(balance.Available, MidpointRounding.AwayFromZero),
				Pending = (int)Math.Round(balance.Blocked, MidpointRounding.AwayFromZero),
				ExpiringAmount = expiringAmount,
				ExpiringDate = expiringAmount.HasValue ? balance.NearestExpiration.Date : null
			};

			_logger.LogInformation(
				"Баланс клиента получен из Mindbox: erpCounterpartyId={ErpCounterpartyId}, httpCode={HttpCode}, status={Status}, balance={Balance}, pending={Pending}, expiring={Expiring}, errorId={ErrorId}, durationMs={DurationMs}",
				erpCounterpartyId, result.HttpStatusCode, result.Status, info.Balance, info.Pending,
				info.ExpiringAmount, result.ErrorId, result.DurationMs);

			return info;
		}

		private MindboxBalanceHistoryItem GetBalance(IEnumerable<MindboxBalanceHistoryItem> balances)
		{
			var list = (balances ?? Enumerable.Empty<MindboxBalanceHistoryItem>()).ToList();

			if(list.Count == 0)
			{
				return null;
			}

			if(!string.IsNullOrWhiteSpace(_settings.BonusBalanceSystemName))
			{
				var matched = list.FirstOrDefault(balance =>
					string.Equals(balance.SystemName, _settings.BonusBalanceSystemName, StringComparison.Ordinal)
					|| (balance.BalanceType?.Ids != null
						&& balance.BalanceType.Ids.TryGetValue("systemName", out var systemName)
						&& string.Equals(systemName, _settings.BonusBalanceSystemName, StringComparison.Ordinal)));

				if(matched != null)
				{
					return matched;
				}
			}

			return list.Count == 1 ? list[0] : null;
		}
	}
}
