using NHibernate;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;

namespace WarehouseApi.Library.Services
{
	/// <summary>
	/// Предзагрузка кодов ЧЗ и связанных сущностей для складского API.
	/// Обход родителей повторяет <c>TrueMarkCodeRepository.PreloadCodes</c>,
	/// дополнительно одним проходом забираются вложенные коды.
	/// </summary>
	public sealed class WarehouseTrueMarkCodePreloader
	{
		private readonly IUnitOfWork _uow;

		private Dictionary<int, TrueMarkWaterGroupCode> _waterGroupCodes = new Dictionary<int, TrueMarkWaterGroupCode>();
		private Dictionary<int, TrueMarkTransportCode> _transportCodes = new Dictionary<int, TrueMarkTransportCode>();
		private Dictionary<int, TrueMarkWaterIdentificationCode> _waterCodes = new Dictionary<int, TrueMarkWaterIdentificationCode>();

		private ILookup<int, TrueMarkTransportCode> _childTransportsByParent = EmptyLookup<TrueMarkTransportCode>();
		private ILookup<int, TrueMarkWaterGroupCode> _childGroupsByTransport = EmptyLookup<TrueMarkWaterGroupCode>();
		private ILookup<int, TrueMarkWaterGroupCode> _childGroupsByGroup = EmptyLookup<TrueMarkWaterGroupCode>();
		private ILookup<int, TrueMarkWaterIdentificationCode> _childWatersByTransport = EmptyLookup<TrueMarkWaterIdentificationCode>();
		private ILookup<int, TrueMarkWaterIdentificationCode> _childWatersByGroup = EmptyLookup<TrueMarkWaterIdentificationCode>();

		public WarehouseTrueMarkCodePreloader(IUnitOfWork uow)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public async Task PreloadCarLoadDocumentItems(
			IEnumerable<CarLoadDocumentItem> documentItems,
			CancellationToken cancellationToken)
		{
			var items = documentItems?.ToArray() ?? Array.Empty<CarLoadDocumentItem>();
			if(items.Length == 0)
			{
				return;
			}

			var itemIds = items.Select(x => x.Id).ToArray();

			// предзагрузка для ускорения
			var productCodes = await _uow.Session.Query<CarLoadDocumentItemTrueMarkProductCode>()
				.Where(x => itemIds.Contains(x.CarLoadDocumentItem.Id))
				.Fetch(x => x.SourceCode)
				.Fetch(x => x.ResultCode)
				.ToListAsync(cancellationToken);

			await _uow.Session.Query<CarLoadDocumentItemEntity>()
				.Where(x => itemIds.Contains(x.Id))
				.FetchMany(x => x.TrueMarkCodes)
				.Fetch(x => x.Nomenclature)
				.Fetch(x => x.Document)
				.ToListAsync(cancellationToken);

			await PreloadNomenclatures(items.Select(x => x.Nomenclature.Id), cancellationToken);
			await PreloadProductCodes(productCodes, cancellationToken);
		}

		public async Task PreloadSelfDelivery(Order order, SelfDeliveryDocument selfDeliveryDocument, CancellationToken cancellationToken)
		{
			if(order != null)
			{
				await _uow.Session.Query<OrderEntity>()
					.Where(x => x.Id == order.Id)
					.FetchMany(x => x.OrderItems)
					.ToListAsync(cancellationToken);

				await PreloadNomenclatures(order.OrderItems.Select(x => x.Nomenclature.Id), cancellationToken);
			}

			if(selfDeliveryDocument == null)
			{
				return;
			}

			await _uow.Session.Query<SelfDeliveryDocumentEntity>()
				.Where(x => x.Id == selfDeliveryDocument.Id)
				.FetchMany(x => x.Items)
				.ToListAsync(cancellationToken);

			var itemIds = selfDeliveryDocument.Items.Select(x => x.Id).ToArray();
			if(itemIds.Length == 0)
			{
				return;
			}

			// предзагрузка для ускорения
			var productCodes = await _uow.Session.Query<SelfDeliveryDocumentItemTrueMarkProductCode>()
				.Where(x => itemIds.Contains(x.SelfDeliveryDocumentItem.Id))
				.Fetch(x => x.SourceCode)
				.Fetch(x => x.ResultCode)
				.ToListAsync(cancellationToken);

			await _uow.Session.Query<SelfDeliveryDocumentItemEntity>()
				.Where(x => itemIds.Contains(x.Id))
				.FetchMany(x => x.TrueMarkProductCodes)
				.Fetch(x => x.Nomenclature)
				.ToListAsync(cancellationToken);

			await PreloadProductCodes(productCodes, cancellationToken);
		}

