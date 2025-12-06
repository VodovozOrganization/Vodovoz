using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Documents;
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

		public OrderDto ConvertToApiOrder(IEnumerable<CarLoadDocumentItemEntity> carLoadDocumentItems)
		{
			var waterCarLoadDocumentItems = carLoadDocumentItems
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.ToList();

			var firstDocumentItem = waterCarLoadDocumentItems.FirstOrDefault();

			var apiOrder = new OrderDto
			{
				Id = firstDocumentItem?.OrderId ?? 0,
				CarLoadDocument = firstDocumentItem?.Document?.Id ?? 0,
				State = GetApiOrderLoadOperationState(waterCarLoadDocumentItems),
				Items = GetApiOrderItems(waterCarLoadDocumentItems)
			};

			return apiOrder;
		}

		public NomenclatureDto ConvertToApiNomenclature(CarLoadDocumentItemEntity documentItem)
		{
			var apiNomenclature = new NomenclatureDto
			{
				NomenclatureId = documentItem.Nomenclature.Id,
				Name = documentItem.Nomenclature.Name,
				Gtin = documentItem.Nomenclature.Gtins.Select(x => x.GtinNumber),
				GroupGtins = documentItem.Nomenclature.GroupGtins.Select(gg => new GroupGtinDto { Gtin = gg.GtinNumber, Count = gg.CodesCount }),
				Quantity = (int)documentItem.Amount,
				Codes = GetApiTrueMarkCodes(documentItem)
			};

			return apiNomenclature;
		}

		private List<OrderItemDto> GetApiOrderItems(List<CarLoadDocumentItemEntity> waterCarLoadDocuemntItems)
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

				apiOrderItem.Codes.AddRange(GetApiTrueMarkCodes(documentItem));

				apiOrderItems.Add(apiOrderItem);
			}

			return apiOrderItems;
		}

		private LoadOperationStateEnumDto GetApiOrderLoadOperationState(IEnumerable<CarLoadDocumentItemEntity> carLoadDocumentItems)
		{
			var itemsLoadState = new List<CarLoadDocumentLoadOperationState>();

			foreach(var item in carLoadDocumentItems)
			{
				itemsLoadState.Add(item.GetDocumentItemLoadOperationState());
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

		private IEnumerable<TrueMarkCodeDto> GetApiTrueMarkCodes(CarLoadDocumentItemEntity documentItem)
		{
			var sequenceNumber = 0;

			var apiTrueMarkCodes =
				documentItem.TrueMarkCodes
				.Select(code => ConvertToApiTrueMarkCode(code, sequenceNumber++))
				.ToList();

			return apiTrueMarkCodes;
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

		private LoadOperationStateEnumDto ConvertToApiLoadOperationState(CarLoadDocumentLoadOperationState documentLoadOperationState)
		{
			return (LoadOperationStateEnumDto)Enum.Parse(typeof(LoadOperationStateEnumDto), documentLoadOperationState.ToString());
		}
	}
}
