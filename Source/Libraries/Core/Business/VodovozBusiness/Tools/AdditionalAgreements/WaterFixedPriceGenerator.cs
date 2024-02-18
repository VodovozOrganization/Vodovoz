using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Tools
{
	public class WaterFixedPriceGenerator
	{
		private readonly IUnitOfWork _uow;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly decimal _priceIncrement;

		public WaterFixedPriceGenerator(IUnitOfWork uow, INomenclatureSettings nomenclatureSettings, INomenclatureRepository nomenclatureRepository)
		{
			_uow = uow;
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			if(nomenclatureSettings == null)
			{
				throw new ArgumentNullException(nameof(nomenclatureSettings));
			}
			
			SemiozeriePrice = 0m;
			_priceIncrement = nomenclatureSettings.GetWaterPriceIncrement;
			SemiozerieWater = _nomenclatureRepository.GetWaterSemiozerie(uow);
			RuchkiWater = _nomenclatureRepository.GetWaterRuchki(uow);
			KislorodnayaWater = _nomenclatureRepository.GetWaterKislorodnaya(uow);
			SnyatogorskayaWater = _nomenclatureRepository.GetWaterSnyatogorskaya(uow);
			KislorodnayaDeluxeWater = _nomenclatureRepository.GetWaterKislorodnayaDeluxe(uow);
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
				return SemiozeriePrice + _priceIncrement;
			}
			set { SemiozeriePrice = value - _priceIncrement; }
		}

		private Nomenclature SnyatogorskayaWater;
		private decimal SnyatogorskayaPrice {
			get {
				if(SemiozeriePrice <= 0m) {
					return 0m;
				}
				return SemiozeriePrice + _priceIncrement * 2;

			}
			set { SemiozeriePrice = value - _priceIncrement * 2; }
		}

		private Nomenclature KislorodnayaDeluxeWater;
		private decimal KislorodnayaDeluxePrice {
			get {
				if(SemiozeriePrice <= 0m) {
					return 0m;
				}
				return SemiozeriePrice + _priceIncrement * 3;
			}
			set { SemiozeriePrice = value - _priceIncrement * 3; }
		}
		
		/// <summary>
		/// Создает фиксированные цены по всем номенклатурам воды основываясь на цене одной номенклатуры
		/// </summary>
		/// <returns>The fixed prices.</returns>
		/// <param name="baseNomenclatureId">ID базовой номенклатуры фикс. цены относителдьно которой будут созданы остальные фиксированные цены</param>
		/// <param name="price">Фиксированная цена для базовой номенклатуры</param>
		public IEnumerable<NomenclatureFixedPrice> GenerateFixedPrices(int baseNomenclatureId, decimal price)
		{
			if(baseNomenclatureId == 0) {
				throw new InvalidOperationException("Невозможно определить цены на воду для новой номенклатуры.");
			}
			var result = new List<NomenclatureFixedPrice>();

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
				var basedNomenclature = _uow.GetById<Nomenclature>(baseNomenclatureId);
				if(basedNomenclature != null) {
					result.Add(CreateNomenclatureFixedPrice(basedNomenclature, price));
				}
				return result;
			}

			result.Add(CreateNomenclatureFixedPrice(SemiozerieWater, SemiozeriePrice));
			result.Add(CreateNomenclatureFixedPrice(RuchkiWater, SemiozeriePrice));
			result.Add(CreateNomenclatureFixedPrice(KislorodnayaWater, KislorodnayaPrice));
			result.Add(CreateNomenclatureFixedPrice(SnyatogorskayaWater, SnyatogorskayaPrice));
			result.Add(CreateNomenclatureFixedPrice(KislorodnayaDeluxeWater, KislorodnayaDeluxePrice));
			return result;
		}

		private NomenclatureFixedPrice CreateNomenclatureFixedPrice(Nomenclature nomenclature, decimal price)
		{
			var nomenclatureFixedPrice = new NomenclatureFixedPrice();
			nomenclatureFixedPrice.Nomenclature = nomenclature;
			nomenclatureFixedPrice.Price = price;
			return nomenclatureFixedPrice;
		}
	}
}
