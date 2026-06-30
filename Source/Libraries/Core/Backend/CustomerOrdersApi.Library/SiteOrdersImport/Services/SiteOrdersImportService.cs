using CustomerOrdersApi.Library.SiteOrdersImport.Dto;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders.SiteOrdersImport;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Services
{
	/// <summary>
	/// Приём пакета выгрузки с сайта (I-5840): каждая запись идемпотентно сохраняется в БД.
	/// Полезная нагрузка хранится сырым JSON по контракту v1; идемпотентность — по паре
	/// «идентификатор записи на сайте + тип сущности», повтор пакета/записи обновляет существующую строку.
	/// </summary>
	public class SiteOrdersImportService : ISiteOrdersImportService
	{
		private static readonly HashSet<string> _availableEntityTypes = new(StringComparer.Ordinal)
		{
			"order",
			"abandoned_cart"
		};

		private readonly ILogger<SiteOrdersImportService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public SiteOrdersImportService(
			ILogger<SiteOrdersImportService> logger,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public Task<OrdersImportResponse> ImportAsync(OrdersImportRequest request, CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var importedOrderIds = new List<long>();
			var errorOrderIds = new List<long>();

			var items = request.Items ?? Array.Empty<OrderImportItem>();

			_logger.LogInformation(
				"Принят пакет выгрузки с сайта: batch_id={BatchId}, contract_version={ContractVersion}, " +
				"sent_at={SentAt}, записей={ItemsCount}, total_count={TotalCount}",
				request.BatchId,
				request.ContractVersion,
				request.SentAt,
				items.Count,
				request.TotalCount);

			foreach(var item in items)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var orderId = item?.OrderId ?? 0;
				var entityType = item?.EntityType;

				try
				{
					ValidateItem(item);
					Save(request, item);
					importedOrderIds.Add(item.OrderId);
				}
				catch(Exception e)
				{
					_logger.LogError(
						e,
						"Ошибка сохранения записи пакета {BatchId}: order_id={OrderId}, entity_type={EntityType}",
						request.BatchId,
						orderId,
						entityType);

					errorOrderIds.Add(orderId);
				}
			}

			var response = new OrdersImportResponse
			{
				Success = errorOrderIds.Count == 0,
				BatchId = request.BatchId,
				ImportedOrderIds = importedOrderIds,
				ErrorOrderIds = errorOrderIds
			};

			return Task.FromResult(response);
		}

		private static void ValidateItem(OrderImportItem item)
		{
			if(item is null)
			{
				throw new InvalidOperationException("Запись пакета выгрузки не заполнена.");
			}

			if(item.OrderId <= 0)
			{
				throw new InvalidOperationException("В записи пакета выгрузки не заполнен order_id.");
			}

			if(string.IsNullOrWhiteSpace(item.EntityType) || !_availableEntityTypes.Contains(item.EntityType))
			{
				throw new InvalidOperationException("В записи пакета выгрузки entity_type должен быть order или abandoned_cart.");
			}

			if(item.Payload.ValueKind == JsonValueKind.Undefined)
			{
				throw new InvalidOperationException("В записи пакета выгрузки не заполнен payload.");
			}
		}

		private void Save(OrdersImportRequest request, OrderImportItem item)
		{
			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Приём выгрузки заказов с сайта");
			var entity = unitOfWork
				             .GetAll<SiteOrderImportItem>()
				             .FirstOrDefault(x => x.SiteOrderId == item.OrderId && x.EntityType == item.EntityType)
			             ?? new SiteOrderImportItem
			             {
				             SiteOrderId = item.OrderId,
				             EntityType = item.EntityType
			             };

			entity.SiteStatus = item.Status;
			entity.SiteUpdatedAt = item.UpdatedAt;
			entity.BatchId = request.BatchId;
			entity.ContractVersion = request.ContractVersion;
			entity.SentAt = request.SentAt;
			entity.Payload = SerializePayload(item.Payload);
			entity.ReceivedAt = DateTime.Now;

			unitOfWork.Save(entity);
			unitOfWork.Commit();
		}

		private static string SerializePayload(JsonElement payload)
		{
			return payload.ValueKind == JsonValueKind.Undefined
				? null
				: payload.GetRawText();
		}
	}
}
