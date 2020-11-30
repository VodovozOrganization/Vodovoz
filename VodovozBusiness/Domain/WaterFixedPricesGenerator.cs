using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Domain {
    public class WaterFixedPricesGenerator {
        private readonly INomenclatureRepository nomenclatureRepository;
        private readonly IUnitOfWork uow;
        private decimal priceIncrement;

		public WaterFixedPricesGenerator(IUnitOfWork uow, INomenclatureRepository nomenclatureRepository)
		{
			this.uow = uow;
			this.nomenclatureRepository = nomenclatureRepository ??
			                              throw new ArgumentNullException(nameof(nomenclatureRepository));
			
			Initialize();
		}

		private void Initialize() {
			SemiozeriePrice = 0m;
			priceIncrement = nomenclatureRepository.GetWaterPriceIncrement;
			SemiozerieWater = nomenclatureRepository.GetWaterSemiozerie(uow);
			RuchkiWater = nomenclatureRepository.GetWaterRuchki(uow);
			KislorodnayaWater = nomenclatureRepository.GetWaterKislorodnaya(uow);
			SnyatogorskayaWater = nomenclatureRepository.GetWaterSnyatogorskaya(uow);
			KislorodnayaDeluxeWater = nomenclatureRepository.GetWaterKislorodnayaDeluxe(uow);
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
			set => semiozeriePrice = value;
		}

		private Nomenclature KislorodnayaWater;
		private decimal KislorodnayaPrice {
			get {
				if(SemiozeriePrice <= 0m) {
					return 0m;
				}
				return SemiozeriePrice + priceIncrement;
			}
			set => SemiozeriePrice = value - priceIncrement;
		}

		private Nomenclature SnyatogorskayaWater;
		private decimal SnyatogorskayaPrice {
			get {
				if(SemiozeriePrice <= 0m) {
					return 0m;
				}
				return SemiozeriePrice + priceIncrement * 2;
			}
			set => SemiozeriePrice = value - priceIncrement * 2;
		}

		private Nomenclature KislorodnayaDeluxeWater;
		private decimal KislorodnayaDeluxePrice {
			get {
				if(SemiozeriePrice <= 0m) {
					return 0m;
				}
				return SemiozeriePrice + priceIncrement * 3;
			}
			set => SemiozeriePrice = value - priceIncrement * 3;
		}

		/// <summary>
		/// Создает фиксированные цены по всем номенклатурам воды основываясь на цене одной номенклатуры
		/// </summary>
		/// <returns>The fixed prices.</returns>
		/// <param name="waterNomenclature"> Номенклатура относительно которой будут созданы остальные фиксированные цены</param>
		/// <param name="fixedPrice"> Фиксированная цена для базовой номенклатуры </param>
		public Dictionary<Nomenclature, decimal> GenerateFixedPricesForAllWater(
			Nomenclature waterNomenclature, decimal fixedPrice) {

			if(waterNomenclature.Id == 0) {
				throw new InvalidOperationException("Невозможно определить цены на воду для новой номенклатуры.");
			}
			
			var result = new Dictionary<Nomenclature, decimal>();
			
			if (waterNomenclature.Id == SemiozerieWater.Id) {
				SemiozeriePrice = fixedPrice;
			}
			else if (waterNomenclature.Id == RuchkiWater.Id) {
				SemiozeriePrice = fixedPrice;
			}
			else if (waterNomenclature.Id == KislorodnayaWater.Id) {
				KislorodnayaPrice = fixedPrice;
			}
			else if (waterNomenclature.Id == SnyatogorskayaWater.Id) {
				SnyatogorskayaPrice = fixedPrice;
			}
			else if (waterNomenclature.Id == KislorodnayaDeluxeWater.Id) {
				KislorodnayaDeluxePrice = fixedPrice;
			}
			else {
				var basedNomenclature = uow.GetById<Nomenclature>(waterNomenclature.Id);
				
				if (basedNomenclature != null) {
					result.Add(basedNomenclature, fixedPrice);
				}

				return result;
			}
			
			result.Add(SemiozerieWater, SemiozeriePrice);
			result.Add(RuchkiWater, SemiozeriePrice);
			result.Add(KislorodnayaWater, KislorodnayaPrice);
			result.Add(SnyatogorskayaWater, SnyatogorskayaPrice);
			result.Add(KislorodnayaDeluxeWater, KislorodnayaDeluxePrice);
			return result;
		}
    }
}