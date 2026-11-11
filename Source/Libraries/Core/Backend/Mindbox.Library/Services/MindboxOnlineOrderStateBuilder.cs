using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Mindbox.Library.Contracts;
using Mindbox.Library.Mapping;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Database.Mindbox;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Extensions;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxOnlineOrderStateBuilder : IMindboxOnlineOrderStateBuilder
	{
		private const string DefaultLineStatus = "Created";

		private readonly IMindboxSettings _settings;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public MindboxOnlineOrderStateBuilder(
			IMindboxSettings settings,
			INomenclatureSettings nomenclatureSettings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}

		/// <summary>
		/// Собирает текущее состояние заказа
		/// </summary>
		/// <param name="onlineOrder">Онлайн-заказ ERP</param>
		/// <returns>Текущее состояние заказа</returns>
		public MindboxOnlineOrderState BuildState(OnlineOrder onlineOrder)
		{
			if(onlineOrder is null)
			{
				throw new ArgumentNullException(nameof(onlineOrder));
			}

			var status = ResolveStatus(onlineOrder);
			var lines = new List<MindboxOnlineOrderStateLine>();
			var lineNumber = 0;
			var bonusesWrittenOff = 0;

			foreach(var item in onlineOrder.OnlineOrderItems.OrderBy(item => item.Id))
			{
				if(IsDeliveryItem(item.NomenclatureId))
				{
					continue;
				}

				var product = BuildProductId(item);

				if(product is null)
				{
					continue;
				}

				lineNumber++;

				lines.Add(new MindboxOnlineOrderStateLine
				{
					LineId = item.Id.ToString(),
					LineNumber = lineNumber,
					ProductId = product,
					Quantity = item.Count,
					BasePricePerItem = item.Count == 0
						? 0
						: Math.Round((item.CurrentSum + item.BonusesWrittenOff) / item.Count, 2),
					BonusesWrittenOff = item.BonusesWrittenOff,
					Status = status
				});

				bonusesWrittenOff += item.BonusesWrittenOff;
			}

			return new MindboxOnlineOrderState
			{
				OnlineOrderId = onlineOrder.Id,
				Status = status,
				DeliveryCost = onlineOrder.OnlineOrderItems
					.Where(item => IsDeliveryItem(item.NomenclatureId))
					.Sum(item => item.CurrentSum),
				TotalPrice = onlineOrder.OnlineOrderSum,
				BonusesWrittenOff = bonusesWrittenOff,
				BonusesToEarn = onlineOrder.BonusesToEarn,
				Lines = lines
			};
		}

		/// <summary>
		/// Возвращает статус позиций заказа по настройкам маппинга
		/// </summary>
		/// <param name="onlineOrder">Онлайн-заказ ERP</param>
		/// <returns>Статус позиций в терминах Mindbox</returns>
		public string ResolveStatus(OnlineOrder onlineOrder)
		{
			if(onlineOrder is null)
			{
				throw new ArgumentNullException(nameof(onlineOrder));
			}

			var mapping = GetMapping();

			if(onlineOrder.Orders != null && onlineOrder.Orders.Count > 0)
			{
				var orderStatusName = onlineOrder.Orders
					.OrderByDescending(order => order.Id)
					.First()
					.OrderStatus
					.ToString();

				if(mapping.TryGetValue(orderStatusName, out var orderStatusValue))
				{
					return orderStatusValue;
				}
			}
			else
			{
				var onlineOrderStatusName = onlineOrder.OnlineOrderStatus.ToString();

				if(mapping.TryGetValue(onlineOrderStatusName, out var onlineOrderStatusValue))
				{
					return onlineOrderStatusValue;
				}
			}

			var externalStatusName = onlineOrder.GetExternalOrderStatus().ToString();

			if(mapping.TryGetValue(externalStatusName, out var externalStatusValue))
			{
				return externalStatusValue;
			}

			return DefaultLineStatus;
		}

		/// <summary>
		/// Собирает запрос создания/обновления состава
		/// </summary>
		/// <param name="onlineOrder">Онлайн-заказ ERP</param>
		/// <param name="state">Состояние заказа для отправки</param>
		/// <returns>Запрос операции CreateAuthorizedLoyaltyOrder</returns>
		public MindboxLoyaltyOrderRequest BuildCreateRequest(OnlineOrder onlineOrder, MindboxOnlineOrderState state)
		{
			if(onlineOrder is null)
			{
				throw new ArgumentNullException(nameof(onlineOrder));
			}

			if(state is null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			var balanceTypeIds = new Dictionary<string, string>(StringComparer.Ordinal);

			if(!string.IsNullOrWhiteSpace(_settings.BonusBalanceSystemName))
			{
				balanceTypeIds["systemName"] = _settings.BonusBalanceSystemName;
			}

			return new MindboxLoyaltyOrderRequest
			{
				Customer = new MindboxLoyaltyOrderCustomer
				{
					Ids = new Dictionary<string, string>(StringComparer.Ordinal)
					{
						["externalid"] = (onlineOrder.CounterpartyId ?? 0).ToString()
					}
				},
				Order = new MindboxLoyaltyOrder
				{
					Ids = new Dictionary<string, string>(StringComparer.Ordinal)
					{
						[_settings.MindboxOrderIdName] = onlineOrder.Id.ToString()
					},
					DeliveryCost = state.DeliveryCost,
					TotalPrice = state.TotalPrice,
					BonusPoints = new List<MindboxAuthorizedCartBonusPoint>
					{
						new MindboxAuthorizedCartBonusPoint
						{
							Amount = state.BonusesWrittenOff,
							BalanceType = new MindboxAuthorizedCartBalanceType
							{
								Ids = balanceTypeIds
							}
						}
					},
					Lines = state.Lines
						.Select(line => new MindboxLoyaltyOrderLine
						{
							LineId = line.LineId,
							LineNumber = line.LineNumber,
							Product = new MindboxProduct
							{
								Ids = new Dictionary<string, string>(StringComparer.Ordinal)
								{
									[_settings.BonusItemIdName] = line.ProductId
								}
							},
							Quantity = line.Quantity,
							QuantityType = "int",
							BasePricePerItem = line.BasePricePerItem,
							Status = new MindboxLoyaltyOrderLineStatus
							{
								Ids = new MindboxLoyaltyOrderLineStatusIds
								{
									ExternalId = line.Status
								}
							}
						})
						.ToList()
				}
			};
		}

		/// <summary>
		/// Собирает запрос смены статуса позиций
		/// </summary>
		/// <param name="onlineOrder">Онлайн-заказ ERP</param>
		/// <param name="state">Текущее состояние заказа</param>
		/// <param name="changedLines">Позиции, у которых изменился статус</param>
		/// <param name="newStatus">Новый статус позиций</param>
		/// <param name="updateAllLines">Признак смены статуса всех позиций</param>
		/// <returns>Запрос операции изменения статуса позиций</returns>
		public MindboxOrderLinesStatusRequest BuildStatusRequest(
			OnlineOrder onlineOrder,
			MindboxOnlineOrderState state,
			IReadOnlyCollection<MindboxOnlineOrderStateLine> changedLines,
			string newStatus,
			bool updateAllLines)
		{
			if(onlineOrder is null)
			{
				throw new ArgumentNullException(nameof(onlineOrder));
			}

			if(state is null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			var order = new MindboxOrderLinesStatusOrder
			{
				Ids = new Dictionary<string, string>(StringComparer.Ordinal)
				{
					[_settings.MindboxOrderIdName] = onlineOrder.Id.ToString()
				}
			};

			if(updateAllLines)
			{
				return new MindboxOrderLinesStatusRequest
				{
					ExecutionDateTimeUtc = DateTime.UtcNow,
					OrderLinesStatus = newStatus,
					Order = order
				};
			}

			order.Lines = (changedLines ?? Array.Empty<MindboxOnlineOrderStateLine>())
				.Select(line => new MindboxOrderLinesStatusLine
				{
					LineId = line.LineId,
					Quantity = line.Quantity,
					Status = newStatus
				})
				.ToList();

			return new MindboxOrderLinesStatusRequest
			{
				ExecutionDateTimeUtc = DateTime.UtcNow,
				Order = order
			};
		}

		private string BuildProductId(OnlineOrderItem item)
		{
			if(item.PromoSet != null && item.PromoSet.Id > 0)
			{
				return MindboxItemIdBuilder.Build(item.PromoSet);
			}

			if(item.Nomenclature != null && item.Nomenclature.Id > 0)
			{
				return MindboxItemIdBuilder.Build(item.Nomenclature);
			}

			return null;
		}

		private bool IsDeliveryItem(int? nomenclatureId)
		{
			if(nomenclatureId is null)
			{
				return false;
			}

			return nomenclatureId == _nomenclatureSettings.PaidDeliveryNomenclatureId
				|| nomenclatureId == _nomenclatureSettings.FastDeliveryNomenclatureId
				|| _nomenclatureSettings.PaidDeliveriesNomenclaturesIds?.Contains(nomenclatureId.Value) == true;
		}

		private IDictionary<string, string> GetMapping()
		{
			var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if(string.IsNullOrWhiteSpace(_settings.OrderLineStatusMapping))
			{
				return mapping;
			}

			try
			{
				var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(_settings.OrderLineStatusMapping);

				if(parsed is null)
				{
					return mapping;
				}

				foreach(var pair in parsed)
				{
					if(!string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
					{
						mapping[pair.Key] = pair.Value;
					}
				}
			}
			catch(JsonException)
			{
				return mapping;
			}

			return mapping;
		}
	}
}
