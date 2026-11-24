using NHibernate.Criterion;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Data.NHibernate.Repositories
{
	/// <summary>
	/// Репозиторий истории кода маркировки в ДВ.
	/// </summary>
	internal sealed class TrueMarkCodeHistoryRepository : ITrueMarkCodeHistoryRepository
	{
		private const string TransportCodeType = "Транспортный код";
		private const string GroupCodeType = "Групповой код";

		public TrueMarkCodeHistoryData GetCodeHistory(
			IUnitOfWork uow,
			string gtin,
			string serialNumber,
			string rawCode = null)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			var hasWaterCodeData = !string.IsNullOrWhiteSpace(gtin) || !string.IsNullOrWhiteSpace(serialNumber);

			if(!hasWaterCodeData && string.IsNullOrWhiteSpace(rawCode))
			{
				return new TrueMarkCodeHistoryData();
			}

			IList<TrueMarkWaterIdentificationCode> codes;
			var foundAggregations = new List<TrueMarkCodeHistoryAggregation>();

			if(hasWaterCodeData)
			{
				codes = GetIdentificationCodes(uow, gtin, serialNumber);

				if(!codes.Any())
				{
					var groupCode = GetGroupCodeByGtinAndSerialNumber(uow, gtin, serialNumber);

					if(groupCode != null)
					{
						AddFoundAggregation(foundAggregations, CreateGroupAggregation(groupCode));
						CollectIdentificationCodesFromGroup(
							uow,
							groupCode.Id,
							codes,
							foundAggregations,
							new HashSet<int>(),
							new HashSet<int>());
					}
				}
			}
			else
			{
				codes = GetIdentificationCodesByRawCode(uow, rawCode, foundAggregations);
			}

			if(!codes.Any() && !foundAggregations.Any())
			{
				return new TrueMarkCodeHistoryData();
			}

			codes = codes
				.GroupBy(x => x.Id)
				.Select(x => x.First())
				.ToList();

			var data = new TrueMarkCodeHistoryData
			{
				Codes = codes.Select(ToHistoryCode).ToList()
			};

			FillAggregations(uow, codes, data);
			FillFoundAggregations(data, foundAggregations);
			FillProductCodes(uow, codes, data);
			FillTasks(uow, data);
			FillOrders(uow, data);
			FillDocumentFlows(uow, data);

			return data;
		}

		private static IList<TrueMarkWaterIdentificationCode> GetIdentificationCodesByRawCode(
			IUnitOfWork uow,
			string rawCode,
			IList<TrueMarkCodeHistoryAggregation> foundAggregations)
		{
			var codes = new List<TrueMarkWaterIdentificationCode>();
			var visitedTransports = new HashSet<int>();
			var visitedGroups = new HashSet<int>();
			var visitedWaterCodes = new HashSet<int>();

			var rawCodeVariants = GetRawCodeVariants(rawCode);

			var transportCodes = uow.Session.Query<TrueMarkTransportCode>()
				.Where(x => rawCodeVariants.Contains(x.RawCode))
				.ToList();

			var groupCodes = uow.Session.Query<TrueMarkWaterGroupCode>()
				.Where(x => rawCodeVariants.Contains(x.RawCode))
				.ToList();

			foreach(var transportCode in transportCodes)
			{
				AddFoundAggregation(
					foundAggregations,
					CreateTransportAggregation(transportCode));

				CollectIdentificationCodesFromTransport(
					uow,
					transportCode.Id,
					codes,
					foundAggregations,
					visitedTransports,
					visitedGroups,
					visitedWaterCodes);
			}

			foreach(var groupCode in groupCodes)
			{
				AddFoundAggregation(
					foundAggregations,
					CreateGroupAggregation(groupCode));

				CollectIdentificationCodesFromGroup(
					uow,
					groupCode.Id,
					codes,
					foundAggregations,
					visitedGroups,
					visitedWaterCodes);
			}

			return codes;
		}

		private static void CollectIdentificationCodesFromTransport(
			IUnitOfWork uow,
			int transportCodeId,
			IList<TrueMarkWaterIdentificationCode> codes,
			IList<TrueMarkCodeHistoryAggregation> foundAggregations,
			ISet<int> visitedTransports,
			ISet<int> visitedGroups,
			ISet<int> visitedWaterCodes)
		{
			if(!visitedTransports.Add(transportCodeId))
			{
				return;
			}

			AddWaterCodes(
				codes,
				visitedWaterCodes,
				uow.Session.Query<TrueMarkWaterIdentificationCode>()
					.Where(x => x.ParentTransportCodeId == transportCodeId)
					.ToList());

			var childTransportCodes = uow.Session.Query<TrueMarkTransportCode>()
				.Where(x => x.ParentTransportCodeId == transportCodeId)
				.ToList();

			foreach(var childTransportCode in childTransportCodes)
			{
				AddFoundAggregation(foundAggregations, CreateTransportAggregation(childTransportCode));

				CollectIdentificationCodesFromTransport(
					uow,
					childTransportCode.Id,
					codes,
					foundAggregations,
					visitedTransports,
					visitedGroups,
					visitedWaterCodes);
			}

			var childGroupCodes = uow.Session.Query<TrueMarkWaterGroupCode>()
				.Where(x => x.ParentTransportCodeId == transportCodeId)
				.ToList();

			foreach(var childGroupCode in childGroupCodes)
			{
				AddFoundAggregation(foundAggregations, CreateGroupAggregation(childGroupCode));

				CollectIdentificationCodesFromGroup(
					uow,
					childGroupCode.Id,
					codes,
					foundAggregations,
					visitedGroups,
					visitedWaterCodes);
			}
		}

		private static void CollectIdentificationCodesFromGroup(
			IUnitOfWork uow,
			int groupCodeId,
			IList<TrueMarkWaterIdentificationCode> codes,
			IList<TrueMarkCodeHistoryAggregation> foundAggregations,
			ISet<int> visitedGroups,
			ISet<int> visitedWaterCodes)
		{
			if(!visitedGroups.Add(groupCodeId))
			{
				return;
			}

			AddWaterCodes(
				codes,
				visitedWaterCodes,
				uow.Session.Query<TrueMarkWaterIdentificationCode>()
					.Where(x => x.ParentWaterGroupCodeId == groupCodeId)
					.ToList());

			var childGroupCodes = uow.Session.Query<TrueMarkWaterGroupCode>()
				.Where(x => x.ParentWaterGroupCodeId == groupCodeId)
				.ToList();

			foreach(var childGroupCode in childGroupCodes)
			{
				AddFoundAggregation(foundAggregations, CreateGroupAggregation(childGroupCode));

				CollectIdentificationCodesFromGroup(
					uow,
					childGroupCode.Id,
					codes,
					foundAggregations,
					visitedGroups,
					visitedWaterCodes);
			}
		}

		private static void AddWaterCodes(
			IList<TrueMarkWaterIdentificationCode> codes,
			ISet<int> visitedWaterCodes,
			IEnumerable<TrueMarkWaterIdentificationCode> waterCodes)
		{
			foreach(var waterCode in waterCodes)
			{
				if(visitedWaterCodes.Add(waterCode.Id))
				{
					codes.Add(waterCode);
				}
			}
		}

		private static void AddFoundAggregation(
			IList<TrueMarkCodeHistoryAggregation> aggregations,
			TrueMarkCodeHistoryAggregation aggregation)
		{
			if(!aggregations.Any(x => x.Type == aggregation.Type && x.Id == aggregation.Id))
			{
				aggregations.Add(aggregation);
			}
		}

		private static void FillFoundAggregations(
			TrueMarkCodeHistoryData data,
			IEnumerable<TrueMarkCodeHistoryAggregation> foundAggregations)
		{
			foreach(var aggregation in foundAggregations)
			{
				if(!data.Aggregations.Any(x => x.Type == aggregation.Type && x.Id == aggregation.Id))
				{
					data.Aggregations.Add(aggregation);
				}
			}
		}

		private static IList<string> GetRawCodeVariants(string rawCode)
		{
			var variants = new List<string> { rawCode };

			if(rawCode.StartsWith("00") && rawCode.Length == 28)
			{
				variants.Add(rawCode.Substring(2));
			}
			else if(rawCode.Length == 26)
			{
				variants.Add("00" + rawCode);
			}

			return variants;
		}

		private static Dictionary<int, T> ToDictionarySafe<T>(
			IEnumerable<T> items,
			Func<T, int> keySelector)
		{
			var result = new Dictionary<int, T>();

			foreach(var item in items)
			{
				var key = keySelector(item);

				if(!result.ContainsKey(key))
				{
					result.Add(key, item);
				}
			}

			return result;
		}

		private static IList<TrueMarkWaterIdentificationCode> GetIdentificationCodes(
			IUnitOfWork uow,
			string gtin,
			string serialNumber)
		{
			if(string.IsNullOrWhiteSpace(serialNumber))
			{
				return new List<TrueMarkWaterIdentificationCode>();
			}

			var candidates = uow.Session.Query<TrueMarkWaterIdentificationCode>()
				.Where(x => x.SerialNumber == serialNumber)
				.ToList();

			var matchedCodes = FilterByGtin(candidates, gtin, x => x.Gtin);

			if(matchedCodes.Any())
			{
				return matchedCodes;
			}

			var rawCodeCandidates = uow.Session.Query<TrueMarkWaterIdentificationCode>()
				.Where(x => x.RawCode.Contains(serialNumber))
				.ToList();

			matchedCodes = FilterByGtin(rawCodeCandidates, gtin, x => x.Gtin);

			if(matchedCodes.Any())
			{
				return matchedCodes;
			}

			return candidates.Any()
				? candidates
				: rawCodeCandidates;
		}

		private static IList<T> FilterByGtin<T>(
			IEnumerable<T> codes,
			string gtin,
			Func<T, string> getGtin)
		{
			var result = codes.ToList();

			if(string.IsNullOrWhiteSpace(gtin))
			{
				return result;
			}

			var normalizedGtin = NormalizeGtin(gtin);

			return result
				.Where(x => string.IsNullOrWhiteSpace(getGtin(x))
					|| NormalizeGtin(getGtin(x)) == normalizedGtin)
				.ToList();
		}

		private static string NormalizeGtin(string gtin)
		{
			return string.IsNullOrWhiteSpace(gtin)
				? gtin
				: gtin.TrimStart('0');
		}

		private static TrueMarkWaterGroupCode GetGroupCodeByGtinAndSerialNumber(
			IUnitOfWork uow,
			string gtin,
			string serialNumber)
		{
			if(string.IsNullOrWhiteSpace(serialNumber))
			{
				return null;
			}

			var candidates = uow.Session.Query<TrueMarkWaterGroupCode>()
				.Where(x => x.SerialNumber == serialNumber)
				.ToList();

			var matchedGroups = FilterByGtin(candidates, gtin, x => x.GTIN);

			if(matchedGroups.Any())
			{
				return matchedGroups.First();
			}

			var rawCodeCandidates = uow.Session.Query<TrueMarkWaterGroupCode>()
				.Where(x => x.RawCode.Contains(serialNumber))
				.ToList();

			matchedGroups = FilterByGtin(rawCodeCandidates, gtin, x => x.GTIN);

			if(matchedGroups.Any())
			{
				return matchedGroups.First();
			}

			return candidates.FirstOrDefault() ?? rawCodeCandidates.FirstOrDefault();
		}

		private static TrueMarkCodeHistoryCode ToHistoryCode(TrueMarkWaterIdentificationCode code)
		{
			return new TrueMarkCodeHistoryCode
			{
				Id = code.Id,
				Gtin = code.Gtin,
				SerialNumber = code.SerialNumber,
				RawCode = code.RawCode,
				IsInvalid = code.IsInvalid,
				ParentWaterGroupCodeId = code.ParentWaterGroupCodeId,
				ParentTransportCodeId = code.ParentTransportCodeId
			};
		}

		private static void FillAggregations(
			IUnitOfWork uow,
			IEnumerable<TrueMarkWaterIdentificationCode> codes,
			TrueMarkCodeHistoryData data)
		{
			var addedAggregations = new HashSet<string>();

			foreach(var code in codes)
			{
				var groupId = code.ParentWaterGroupCodeId;
				var transportId = code.ParentTransportCodeId;
				var visitedGroups = new HashSet<int>();
				var visitedTransports = new HashSet<int>();

				while(groupId.HasValue && visitedGroups.Add(groupId.Value))
				{
					var groupCode = uow.Session.Get<TrueMarkWaterGroupCode>(groupId.Value);

					if(groupCode is null)
					{
						break;
					}

					AddAggregationIfNotExists(
						data,
						addedAggregations,
						$"group_{groupCode.Id}",
						CreateGroupAggregation(groupCode, code.Id));

					if(!transportId.HasValue && groupCode.ParentTransportCodeId.HasValue)
					{
						transportId = groupCode.ParentTransportCodeId;
					}

					groupId = groupCode.ParentWaterGroupCodeId;
				}

				while(transportId.HasValue && visitedTransports.Add(transportId.Value))
				{
					var transportCode = uow.Session.Get<TrueMarkTransportCode>(transportId.Value);

					if(transportCode is null)
					{
						break;
					}

					AddAggregationIfNotExists(
						data,
						addedAggregations,
						$"transport_{transportCode.Id}",
						CreateTransportAggregation(transportCode, code.Id));

					transportId = transportCode.ParentTransportCodeId;
				}
			}
		}

		private static void AddAggregationIfNotExists(
			TrueMarkCodeHistoryData data,
			ISet<string> addedAggregations,
			string key,
			TrueMarkCodeHistoryAggregation aggregation)
		{
			if(addedAggregations.Add(key))
			{
				data.Aggregations.Add(aggregation);
			}
		}

		private static TrueMarkCodeHistoryAggregation CreateGroupAggregation(
			TrueMarkWaterGroupCode groupCode,
			int codeId = 0)
		{
			return new TrueMarkCodeHistoryAggregation
			{
				Id = groupCode.Id,
				Code = groupCode.RawCode,
				Type = GroupCodeType,
				Gtin = groupCode.GTIN,
				SerialNumber = groupCode.SerialNumber,
				IsInvalid = groupCode.IsInvalid,
				ParentId = groupCode.ParentWaterGroupCodeId ?? groupCode.ParentTransportCodeId,
				ParentType = groupCode.ParentWaterGroupCodeId.HasValue
					? GroupCodeType
					: groupCode.ParentTransportCodeId.HasValue
						? TransportCodeType
						: null,
				CodeId = codeId
			};
		}

		private static TrueMarkCodeHistoryAggregation CreateTransportAggregation(
			TrueMarkTransportCode transportCode,
			int codeId = 0)
		{
			return new TrueMarkCodeHistoryAggregation
			{
				Id = transportCode.Id,
				Code = transportCode.RawCode,
				Type = TransportCodeType,
				IsInvalid = transportCode.IsInvalid,
				ParentId = transportCode.ParentTransportCodeId,
				ParentType = transportCode.ParentTransportCodeId.HasValue
					? TransportCodeType
					: null,
				CodeId = codeId
			};
		}

		private static void FillProductCodes(
			IUnitOfWork uow,
			IEnumerable<TrueMarkWaterIdentificationCode> codes,
			TrueMarkCodeHistoryData data)
		{
			var codeIds = codes.Select(x => x.Id).ToArray();

			if(!codeIds.Any())
			{
				return;
			}

			TrueMarkProductCode productCodeAlias = null;
			TrueMarkWaterIdentificationCode sourceCodeAlias = null;
			TrueMarkWaterIdentificationCode resultCodeAlias = null;

			var productCodes = uow.Session.QueryOver(() => productCodeAlias)
				.JoinAlias(() => productCodeAlias.SourceCode, () => sourceCodeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => productCodeAlias.ResultCode, () => resultCodeAlias, JoinType.LeftOuterJoin)
				.Where(Restrictions.Or(
					Restrictions.In(Projections.Property(() => sourceCodeAlias.Id), codeIds),
					Restrictions.In(Projections.Property(() => resultCodeAlias.Id), codeIds)))
				.List();

			if(!productCodes.Any())
			{
				return;
			}

			var routeListItemIds = productCodes
				.OfType<RouteListItemTrueMarkProductCode>()
				.Where(x => x.RouteListItemId.HasValue)
				.Select(x => x.RouteListItemId.Value)
				.Distinct()
				.ToArray();

			var carLoadDocumentItemIds = productCodes
				.OfType<CarLoadDocumentItemTrueMarkProductCode>()
				.Where(x => x.CarLoadDocumentItemId.HasValue)
				.Select(x => x.CarLoadDocumentItemId.Value)
				.Distinct()
				.ToArray();

			var selfDeliveryDocumentItemIds = productCodes
				.OfType<SelfDeliveryDocumentItemTrueMarkProductCode>()
				.Where(x => x.SelfDeliveryDocumentItemId.HasValue)
				.Select(x => x.SelfDeliveryDocumentItemId.Value)
				.Distinct()
				.ToArray();

			var routeListItems = routeListItemIds.Any()
				? ToDictionarySafe(
					uow.Session.Query<RouteListItemEntity>()
						.Where(x => routeListItemIds.Contains(x.Id)),
					x => x.Id)
				: new Dictionary<int, RouteListItemEntity>();

			var carLoadDocumentItems = carLoadDocumentItemIds.Any()
				? ToDictionarySafe(
					uow.Session.Query<CarLoadDocumentItemEntity>()
						.Where(x => carLoadDocumentItemIds.Contains(x.Id)),
					x => x.Id)
				: new Dictionary<int, CarLoadDocumentItemEntity>();

			var selfDeliveryDocumentItems = selfDeliveryDocumentItemIds.Any()
				? ToDictionarySafe(
					uow.Session.Query<SelfDeliveryDocumentItemEntity>()
						.Where(x => selfDeliveryDocumentItemIds.Contains(x.Id)),
					x => x.Id)
				: new Dictionary<int, SelfDeliveryDocumentItemEntity>();

			foreach(var productCode in productCodes
				.GroupBy(x => x.Id)
				.Select(x => x.First()))
			{
				var historyProductCode = new TrueMarkCodeHistoryProductCode
				{
					Id = productCode.Id,
					Type = GetProductCodeType(productCode),
					Status = productCode.SourceCodeStatus,
					Problem = productCode.Problem,
					SourceCode = productCode.SourceCode?.IdentificationCode,
					ResultCode = productCode.ResultCode?.IdentificationCode,
					LastModified = productCode.LastModified,
					CustomerEdoRequestId = productCode.CustomerEdoRequest?.Id,
					CustomerEdoRequestTime = productCode.CustomerEdoRequest?.Time,
					TaskId = productCode.CustomerEdoRequest?.Task?.Id
				};

				switch(productCode)
				{
					case RouteListItemTrueMarkProductCode routeListItemProductCode:
						historyProductCode.SourceDocumentType = TrueMarkCodeHistoryProductCodeDocumentType.RouteList;

						if(routeListItemProductCode.RouteListItemId.HasValue
							&& routeListItems.TryGetValue(routeListItemProductCode.RouteListItemId.Value, out var routeListItem))
						{
							historyProductCode.SourceDocumentId = routeListItem.RouteList?.Id;
							historyProductCode.OrderId = routeListItem.Order?.Id;
						}
						break;

					case CarLoadDocumentItemTrueMarkProductCode carLoadDocumentItemProductCode:
						historyProductCode.SourceDocumentType = TrueMarkCodeHistoryProductCodeDocumentType.CarLoadDocument;

						if(carLoadDocumentItemProductCode.CarLoadDocumentItemId.HasValue
							&& carLoadDocumentItems.TryGetValue(carLoadDocumentItemProductCode.CarLoadDocumentItemId.Value, out var carLoadDocumentItem))
						{
							historyProductCode.SourceDocumentId = carLoadDocumentItem.Document?.Id;
							historyProductCode.OrderId = carLoadDocumentItem.OrderId;
						}
						break;

					case SelfDeliveryDocumentItemTrueMarkProductCode selfDeliveryProductCode:
						historyProductCode.SourceDocumentType = TrueMarkCodeHistoryProductCodeDocumentType.SelfDeliveryDocument;

						if(selfDeliveryProductCode.SelfDeliveryDocumentItemId.HasValue
							&& selfDeliveryDocumentItems.TryGetValue(selfDeliveryProductCode.SelfDeliveryDocumentItemId.Value, out var selfDeliveryItem))
						{
							historyProductCode.SourceDocumentId = selfDeliveryItem.Document?.Id;
							historyProductCode.OrderId = selfDeliveryItem.Document?.Order?.Id;
						}
						break;

					default:
						historyProductCode.OrderId = productCode.CustomerEdoRequest?.Order?.Id;
						break;
				}

				data.ProductCodes.Add(historyProductCode);
			}
		}

		private static string GetProductCodeType(TrueMarkProductCode productCode)
		{
			switch(productCode)
			{
				case RouteListItemTrueMarkProductCode _:
					return "Водитель";
				case CarLoadDocumentItemTrueMarkProductCode _:
					return "Склад";
				case SelfDeliveryDocumentItemTrueMarkProductCode _:
					return "Самовывоз";
				case ResentTrueMarkProductCode _:
					return "Переотправлен";
				case ManualTrueMarkProductCode _:
					return "Добавлен вручную";
				case AutoTrueMarkProductCode _:
					return "Из пула";
				default:
					return "Код товара";
			}
		}

		private static void FillTasks(IUnitOfWork uow, TrueMarkCodeHistoryData data)
		{
			if(!data.ProductCodes.Any())
			{
				return;
			}

			var productCodeIds = data.ProductCodes.Select(x => x.Id).ToArray();
			EdoTaskItem edoTaskItemAlias = null;
			TrueMarkProductCode productCodeAlias = null;

			var taskItems = uow.Session.QueryOver(() => edoTaskItemAlias)
				.JoinAlias(() => edoTaskItemAlias.ProductCode, () => productCodeAlias)
				.Where(Restrictions.In(Projections.Property(() => productCodeAlias.Id), productCodeIds))
				.List();

			var taskIdsFromTaskItems = taskItems
				.Select(x => x.CustomerEdoTask.Id)
				.ToArray();

			var orderIdByProductCodeId = new Dictionary<int, int>();

			foreach(var productCode in data.ProductCodes.Where(x => x.OrderId.HasValue))
			{
				if(!orderIdByProductCodeId.ContainsKey(productCode.Id))
				{
					orderIdByProductCodeId.Add(productCode.Id, productCode.OrderId.Value);
				}
			}

			var orderIdByTaskId = new Dictionary<int, int>();

			foreach(var taskItem in taskItems)
			{
				if(orderIdByProductCodeId.TryGetValue(taskItem.ProductCode.Id, out var orderId))
				{
					orderIdByTaskId[taskItem.CustomerEdoTask.Id] = orderId;
				}
			}

			var taskIds = data.ProductCodes
				.Where(x => x.TaskId.HasValue)
				.Select(x => x.TaskId.Value)
				.Concat(taskIdsFromTaskItems)
				.Distinct()
				.ToArray();

			if(!taskIds.Any())
			{
				return;
			}

			var tasks = uow.Session.Query<EdoTask>()
				.Where(x => taskIds.Contains(x.Id))
				.ToList();

			foreach(var task in tasks
				.GroupBy(x => x.Id)
				.Select(x => x.First()))
			{
				var orderId = (task as OrderEdoTask)?.FormalEdoRequest?.Order?.Id;

				if(!orderId.HasValue)
				{
					orderId = data.ProductCodes
						.Where(x => x.TaskId == task.Id && x.OrderId.HasValue)
						.Select(x => x.OrderId)
						.FirstOrDefault();
				}

				if(!orderId.HasValue
					&& orderIdByTaskId.TryGetValue(task.Id, out var orderIdFromTaskItem))
				{
					orderId = orderIdFromTaskItem;
				}

				data.Tasks.Add(new TrueMarkCodeHistoryTask
				{
					Id = task.Id,
					Type = task.TaskType,
					Status = task.Status,
					CreationTime = task.CreationTime,
					StartTime = task.StartTime,
					EndTime = task.EndTime,
					OrderId = orderId
				});
			}
		}

		private static void FillOrders(IUnitOfWork uow, TrueMarkCodeHistoryData data)
		{
			var orderIds = data.ProductCodes
				.Where(x => x.OrderId.HasValue)
				.Select(x => x.OrderId.Value)
				.Concat(data.Tasks.Where(x => x.OrderId.HasValue).Select(x => x.OrderId.Value))
				.Distinct()
				.ToArray();

			if(!orderIds.Any())
			{
				return;
			}

			var orders = uow.Session.Query<OrderEntity>()
				.Where(x => orderIds.Contains(x.Id))
				.ToList();

			foreach(var order in orders
				.GroupBy(x => x.Id)
				.Select(x => x.First()))
			{
				data.Orders.Add(new TrueMarkCodeHistoryOrder
				{
					Id = order.Id,
					DeliveryDate = order.DeliveryDate,
					OrderStatus = order.OrderStatus
				});
			}
		}

		private static void FillDocumentFlows(IUnitOfWork uow, TrueMarkCodeHistoryData data)
		{
			var orderTaskIds = data.Tasks.Select(x => x.Id).Distinct().ToArray();

			if(!orderTaskIds.Any())
			{
				return;
			}

			var outgoingDocuments = uow.Session.Query<OrderEdoDocument>()
				.Where(x => orderTaskIds.Contains(x.DocumentTaskId))
				.ToList();

			if(!outgoingDocuments.Any())
			{
				return;
			}

			var outgoingDocumentIds = outgoingDocuments.Select(x => x.Id).ToArray();

			var taxcomDocflows = new Dictionary<int, TaxcomDocflow>();

			foreach(var taxcomDocflow in uow.Session.Query<TaxcomDocflow>()
				.Where(x => outgoingDocumentIds.Contains(x.EdoDocumentId)))
			{
				if(!taxcomDocflows.TryGetValue(taxcomDocflow.EdoDocumentId, out var existingDocflow)
					|| existingDocflow.CreationTime < taxcomDocflow.CreationTime)
				{
					taxcomDocflows[taxcomDocflow.EdoDocumentId] = taxcomDocflow;
				}
			}

			var taskOrderIds = new Dictionary<int, int>();

			foreach(var task in data.Tasks.Where(x => x.OrderId.HasValue))
			{
				if(!taskOrderIds.ContainsKey(task.Id))
				{
					taskOrderIds.Add(task.Id, task.OrderId.Value);
				}
			}

			foreach(var outgoingDocument in outgoingDocuments
				.GroupBy(x => x.Id)
				.Select(x => x.First()))
			{
				taxcomDocflows.TryGetValue(outgoingDocument.Id, out var taxcomDocflow);

				data.DocumentFlows.Add(new TrueMarkCodeHistoryDocumentFlow
				{
					OutgoingDocumentId = outgoingDocument.Id,
					OrderId = taskOrderIds.TryGetValue(outgoingDocument.DocumentTaskId, out var orderId)
						? (int?)orderId
						: null,
					OrderTaskId = outgoingDocument.DocumentTaskId,
					EdoType = outgoingDocument.EdoType,
					DocumentType = outgoingDocument.DocumentType,
					Status = outgoingDocument.Status,
					CreationTime = outgoingDocument.CreationTime,
					SendTime = outgoingDocument.SendTime,
					AcceptTime = outgoingDocument.AcceptTime,
					DiscrepancyState = outgoingDocument.DiscrepancyState,
					TaxcomDocflowId = taxcomDocflow?.DocflowId,
					TaxcomDocflowStatus = taxcomDocflow?.DocFlowState,
					TaxcomStateChangeTime = taxcomDocflow?.StateChangeTime,
					IsReceived = taxcomDocflow?.IsReceived,
					ErrorMessage = taxcomDocflow?.ErrorMessage
				});
			}
		}
	}
}
