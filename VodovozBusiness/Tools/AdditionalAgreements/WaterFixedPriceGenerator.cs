using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QSSupportLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Repository;

namespace Vodovoz.Tools.AdditionalAgreements
{
	public class WaterFixedPriceGenerator
	{
		IUnitOfWork uow;
		decimal priceIncrement;

		public WaterFixedPriceGenerator(IUnitOfWork uow)
		{
			this.uow = uow;
			SemiozeriePrice = 0m;
			priceIncrement = GetWaterPriceIncrement();
			SemiozerieWater = NomenclatureRepository.GetWaterSemiozerie(uow);
			RuchkiWater = NomenclatureRepository.GetWaterRuchki(uow);
			KislorodnayaWater = NomenclatureRepository.GetWaterKislorodnaya(uow);
			SnyatogorskayaWater = NomenclatureRepository.GetWaterSnyatogorskaya(uow);
			KislorodnayaDeluxeWater = NomenclatureRepository.GetWaterKislorodnayaDeluxe(uow);
		}

		private Nomenclature SemiozerieWater;
		private Nomenclature RuchkiWater;
		private decimal semiozeriePrice;
		private decimal SemiozeriePrice {
			get {
				if(semiozeriePrice <= 0m) {
					return 0m;
				}
				return semiozeriePrice;
			}
			set { semiozeriePrice = value; }
		}

		private Nomenclature KislorodnayaWater;
		private decimal KislorodnayaPrice {
			get {
				if(SemiozeriePrice <= 0m) {
					return 0m;
				}
				return SemiozeriePrice + priceIncrement;
			}
			set { SemiozeriePrice = value - priceIncrement; }
		}

		private Nomenclature SnyatogorskayaWater;
		private decimal SnyatogorskayaPrice {
			get {
				if(SemiozeriePrice <= 0m) {
					return 0m;
				}
				return SemiozeriePrice + priceIncrement * 2;

			}
			set { SemiozeriePrice = value - priceIncrement * 2; }
		}

		private Nomenclature KislorodnayaDeluxeWater;
		private decimal KislorodnayaDeluxePrice {
			get {
				if(SemiozeriePrice <= 0m) {
					return 0m;
				}
				return SemiozeriePrice + priceIncrement * 3;
			}
			set { SemiozeriePrice = value - priceIncrement * 3; }
		}

		/// <summary>
		/// Создает фиксированные цены по всем номенклатурам воды основываясь на цене одной номенклатуры
		/// </summary>
		/// <returns>The fixed prices.</returns>
		/// <param name="baseNomenclatureId">ID базовой номенклатуры фикс. цены относителдьно которой будут созданы остальные фиксированные цены</param>
		/// <param name="price">Фиксированная цена для базовой номенклатуры</param>
		public IEnumerable<WaterSalesAgreementFixedPrice> GenerateFixedPrices(int baseNomenclatureId, decimal price)
		{
			if(baseNomenclatureId == 0) {
				throw new InvalidOperationException("Невозможно определить цены на воду для новой номенклатуры.");
			}
			var result = new List<WaterSalesAgreementFixedPrice>();

			if(baseNomenclatureId == SemiozerieWater.Id) {
				SemiozeriePrice = price;
			} else if(baseNomenclatureId == RuchkiWater.Id) {
				SemiozeriePrice = price;
			} else if(baseNomenclatureId == KislorodnayaWater.Id) {
				KislorodnayaPrice = price;
			} else if(baseNomenclatureId == SnyatogorskayaWater.Id) {
				SnyatogorskayaPrice = price;
			} else if(baseNomenclatureId == KislorodnayaDeluxeWater.Id) {
				KislorodnayaDeluxePrice = price;
			} else {
				var basedNomenclature = uow.GetById<Nomenclature>(baseNomenclatureId);
				if(basedNomenclature != null) {
					result.Add(new WaterSalesAgreementFixedPrice(basedNomenclature, price));
				}
				return result;
			}

			result.Add(new WaterSalesAgreementFixedPrice(SemiozerieWater, SemiozeriePrice));
			result.Add(new WaterSalesAgreementFixedPrice(RuchkiWater, SemiozeriePrice));
			result.Add(new WaterSalesAgreementFixedPrice(KislorodnayaWater, KislorodnayaPrice));
			result.Add(new WaterSalesAgreementFixedPrice(SnyatogorskayaWater, SnyatogorskayaPrice));
			result.Add(new WaterSalesAgreementFixedPrice(KislorodnayaDeluxeWater, KislorodnayaDeluxePrice));
			return result;
		}

		private decimal GetWaterPriceIncrement()
		{
			var waterPriceParam = "water_price_increment";
			if(!MainSupport.BaseParameters.All.ContainsKey(waterPriceParam))
				throw new InvalidProgramException("В параметрах базы не настроено значение инкремента для цен на воду");
			return decimal.Parse(MainSupport.BaseParameters.All[waterPriceParam]);
		}
	}
}