		public async Task PreloadProductCodes(IEnumerable<TrueMarkProductCode> productCodes, CancellationToken cancellationToken)
		{
			var sourceCodes = productCodes
				.Where(x => x.SourceCode != null)
				.Select(x => x.SourceCode);

			var resultCodes = productCodes
				.Where(x => x.ResultCode != null)
				.Select(x => x.ResultCode);

			var codesToPreload = sourceCodes.Union(resultCodes).Distinct();
			await PreloadCodes(codesToPreload, cancellationToken);
		}

		public async Task PreloadCodes(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken)
		{
			_waterGroupCodes.Clear();
			_transportCodes.Clear();
			_waterCodes.Clear();

			foreach(var code in codes)
			{
				if(code != null && !_waterCodes.ContainsKey(code.Id))
				{
					_waterCodes.Add(code.Id, code);
				}
			}

			var transportCodeIds = new HashSet<int>(
				_waterCodes.Values.Where(x => x.ParentTransportCodeId.HasValue)
					.Select(x => x.ParentTransportCodeId.Value)
			);

			var loadedGroupIds = new HashSet<int>();
			var groupCodeIds = _waterCodes.Values.Where(x => x.ParentWaterGroupCodeId.HasValue)
				.Select(x => x.ParentWaterGroupCodeId.Value)
				.Distinct()
				.ToArray();

			while(groupCodeIds.Any())
			{
				groupCodeIds = groupCodeIds.Where(id => loadedGroupIds.Add(id)).ToArray();
				if(groupCodeIds.Length == 0)
				{
					break;
				}

				var groupCodes = await _uow.Session.QueryOver<TrueMarkWaterGroupCode>()
					.WhereRestrictionOn(x => x.Id).IsIn(groupCodeIds)
					.ListAsync(cancellationToken);
				groupCodeIds = groupCodes.Where(x => x.ParentWaterGroupCodeId.HasValue)
					.Select(x => x.ParentWaterGroupCodeId.Value)
					.Distinct()
					.ToArray();

				foreach(var groupCode in groupCodes)
				{
					if(!_waterGroupCodes.ContainsKey(groupCode.Id))
					{
						_waterGroupCodes.Add(groupCode.Id, groupCode);
					}
				}

				foreach(var groupCode in groupCodes.Where(x => x.ParentTransportCodeId.HasValue))
				{
					if(!transportCodeIds.Contains(groupCode.ParentTransportCodeId.Value))
					{
						transportCodeIds.Add(groupCode.ParentTransportCodeId.Value);
					}
				}
			}

			var nextTransportIds = transportCodeIds.ToArray();
			while(nextTransportIds.Any())
			{
				var loadedTransports = await _uow.Session.QueryOver<TrueMarkTransportCode>()
					.WhereRestrictionOn(x => x.Id).IsIn(nextTransportIds)
					.ListAsync(cancellationToken);

				var parentIds = new List<int>();
				foreach(var transportCode in loadedTransports)
				{
					if(!_transportCodes.ContainsKey(transportCode.Id))
					{
						_transportCodes.Add(transportCode.Id, transportCode);
					}

					if(transportCode.ParentTransportCodeId.HasValue
						&& !_transportCodes.ContainsKey(transportCode.ParentTransportCodeId.Value))
					{
						parentIds.Add(transportCode.ParentTransportCodeId.Value);
					}
				}

				nextTransportIds = parentIds.Distinct().ToArray();
			}

			await PreloadChildCodes(cancellationToken);
		}

		public TrueMarkAnyCode GetParentGroupCode(TrueMarkAnyCode trueMarkAnyCode)
		{
			if(trueMarkAnyCode == null)
			{
				throw new ArgumentNullException(nameof(trueMarkAnyCode), "Передано пустое значение в параметр кода");
			}

			return trueMarkAnyCode.Match(
				transportCode =>
				{
					if(transportCode.ParentTransportCodeId != null)
					{
						return GetParentGroupCode(GetTransportCode(transportCode.ParentTransportCodeId.Value));
					}

					return transportCode;
				},
				groupCode =>
				{
					if(groupCode.ParentTransportCodeId != null)
					{
						return GetParentGroupCode(GetTransportCode(groupCode.ParentTransportCodeId.Value));
					}

					if(groupCode.ParentWaterGroupCodeId != null)
					{
						return GetParentGroupCode(GetGroupCode(groupCode.ParentWaterGroupCodeId.Value));
					}

					return groupCode;
				},
				waterCode =>
				{
					if(waterCode.ParentWaterGroupCodeId != null)
					{
						return GetParentGroupCode(GetGroupCode(waterCode.ParentWaterGroupCodeId.Value));
					}

					if(waterCode.ParentTransportCodeId != null)
					{
						return GetParentGroupCode(GetTransportCode(waterCode.ParentTransportCodeId.Value));
					}

					return waterCode;
				});
		}

