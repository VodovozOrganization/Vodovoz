using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Domain {
    public class WaterFixedPricesGenerator {
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly INomenclatureRepository nomenclatureRepository;
        private decimal priceIncrement;

		public WaterFixedPricesGenerator(IUnitOfWorkFactory uowFactory, INomenclatureRepository nomenclatureRepository)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			this.nomenclatureRepository = nomenclatureRepository ??
			                              throw new ArgumentNullException(nameof(nomenclatureRepository));
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

		private void LoadNomenclatures()
		{
			using(var uow = _uowFactory.CreateWithoutRoot()) {
				SemiozeriePrice = 0m;
				priceIncrement = nomenclatureRepository.GetWaterPriceIncrement;
				SemiozerieWater = nomenclatureRepository.GetWaterSemiozerie(uow);
				RuchkiWater = nomenclatureRepository.GetWaterRuchki(uow);
				KislorodnayaWater = nomenclatureRepository.GetWaterKislorodnaya(uow);
				SnyatogorskayaWater = nomenclatureRepository.GetWaterSnyatogorskaya(uow);
				KislorodnayaDeluxeWater = nomenclatureRepository.GetWaterKislorodnayaDeluxe(uow);
			}
		}

		/// <summary>
		/// Создает фиксированные цены по всем номенклатурам воды основываясь на цене одной номенклатуры
		/// </summary>
		/// <returns>Словарь с id номенклатуры и ценой</returns>
		/// <param name="waterNomenclature"> Номенклатура относительно которой будут созданы остальные фиксированные цены</param>
		/// <param name="fixedPrice"> Фиксированная цена для базовой номенклатуры </param>
		public Dictionary<int, decimal> GenerateFixedPricesForAllWater(int waterNomenclatureId, decimal fixedPrice) {
			if(waterNomenclatureId == 0) {
				throw new InvalidOperationException("Невозможно определить цены на воду для новой номенклатуры.");
			}

			LoadNomenclatures();

			var result = new Dictionary<int, decimal>();
			
			if (waterNomenclatureId == SemiozerieWater.Id) {
				SemiozeriePrice = fixedPrice;
			}
			else if (waterNomenclatureId == RuchkiWater.Id) {
				SemiozeriePrice = fixedPrice;
			}
			else if (waterNomenclatureId == KislorodnayaWater.Id) {
				KislorodnayaPrice = fixedPrice;
			}
			else if (waterNomenclatureId == SnyatogorskayaWater.Id) {
				SnyatogorskayaPrice = fixedPrice;
			}
			else if (waterNomenclatureId == KislorodnayaDeluxeWater.Id) {
				KislorodnayaDeluxePrice = fixedPrice;
			}
			else {
				result.Add(waterNomenclatureId, fixedPrice);
				return result;
			}
			
			result.Add(SemiozerieWater.Id, SemiozeriePrice);
			result.Add(RuchkiWater.Id, SemiozeriePrice);
			result.Add(KislorodnayaWater.Id, KislorodnayaPrice);
			result.Add(SnyatogorskayaWater.Id, SnyatogorskayaPrice);
			result.Add(KislorodnayaDeluxeWater.Id, KislorodnayaDeluxePrice);
			return result;
		}
    }
}
