using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Mindbox;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxOrderBonusesService : IMindboxOrderBonusesService
	{
		private const string SuccessStatus = "Success";
		private const string FoundStatus = "Found";
		private const string SpentBonusPointsType = "spentBonusPoints";
		private const string EarnedBonusPointsType = "earnedBonusPoints";
		private const string CanceledLineStatus = "Canceled";
		private const string ReturnedLineStatus = "Returned";
		private const string CompletedLineStatus = "OrderCompleted";

		private readonly IMindboxClient _client;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxOrderBonusesService> _logger;

		public MindboxOrderBonusesService(
			IMindboxClient client,
			IMindboxSettings settings,
			ILogger<MindboxOrderBonusesService> logger)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task<MindboxOrderBonuses> GetAsync(
			OnlineOrder onlineOrder,
			IReadOnlyCollection<string> lineIds,
			CancellationToken cancellationToken)
		{
			if(onlineOrder is null
				|| onlineOrder.Source != Source.MobileApp
				|| !_settings.IsEnabled
				|| onlineOrder.MindboxStatus != MindboxCounterpartyOutboxStatus.Sent)
			{
				return null;
			}

			var request = new MindboxGetOrderInfoRequest
			{
				Order = new MindboxGetOrderInfoRequestOrder
				{
					Ids = new Dictionary<string, string>(StringComparer.Ordinal)
					{
						[_settings.MindboxOrderIdName] = onlineOrder.Id.ToString()
					}
				}
			};

			var result = await _client.GetOrderInfoAsync(request, cancellationToken).ConfigureAwait(false);
			var now = DateTime.UtcNow;

			if(!IsSuccess(result))
			{
				if(result.HttpStatusCode == 400 || result.Outcome == MindboxSendOutcome.NotFound)
				{
					_logger.LogWarning(
						"Заказ {OnlineOrderId} не найден в Mindbox: httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}",
						onlineOrder.Id, result.HttpStatusCode, result.Status, result.ErrorId, result.DurationMs);
					return null;
				}

				_logger.LogError(
					"Ошибка GetOrderInfo Mindbox: onlineOrderId={OnlineOrderId}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}",
					onlineOrder.Id, result.HttpStatusCode, result.Status, result.ErrorId, result.DurationMs);

				return MindboxOrderBonuses.Unavailable();
			}

			MindboxGetOrderInfoResponse response;

			try
			{
				response = JsonSerializer.Deserialize<MindboxGetOrderInfoResponse>(result.RawResponse);
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Не удалось разобрать ответ GetOrderInfo Mindbox для заказа {OnlineOrderId}", onlineOrder.Id);
				return MindboxOrderBonuses.Unavailable();
			}

			if(response?.Order is null
				|| !string.Equals(response.Order.ProcessingStatus, FoundStatus, StringComparison.OrdinalIgnoreCase))
			{
				_logger.LogWarning(
					"Заказ {OnlineOrderId} не найден в ответе Mindbox: processingStatus={ProcessingStatus}",
					onlineOrder.Id, response?.Order?.ProcessingStatus);
				return null;
			}

			var lines = FilterLines(response.Order.Lines, lineIds);

			if(lines.Count == 0 || IsCanceledOrReturned(lines))
			{
				return null;
			}

			var bonusInfo = GetBonusInfo(response.Order.BonusPointsInfoPerBalanceType);

			if(bonusInfo is null && lineIds is null)
			{
				return null;
			}

			var writtenOff = lineIds is null
				? ToInt(bonusInfo?.SpentAmount ?? 0)
				: ToInt(lines.Sum(line => GetPromotionAmount(line, SpentBonusPointsType)));

			var earned = lineIds is null
				? ToInt(bonusInfo?.EarnedAmount ?? 0)
				: ToInt(lines.Sum(line => GetPromotionAmount(line, EarnedBonusPointsType)));

			if(writtenOff == 0 && earned == 0)
			{
				return null;
			}

			var earnStatus = IsCompleted(lines)
				? MindboxOrderBonusesEarnStatus.Earned
				: MindboxOrderBonusesEarnStatus.Pending;

			_logger.LogInformation(
				"Бонусы заказа получены из Mindbox: onlineOrderId={OnlineOrderId}, httpCode={HttpCode}, status={Status}, processingStatus={ProcessingStatus}, writtenOff={WrittenOff}, earned={Earned}, errorId={ErrorId}, durationMs={DurationMs}",
				onlineOrder.Id, result.HttpStatusCode, response.Status, response.Order.ProcessingStatus,
				writtenOff, earned, result.ErrorId, result.DurationMs);

			return new MindboxOrderBonuses
			{
				IsAvailable = true,
				WrittenOff = writtenOff,
				Earned = earned,
				EarnStatus = earnStatus
			};
		}

		private static bool IsSuccess(MindboxOperationResult result)
		{
			return result.IsSuccess
				&& string.Equals(result.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase);
		}

		private static List<MindboxGetOrderInfoLine> FilterLines(
			IEnumerable<MindboxGetOrderInfoLine> lines,
			IReadOnlyCollection<string> lineIds)
		{
			var result = (lines ?? Enumerable.Empty<MindboxGetOrderInfoLine>()).ToList();

			if(lineIds is null)
			{
				return result;
			}

			var ids = new HashSet<string>(lineIds, StringComparer.Ordinal);
			return result.Where(line => ids.Contains(line.LineId)).ToList();
		}

		private MindboxGetOrderInfoBonusInfo GetBonusInfo(IEnumerable<MindboxGetOrderInfoBonusInfo> infos)
		{
			var list = (infos ?? Enumerable.Empty<MindboxGetOrderInfoBonusInfo>()).ToList();

			if(list.Count == 0)
			{
				return null;
			}

			if(!string.IsNullOrWhiteSpace(_settings.BonusBalanceSystemName))
			{
				var matched = list.FirstOrDefault(info =>
					info.BalanceType?.Ids != null
					&& info.BalanceType.Ids.TryGetValue("systemName", out var systemName)
					&& string.Equals(systemName, _settings.BonusBalanceSystemName, StringComparison.Ordinal));

				if(matched != null)
				{
					return matched;
				}
			}

			return list.Count == 1 ? list[0] : null;
		}

		private static bool IsCanceledOrReturned(IEnumerable<MindboxGetOrderInfoLine> lines)
		{
			return lines.All(line =>
			{
				var status = GetLineStatus(line);
				return string.Equals(status, CanceledLineStatus, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(status, ReturnedLineStatus, StringComparison.OrdinalIgnoreCase);
			});
		}

		private static bool IsCompleted(IEnumerable<MindboxGetOrderInfoLine> lines)
		{
			var active = lines.Where(line =>
			{
				var status = GetLineStatus(line);
				return !string.Equals(status, CanceledLineStatus, StringComparison.OrdinalIgnoreCase)
					&& !string.Equals(status, ReturnedLineStatus, StringComparison.OrdinalIgnoreCase);
			}).ToList();

			return active.Count > 0 && active.All(line =>
				string.Equals(GetLineStatus(line), CompletedLineStatus, StringComparison.OrdinalIgnoreCase));
		}

		private static string GetLineStatus(MindboxGetOrderInfoLine line)
		{
			return line.Status?.Ids != null
				&& line.Status.Ids.TryGetValue("externalId", out var status)
					? status
					: null;
		}

		private static decimal GetPromotionAmount(MindboxGetOrderInfoLine line, string type)
		{
			return (line.AppliedPromotions ?? Enumerable.Empty<MindboxAppliedPromotion>())
				.Where(promotion => string.Equals(promotion.Type, type, StringComparison.OrdinalIgnoreCase))
				.Sum(promotion => promotion.Amount);
		}

		private static int ToInt(decimal value)
		{
			return (int)Math.Round(value, MidpointRounding.AwayFromZero);
		}
	}
}