		public IEnumerable<TrueMarkAnyCode> GetAllCodes(TrueMarkAnyCode parentCode)
		{
			if(parentCode == null)
			{
				throw new ArgumentNullException(nameof(parentCode));
			}

			return parentCode.Match(
				transportCode => GetAllCodes(transportCode, new HashSet<int>(), new HashSet<int>()),
				groupCode => GetAllCodes(groupCode, new HashSet<int>(), new HashSet<int>()),
				waterCode => new TrueMarkAnyCode[] { waterCode });
		}

		private async Task PreloadNomenclatures(IEnumerable<int> nomenclatureIds, CancellationToken cancellationToken)
		{
			var ids = nomenclatureIds.Distinct().ToArray();
			if(ids.Length == 0)
			{
				return;
			}

			await _uow.Session.Query<NomenclatureEntity>()
				.Where(x => ids.Contains(x.Id))
				.FetchMany(x => x.Gtins)
				.ToListAsync(cancellationToken);

			await _uow.Session.Query<NomenclatureEntity>()
				.Where(x => ids.Contains(x.Id))
				.FetchMany(x => x.GroupGtins)
				.ToListAsync(cancellationToken);
		}

		private async Task PreloadChildCodes(CancellationToken cancellationToken)
		{
			var groupIdsToVisit = new HashSet<int>(_waterGroupCodes.Keys);
			var transportIdsToVisit = new HashSet<int>(_transportCodes.Keys);
			var visitedGroupIds = new HashSet<int>();
			var visitedTransportIds = new HashSet<int>();

			while(groupIdsToVisit.Count > 0 || transportIdsToVisit.Count > 0)
			{
				var groupIds = groupIdsToVisit.ToArray();
				var transportIds = transportIdsToVisit.ToArray();
				groupIdsToVisit.Clear();
				transportIdsToVisit.Clear();

				foreach(var groupId in groupIds)
				{
					visitedGroupIds.Add(groupId);
				}

				foreach(var transportId in transportIds)
				{
					visitedTransportIds.Add(transportId);
				}

				if(transportIds.Length > 0)
				{
					var childTransports = await _uow.Session.QueryOver<TrueMarkTransportCode>()
						.WhereRestrictionOn(x => x.ParentTransportCodeId).IsIn(transportIds)
						.ListAsync(cancellationToken);

					foreach(var childTransport in childTransports)
					{
						if(!_transportCodes.ContainsKey(childTransport.Id))
						{
							_transportCodes.Add(childTransport.Id, childTransport);
						}

						if(visitedTransportIds.Add(childTransport.Id))
						{
							transportIdsToVisit.Add(childTransport.Id);
						}
					}

					var childGroups = await _uow.Session.QueryOver<TrueMarkWaterGroupCode>()
						.WhereRestrictionOn(x => x.ParentTransportCodeId).IsIn(transportIds)
						.ListAsync(cancellationToken);

					foreach(var childGroup in childGroups)
					{
						if(!_waterGroupCodes.ContainsKey(childGroup.Id))
						{
							_waterGroupCodes.Add(childGroup.Id, childGroup);
						}

						if(visitedGroupIds.Add(childGroup.Id))
						{
							groupIdsToVisit.Add(childGroup.Id);
						}
					}

					var childWaters = await _uow.Session.QueryOver<TrueMarkWaterIdentificationCode>()
						.WhereRestrictionOn(x => x.ParentTransportCodeId).IsIn(transportIds)
						.ListAsync(cancellationToken);

					foreach(var childWater in childWaters)
					{
						if(!_waterCodes.ContainsKey(childWater.Id))
						{
							_waterCodes.Add(childWater.Id, childWater);
						}
					}
				}

				if(groupIds.Length > 0)
				{
					var childGroups = await _uow.Session.QueryOver<TrueMarkWaterGroupCode>()
						.WhereRestrictionOn(x => x.ParentWaterGroupCodeId).IsIn(groupIds)
						.ListAsync(cancellationToken);

					foreach(var childGroup in childGroups)
					{
						if(!_waterGroupCodes.ContainsKey(childGroup.Id))
						{
							_waterGroupCodes.Add(childGroup.Id, childGroup);
						}

						if(visitedGroupIds.Add(childGroup.Id))
						{
							groupIdsToVisit.Add(childGroup.Id);
						}
					}

					var childWaters = await _uow.Session.QueryOver<TrueMarkWaterIdentificationCode>()
						.WhereRestrictionOn(x => x.ParentWaterGroupCodeId).IsIn(groupIds)
						.ListAsync(cancellationToken);

					foreach(var childWater in childWaters)
					{
						if(!_waterCodes.ContainsKey(childWater.Id))
						{
							_waterCodes.Add(childWater.Id, childWater);
						}
					}
				}
			}

			_childTransportsByParent = _transportCodes.Values
				.Where(x => x.ParentTransportCodeId.HasValue)
				.ToLookup(x => x.ParentTransportCodeId.Value);
			_childGroupsByTransport = _waterGroupCodes.Values
				.Where(x => x.ParentTransportCodeId.HasValue)
				.ToLookup(x => x.ParentTransportCodeId.Value);
			_childGroupsByGroup = _waterGroupCodes.Values
				.Where(x => x.ParentWaterGroupCodeId.HasValue)
				.ToLookup(x => x.ParentWaterGroupCodeId.Value);
			_childWatersByTransport = _waterCodes.Values
				.Where(x => x.ParentTransportCodeId.HasValue)
				.ToLookup(x => x.ParentTransportCodeId.Value);
			_childWatersByGroup = _waterCodes.Values
				.Where(x => x.ParentWaterGroupCodeId.HasValue)
				.ToLookup(x => x.ParentWaterGroupCodeId.Value);
		}

