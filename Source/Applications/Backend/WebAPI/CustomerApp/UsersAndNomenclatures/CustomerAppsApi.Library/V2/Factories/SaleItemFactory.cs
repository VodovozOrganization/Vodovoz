using System;
using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.V2.Dto.Goods;
using CustomerAppsApi.Library.V2.Dto.Goods.Attributes;
using Vodovoz.Converters;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Nodes;

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
		
		public NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(NomenclatureOnlineParametersData parametersData)
		{
			return new NomenclaturesPricesAndStockDto
			{
				PricesAndStocks = CreateNomenclaturePricesAndStockDto(parametersData)
			};
		}
		
		public SaleItemsDto CreateSaleItemsDto(IEnumerable<OnlineNomenclatureNode> onlineNomenclatures)
		{
			return new SaleItemsDto
			{
				SaleItems = new List<object>(onlineNomenclatures.Select(CreateSaleItem))
			};
		}

		public object CreateWaterSaleItem(OnlineNomenclatureNode nomenclatureNode)
		{
			var waterItem = new WaterSaleItemDto();
			FillSaleItem(waterItem, nomenclatureNode);
			waterItem.Attributes = new WaterSaleItemAttributes
			{
				IsDisposableTare = nomenclatureNode.IsDisposableTare,
				IsNewBottle = nomenclatureNode.IsNewBottle,
				IsSparklingWater = nomenclatureNode.IsSparklingWater,
				TareVolume = nomenclatureNode.TareVolume,
			};
			
			return waterItem;
		}
		
		public object CreateServiceSaleItem(OnlineNomenclatureNode nomenclatureNode)
		{
			var serviceItem = new ServiceSaleItemDto();
			FillSaleItem(serviceItem, nomenclatureNode);
			serviceItem.Attributes = new ServiceSaleItemAttributes();
			
			return serviceItem;
		}
		
		public object CreateEquipmentSaleItem(OnlineNomenclatureNode nomenclatureNode)
		{
			var equipmentSaleItem = new EquipmentSaleItemDto();
			FillSaleItem(equipmentSaleItem, nomenclatureNode);
			equipmentSaleItem.Attributes = new EquipmentSaleItemAttributes
			{
				EquipmentInstallationType = nomenclatureNode.EquipmentInstallationType,
					EquipmentWorkloadType = nomenclatureNode.EquipmentWorkloadType,
					PumpType = nomenclatureNode.PumpType,
					CupHolderBracingType = nomenclatureNode.CupHolderBracingType,
					HasHeating = nomenclatureNode.HasHeating,
					HeatingPower = nomenclatureNode.HeatingPower,
					HeatingProductivity = nomenclatureNode.HeatingProductivity,
					ProtectionOnHotWaterTap = nomenclatureNode.ProtectionOnHotWaterTap,
					HasCooling = nomenclatureNode.HasCooling,
					CoolingPower = nomenclatureNode.CoolingPower,
					CoolingProductivity = nomenclatureNode.CoolingProductivity,
					CoolingType = nomenclatureNode.CoolingType,
					LockerRefrigeratorType = nomenclatureNode.LockerRefrigeratorType,
					LockerRefrigeratorVolume = nomenclatureNode.LockerRefrigeratorVolume,
					TapType = nomenclatureNode.TapType,
					GlassHolderType = nomenclatureNode.GlassHolderType,
					HeatingTemperatureFrom = nomenclatureNode.HeatingTemperatureFrom,
					HeatingTemperatureTo = nomenclatureNode.HeatingTemperatureTo,
					CoolingTemperatureFrom = nomenclatureNode.CoolingTemperatureFrom,
					CoolingTemperatureTo = nomenclatureNode.CoolingTemperatureTo,
					Length = nomenclatureNode.Length,
					Width = nomenclatureNode.Width,
					Height = nomenclatureNode.Height,
					Weight = nomenclatureNode.Weight,
					Size = _onlineCharacteristicsConverter.GetSizeString(nomenclatureNode.Length, nomenclatureNode.Width, nomenclatureNode.Height),
					WeightString = _onlineCharacteristicsConverter.GetWeightString(nomenclatureNode.Weight),
					HeatingProductivityString = _onlineCharacteristicsConverter.GetProductivityString(
						nomenclatureNode.HeatingProductivityComparisionSign,
						nomenclatureNode.HeatingProductivity,
						nomenclatureNode.HeatingProductivityUnits),
					HeatingPowerString =
						_onlineCharacteristicsConverter.GetPowerString(nomenclatureNode.HeatingPower, nomenclatureNode.HeatingPowerUnits),
					CoolingProductivityString = _onlineCharacteristicsConverter.GetProductivityString(
						nomenclatureNode.CoolingProductivityComparisionSign,
						nomenclatureNode.CoolingProductivity,
						nomenclatureNode.CoolingProductivityUnits),
					CoolingPowerString =
						_onlineCharacteristicsConverter.GetPowerString(nomenclatureNode.CoolingPower, nomenclatureNode.CoolingPowerUnits),
					HeatingTemperatureString =
						_onlineCharacteristicsConverter.GetTemperatureString(
							nomenclatureNode.HeatingTemperatureFrom, nomenclatureNode.HeatingTemperatureTo),
					CoolingTemperatureString =
						_onlineCharacteristicsConverter.GetTemperatureString(
							nomenclatureNode.CoolingTemperatureFrom, nomenclatureNode.CoolingTemperatureTo)
			};
			
			return equipmentSaleItem;
		}
		
		public object CreateOtherSaleItem(OnlineNomenclatureNode nomenclatureNode)
		{
			var serviceItem = new OtherSaleItemDto();
			FillSaleItem(serviceItem, nomenclatureNode);
			serviceItem.Attributes = new OtherSaleItemAttributes();
			
			return serviceItem;
		}
		
		public IEnumerable<object> CreatePromoSetSaleItems(PromotionalSetOnlineParametersData parametersData)
		{
			return parametersData.PromotionalSetOnlineParametersNodes.Select(keyPairValue => new PromoSetSaleItemDto
				{
					ErpId = keyPairValue.Value.PromotionalSetId,
					OnlineName = keyPairValue.Value.PromotionalSetOnlineName,
					OnlineCategory = null,
					OnlineGroup = null,
					OnlineCatalogGuid = null,
					Attributes = new PromoSetSaleItemAttributes
					{
						ForNewClients = keyPairValue.Value.PromotionalSetForNewClients,
						BottlesCountForCalculatingDeliveryPrice = keyPairValue.Value.BottlesCountForCalculatingDeliveryPrice,
						PromotionalNomenclatures =
							CreatePromotionalNomenclatureDto(keyPairValue.Value.PromotionalSetId, parametersData.PromotionalSetItemBalanceNodes)
					}
				})
				.Cast<object>()
				.ToList();
		}
		
		public object CreateFreeRentPackageSaleItems(FreeRentPackageWithOnlineParametersNode packageNode)
		{
			return new FreeRentPackageSaleItemDto
			{
				ErpId = packageNode.Id,
				OnlineName = packageNode.OnlineName,
				OnlineCategory = null,
				OnlineGroup = null,
				OnlineCatalogGuid = null,
				Attributes = new FreeRentPackageAttributes
				{
					Deposit = packageNode.Deposit,
					MinWaterAmount = packageNode.MinWaterAmount,
					DepositServiceId = packageNode.DepositServiceId
				}
			};
		}

		private void FillSaleItem(SaleItemDto saleItem, OnlineNomenclatureNode nomenclatureNode)
		{
			saleItem.ErpId = nomenclatureNode.ErpId;
			saleItem.OnlineCatalogGuid = nomenclatureNode.OnlineCatalogGuid;
			saleItem.OnlineCategory = nomenclatureNode.OnlineCategory;
			saleItem.OnlineGroup = nomenclatureNode.OnlineGroup;
			saleItem.OnlineName = nomenclatureNode.OnlineName;
		}

		private object CreateSaleItem(OnlineNomenclatureNode nomenclatureNode)
		{
			switch(nomenclatureNode.NomenclatureCategory)
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
		
		private IList<NomenclaturePricesAndStockDto> CreateNomenclaturePricesAndStockDto(NomenclatureOnlineParametersData parametersData)
		{
			return parametersData.NomenclatureOnlineParametersNodes.Select(parametersNode => new NomenclaturePricesAndStockDto
				{
					NomenclatureErpId = parametersNode.Value.NomenclatureId,
					AvailableForSale = parametersNode.Value.AvailableForSale,
					Marker = parametersNode.Value.Marker,
					PercentDiscount = parametersNode.Value.PercentDiscount,
					Prices = CreateNomenclaturePricesDto(parametersNode.Value.Id, parametersData.NomenclatureOnlinePricesNodes)
				})
				.ToList();
		}
		
		private IList<NomenclaturePricesDto> CreateNomenclaturePricesDto(
			int parametersId, ILookup<int, NomenclatureOnlinePriceNode> onlinePrices)
		{
			var prices = onlinePrices[parametersId];
			return !prices.Any()
				? new List<NomenclaturePricesDto>()
				: prices.Select(CreateNomenclaturePricesDto).ToList();
		}

		private NomenclaturePricesDto CreateNomenclaturePricesDto(NomenclatureOnlinePriceNode onlinePrice)
		{
			return new NomenclaturePricesDto
			{
				MinCount = onlinePrice.MinCount,
				Price = onlinePrice.Price,
				PriceWithoutDiscount = onlinePrice.PriceWithoutDiscount
			};
		}

		private NomenclaturePricesDto CreateNomenclaturePricesDto(NomenclatureOnlinePrice onlinePrice)
		{
			return new NomenclaturePricesDto
			{
				MinCount = onlinePrice.NomenclaturePrice.MinCount,
				Price = onlinePrice.NomenclaturePrice.Price,
				PriceWithoutDiscount = onlinePrice.PriceWithoutDiscount
			};
		}
		
		private IList<PromotionalNomenclatureDto> CreatePromotionalNomenclatureDto(
			int promoSetId, ILookup<int, PromotionalSetItemBalanceNode> promoSetItems)
		{
			var items = promoSetItems[promoSetId];
			return !items.Any()
				? new List<PromotionalNomenclatureDto>()
				: items.Select(CreatePromotionalNomenclatureDto).ToList();
		}

		private PromotionalNomenclatureDto CreatePromotionalNomenclatureDto(PromotionalSetItemBalanceNode promoSetItem)
		{
			return new PromotionalNomenclatureDto
			{
				Count = promoSetItem.Count,
				Discount = promoSetItem.Discount,
				ErpNomenclatureId = promoSetItem.NomenclatureId,
				IsDiscountMoney = promoSetItem.IsDiscountMoney
			};
		}
	}
}
