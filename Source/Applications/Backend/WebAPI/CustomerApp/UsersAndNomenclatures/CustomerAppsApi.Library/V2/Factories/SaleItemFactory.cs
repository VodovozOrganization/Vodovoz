using System;
using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.V2.Dto;
using CustomerAppsApi.Library.V2.Dto.Goods;
using CustomerAppsApi.Library.V2.Dto.Goods.Attributes;
using CustomerAppsApi.Library.V2.Extensions;
using Vodovoz.Converters;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V2.Factories
{
	public class SaleItemFactory : ISaleItemFactory
	{
		private readonly INomenclatureOnlineCharacteristicsConverter _onlineCharacteristicsConverter;

		public SaleItemFactory(
			INomenclatureOnlineCharacteristicsConverter nomenclatureOnlineCharacteristicsConverter
			)
		{
			_onlineCharacteristicsConverter =
				nomenclatureOnlineCharacteristicsConverter ?? throw new ArgumentNullException(nameof(nomenclatureOnlineCharacteristicsConverter));
		}
		
		public IEnumerable<SaleItemPricesDto> CreateSelItemPricesDto(
			IEnumerable<NomenclatureOnlineParametersDto> nomenclatureParameters,
			IEnumerable<NomenclatureOnlinePriceDto> nomenclaturePrices
			)
		{
			return CreateSelItemPricesDto(
				nomenclatureParameters,
				nomenclaturePrices.ToLookup(x => x.NomenclatureOnlineParametersId)
				);
		}

		public SaleItemsDto CreateSaleItemsDto(AggregatedSaleItems saleItems, IEnumerable<int> availableWaterIds)
		{
			var allSaleItems = new List<object>();
			
			allSaleItems.AddRange(CreateSaleItemList(saleItems.Nomenclatures));
			allSaleItems.AddRange(CreatePromoSetSaleItems(saleItems.PromoSets));
			allSaleItems.AddRange(CreateFreeRentPackageSaleItems(saleItems.RentPackages, availableWaterIds));
			
			return new SaleItemsDto
			{
				SaleItems = allSaleItems
			};
		}

		public SaleItemsPricesAndStockDto CreateSaleItemsPricesAndStockDto(IEnumerable<SaleItemPricesDto> saleItemPrices)
		{
			return new SaleItemsPricesAndStockDto
			{
				PricesAndStocks = saleItemPrices
			};
		}
		
		private IEnumerable<SaleItemPricesDto> CreateSelItemPricesDto(
			IEnumerable<NomenclatureOnlineParametersDto> nomenclatureParameters,
			ILookup<int, NomenclatureOnlinePriceDto> nomenclaturePrices
		)
		{
			return (
				from nomenclatureParameter in nomenclatureParameters
				let prices = nomenclaturePrices[nomenclatureParameter.Id]
				select new SaleItemPricesDto
				{
					ErpId = nomenclatureParameter.NomenclatureId,
					Type = nomenclatureParameter.Category.ToSaleItemType(),
					AvailableForSale = nomenclatureParameter.AvailableForSale,
					Marker = nomenclatureParameter.Marker,
					PercentDiscount = nomenclatureParameter.PercentDiscount,
					Prices = prices.Select(x =>
						new SaleItemPriceDto
						{
							MinCount = x.MinCount,
							Price = x.Price,
							PriceWithoutDiscount = x.PriceWithoutDiscount
						})
				}).ToList();
		}

		private IEnumerable<object> CreateSaleItemList(IEnumerable<OnlineNomenclatureDto> onlineNomenclatures)
		{
			return new List<object>(onlineNomenclatures.Select(CreateSaleItem));
		}

		private object CreateWaterSaleItem(OnlineNomenclatureDto nomenclatureDto)
		{
			var waterItem = new WaterSaleItemDto();
			FillSaleItem(waterItem, nomenclatureDto);
			waterItem.Attributes = new WaterSaleItemAttributes
			{
				IsDisposableTare = nomenclatureDto.IsDisposableTare,
				IsNewBottle = nomenclatureDto.IsNewBottle,
				IsSparklingWater = nomenclatureDto.IsSparklingWater,
				TareVolume = nomenclatureDto.TareVolume,
			};
			
			return waterItem;
		}
		
		private object CreateServiceSaleItem(OnlineNomenclatureDto nomenclatureDto)
		{
			var serviceItem = new ServiceSaleItemDto();
			FillSaleItem(serviceItem, nomenclatureDto);
			serviceItem.Attributes = new ServiceSaleItemAttributes();
			
			return serviceItem;
		}
		
		private object CreateEquipmentSaleItem(OnlineNomenclatureDto nomenclatureDto)
		{
			var equipmentSaleItem = new EquipmentSaleItemDto();
			FillSaleItem(equipmentSaleItem, nomenclatureDto);
			equipmentSaleItem.Attributes = new EquipmentSaleItemAttributes
			{
				EquipmentInstallationType = nomenclatureDto.EquipmentInstallationType,
				EquipmentWorkloadType = nomenclatureDto.EquipmentWorkloadType,
				PumpType = nomenclatureDto.PumpType,
				CupHolderBracingType = nomenclatureDto.CupHolderBracingType,
				HasHeating = nomenclatureDto.HasHeating,
				HeatingPower = nomenclatureDto.HeatingPower,
				HeatingProductivity = nomenclatureDto.HeatingProductivity,
				ProtectionOnHotWaterTap = nomenclatureDto.ProtectionOnHotWaterTap,
				HasCooling = nomenclatureDto.HasCooling,
				CoolingPower = nomenclatureDto.CoolingPower,
				CoolingProductivity = nomenclatureDto.CoolingProductivity,
				CoolingType = nomenclatureDto.CoolingType,
				LockerRefrigeratorType = nomenclatureDto.LockerRefrigeratorType,
				LockerRefrigeratorVolume = nomenclatureDto.LockerRefrigeratorVolume,
				TapType = nomenclatureDto.TapType,
				GlassHolderType = nomenclatureDto.GlassHolderType,
				HeatingTemperatureFrom = nomenclatureDto.HeatingTemperatureFrom,
				HeatingTemperatureTo = nomenclatureDto.HeatingTemperatureTo,
				CoolingTemperatureFrom = nomenclatureDto.CoolingTemperatureFrom,
				CoolingTemperatureTo = nomenclatureDto.CoolingTemperatureTo,
				Length = nomenclatureDto.Length,
				Width = nomenclatureDto.Width,
				Height = nomenclatureDto.Height,
				Weight = nomenclatureDto.Weight,
				Size = nomenclatureDto.Size,
				WeightString = nomenclatureDto.WeightString,
				HeatingProductivityString = nomenclatureDto.HeatingProductivityString,
				HeatingPowerString = nomenclatureDto.HeatingPowerString,
				CoolingProductivityString = nomenclatureDto.CoolingProductivityString,
				CoolingPowerString = nomenclatureDto.CoolingPowerString,
				HeatingTemperatureString = nomenclatureDto.HeatingTemperatureString,
				CoolingTemperatureString = nomenclatureDto.CoolingTemperatureString
			};
			
			return equipmentSaleItem;
		}
		
		private object CreateOtherSaleItem(OnlineNomenclatureDto nomenclatureDto)
		{
			var serviceItem = new OtherSaleItemDto();
			FillSaleItem(serviceItem, nomenclatureDto);
			serviceItem.Attributes = new OtherSaleItemAttributes();
			
			return serviceItem;
		}
		
		private IEnumerable<object> CreatePromoSetSaleItems(IEnumerable<PromotionalSetDto> promoSetsData)
		{
			return promoSetsData.Select(data => new PromoSetSaleItemDto
				{
					ErpId = data.Id,
					OnlineName = data.OnlineName,
					OnlineCategory = null,
					OnlineGroup = null,
					OnlineCatalogGuid = null,
					Attributes = new PromoSetSaleItemAttributes
					{
						ForNewClients = data.ForNewClients,
					}
				})
				.Cast<object>()
				.ToList();
		}
		
		private IEnumerable<object> CreateFreeRentPackageSaleItems(IEnumerable<FreeRentPackageDto> packages, IEnumerable<int> availableWaterIds)
		{
			return packages.Select(x => CreateFreeRentPackageSaleItem(x, availableWaterIds));
		}
		
		private object CreateFreeRentPackageSaleItem(FreeRentPackageDto package, IEnumerable<int> availableWaterIds)
		{
			return new FreeRentPackageSaleItemDto
			{
				ErpId = package.ErpId,
				OnlineName = package.OnlineName,
				OnlineCategory = null,
				OnlineGroup = null,
				OnlineCatalogGuid = null,
				Attributes = new FreeRentPackageAttributes
				{
					MinWaterAmount = package.MinWaterAmount,
					AvailableWaterIds = availableWaterIds
				}
			};
		}

		private void FillSaleItem(SaleItemDto saleItem, OnlineNomenclatureDto nomenclatureDto)
		{
			saleItem.ErpId = nomenclatureDto.ErpId;
			saleItem.OnlineCatalogGuid = nomenclatureDto.OnlineCatalogGuid;
			saleItem.OnlineCategory = nomenclatureDto.OnlineCategory;
			saleItem.OnlineGroup = nomenclatureDto.OnlineGroup;
			saleItem.OnlineName = nomenclatureDto.OnlineName;
		}

		private object CreateSaleItem(OnlineNomenclatureDto nomenclatureNode)
		{
			switch(nomenclatureNode.Category)
			{
				case NomenclatureCategory.water:
					return CreateWaterSaleItem(nomenclatureNode);
				case NomenclatureCategory.master:
				case NomenclatureCategory.service:
					return CreateServiceSaleItem(nomenclatureNode);
				case NomenclatureCategory.equipment:
					return CreateEquipmentSaleItem(nomenclatureNode);
				default:
					return CreateOtherSaleItem(nomenclatureNode);
			}
		}
	}
}
