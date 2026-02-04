using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Library.Converters
{
	public class CarLoadDocumentConverter
	{
		public CarLoadDocumentDto ConvertToApiCarLoadDocument(CarLoadDocumentEntity carLoadDocument, int loadPriority)
		{
			var carLoadDocumentDto = new CarLoadDocumentDto
			{
				Id = carLoadDocument.Id,
				Driver = carLoadDocument.RouteList.Driver?.FullName,
				Car = carLoadDocument.RouteList.Car?.RegistrationNumber,
				LoadPriority = loadPriority,
				State = ConvertToApiLoadOperationState(carLoadDocument.LoadOperationState)
			};

			return carLoadDocumentDto;
		}

		public OrderDto ConvertToApiOrder(
			IEnumerable<CarLoadDocumentItemEntity> carLoadDocumentItems,
			IDictionary<int, IEnumerable<StagingTrueMarkCode>> carLoadDocumentItemsStagingCodes)
		{
			var waterCarLoadDocumentItems = carLoadDocumentItems
				.Where(item => item.Nomenclature != null && item.Nomenclature.Category == NomenclatureCategory.water)
				.ToList();

			var firstDocumentItem = waterCarLoadDocumentItems.FirstOrDefault();

			var apiOrder = new OrderDto
			{
				Id = firstDocumentItem?.OrderId ?? 0,
				CarLoadDocument = firstDocumentItem?.Document?.Id ?? 0,
				State = GetApiOrderLoadOperationState(waterCarLoadDocumentItems, carLoadDocumentItemsStagingCodes),
				Items = GetApiOrderItems(waterCarLoadDocumentItems, carLoadDocumentItemsStagingCodes)
			};

			return apiOrder;
		}

		public NomenclatureDto ConvertToApiNomenclature(
			CarLoadDocumentItemEntity documentItem,
			IDictionary<int, IEnumerable<StagingTrueMarkCode>> carLoadDocumentItemsStagingCodes = null)
		{
			var apiNomenclature = new NomenclatureDto
			{
				NomenclatureId = documentItem.Nomenclature.Id,
				Name = documentItem.Nomenclature.Name,
				Gtin = documentItem.Nomenclature.Gtins.Select(x => x.GtinNumber),
				GroupGtins = documentItem.Nomenclature.GroupGtins.Select(gg => new GroupGtinDto { Gtin = gg.GtinNumber, Count = gg.CodesCount }),
				Quantity = (int)documentItem.Amount,
				Codes = GetApiTrueMarkCodes(documentItem, carLoadDocumentItemsStagingCodes)
			};

			return apiNomenclature;
		}

		private List<OrderItemDto> GetApiOrderItems(
			List<CarLoadDocumentItemEntity> waterCarLoadDocuemntItems,
			IDictionary<int, IEnumerable<StagingTrueMarkCode>> carLoadDocumentItemsStagingCodes)
		{
			var apiOrderItems = new List<OrderItemDto>();

			foreach(var documentItem in waterCarLoadDocuemntItems)
			{
				var apiOrderItem = new OrderItemDto
				{
					NomenclatureId = documentItem.Nomenclature.Id,
					Name = documentItem.Nomenclature.Name,
					Gtin = documentItem.Nomenclature.Gtins.Select(x => x.GtinNumber),
					GroupGtins = documentItem.Nomenclature.GroupGtins.Select(gg => new GroupGtinDto { Gtin = gg.GtinNumber, Count = gg.CodesCount }),
					Quantity = (int)documentItem.Amount,
				};

				apiOrderItem.Codes.AddRange(GetApiTrueMarkCodes(documentItem, carLoadDocumentItemsStagingCodes));

				apiOrderItems.Add(apiOrderItem);
			}

			return apiOrderItems;
		}

		private LoadOperationStateEnumDto GetApiOrderLoadOperationState(
			IEnumerable<CarLoadDocumentItemEntity> carLoadDocumentItems,
			IDictionary<int, IEnumerable<StagingTrueMarkCode>> carLoadDocumentItemsStagingCodes)
		{
			var itemsLoadState = new List<CarLoadDocumentLoadOperationState>();

			foreach(var item in carLoadDocumentItems)
			{
				carLoadDocumentItemsStagingCodes.TryGetValue(item.Id, out var stagingCodes);
				itemsLoadState.Add(item.GetDocumentItemLoadOperationState(stagingCodes ?? Enumerable.Empty<StagingTrueMarkCode>()));
			}

			var apiOrderLoadOperationState = LoadOperationStateEnumDto.NotStarted;

			if(itemsLoadState.Any(st => st == CarLoadDocumentLoadOperationState.InProgress || st == CarLoadDocumentLoadOperationState.Done))
			{
				apiOrderLoadOperationState = LoadOperationStateEnumDto.InProgress;
			}

			if(itemsLoadState.All(st => st == CarLoadDocumentLoadOperationState.Done))
			{
				apiOrderLoadOperationState = LoadOperationStateEnumDto.Done;
			}

			return apiOrderLoadOperationState;
		}

		private IEnumerable<TrueMarkCodeDto> GetApiTrueMarkCodes(
			CarLoadDocumentItemEntity documentItem,
			IDictionary<int, IEnumerable<StagingTrueMarkCode>> carLoadDocumentItemsStagingCodes = null)
		{
			var sequenceNumber = 0;

			if(documentItem.Document.LoadOperationState == CarLoadDocumentLoadOperationState.Done
				|| carLoadDocumentItemsStagingCodes == null)
			{
				return documentItem.TrueMarkCodes
					.Select(code => ConvertToApiTrueMarkCode(code, sequenceNumber++))
					.ToList();
			}

			if(!carLoadDocumentItemsStagingCodes.TryGetValue(documentItem.Id, out var stagingCodes))
			{
				return Enumerable.Empty<TrueMarkCodeDto>();
			}

			if(stagingCodes?.Any() != true)
			{
				return Enumerable.Empty<TrueMarkCodeDto>();
			}

			return stagingCodes
				.Select(PopulateStagingTrueMarkCodes(stagingCodes, sequenceNumber++))
				.ToList();

		}

		private TrueMarkCodeDto ConvertToApiTrueMarkCode(CarLoadDocumentItemTrueMarkProductCode documentTrueMarkCode, int sequenceNumber)
		{
			return new TrueMarkCodeDto
			{
				SequenceNumber = sequenceNumber,
				Code = documentTrueMarkCode.SourceCode.RawCode,
				Level = WarehouseApiTruemarkCodeLevel.unit
			};
		}

		public Func<StagingTrueMarkCode, TrueMarkCodeDto> PopulateStagingTrueMarkCodes(
			IEnumerable<StagingTrueMarkCode> allCodes,
			int sequenceNumber = 0)
		{
			return stagingCode =>
			{
				string parentRawCode = null;

				if(stagingCode.ParentCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.Id == stagingCode.ParentCodeId)
						?.RawCode;
				}

				WarehouseApiTruemarkCodeLevel level;

				switch(stagingCode.CodeType)
				{
					case StagingTrueMarkCodeType.Transport:
						level = WarehouseApiTruemarkCodeLevel.transport;
						break;
					case StagingTrueMarkCodeType.Group:
						level = WarehouseApiTruemarkCodeLevel.group;
						break;
					case StagingTrueMarkCodeType.Identification:
						level = WarehouseApiTruemarkCodeLevel.unit;
						break;
					default:
						throw new InvalidOperationException("Unknown StagingTrueMarkCodeLevel");
				}

				return new TrueMarkCodeDto
				{
					Code = stagingCode.RawCode,
					Level = level,
					Parent = parentRawCode
				};
			};
		}

		private LoadOperationStateEnumDto ConvertToApiLoadOperationState(CarLoadDocumentLoadOperationState documentLoadOperationState)
		{
			return (LoadOperationStateEnumDto)Enum.Parse(typeof(LoadOperationStateEnumDto), documentLoadOperationState.ToString());
		}
	}
}