		private TrueMarkWaterGroupCode GetGroupCode(int id)
		{
			if(!_waterGroupCodes.TryGetValue(id, out var groupCode))
			{
				groupCode = _uow.Session.Get<TrueMarkWaterGroupCode>(id);
				if(groupCode != null)
				{
					_waterGroupCodes.Add(id, groupCode);
				}
			}

			return groupCode;
		}

		private TrueMarkTransportCode GetTransportCode(int id)
		{
			if(!_transportCodes.TryGetValue(id, out var transportCode))
			{
				transportCode = _uow.Session.Get<TrueMarkTransportCode>(id);
				if(transportCode != null)
				{
					_transportCodes.Add(id, transportCode);
				}
			}

			return transportCode;
		}

		private IEnumerable<TrueMarkAnyCode> GetAllCodes(
			TrueMarkTransportCode transportCode,
			HashSet<int> visitedTransports,
			HashSet<int> visitedGroups)
		{
			if(!visitedTransports.Add(transportCode.Id))
			{
				yield break;
			}

			yield return transportCode;

			foreach(var innerTransportCode in _childTransportsByParent[transportCode.Id])
			{
				foreach(var code in GetAllCodes(innerTransportCode, visitedTransports, visitedGroups))
				{
					yield return code;
				}
			}

			foreach(var innerGroupCode in _childGroupsByTransport[transportCode.Id])
			{
				foreach(var code in GetAllCodes(innerGroupCode, visitedTransports, visitedGroups))
				{
					yield return code;
				}
			}

			foreach(var innerWaterCode in _childWatersByTransport[transportCode.Id])
			{
				yield return innerWaterCode;
			}
		}

		private IEnumerable<TrueMarkAnyCode> GetAllCodes(
			TrueMarkWaterGroupCode groupCode,
			HashSet<int> visitedTransports,
			HashSet<int> visitedGroups)
		{
			if(!visitedGroups.Add(groupCode.Id))
			{
				yield break;
			}

			yield return groupCode;

			foreach(var innerGroupCode in _childGroupsByGroup[groupCode.Id])
			{
				foreach(var code in GetAllCodes(innerGroupCode, visitedTransports, visitedGroups))
				{
					yield return code;
				}
			}

			foreach(var innerWaterCode in _childWatersByGroup[groupCode.Id])
			{
				yield return innerWaterCode;
			}
		}

		private static ILookup<int, TCode> EmptyLookup<TCode>()
		{
			return Enumerable.Empty<TCode>().ToLookup(x => 0);
		}
	}
}
