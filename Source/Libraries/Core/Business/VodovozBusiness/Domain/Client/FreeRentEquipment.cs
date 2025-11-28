using QS.DomainModel.Entity;
using QS.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;

namespace Vodovoz.Domain.Client
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки БА соглашения",
		Nominative = "строка БА соглашения")]
	public class FreeRentEquipment : FreeRentEquipmentEntity
	{
		private FreeRentPackage _freeRentPackage;
		private Equipment _equipment;
		private Nomenclature _nomenclature;

		/// <summary>
		/// Пакет бесплатной аренды
		/// </summary>
		[Display (Name = "Пакет бесплатной аренды")]
		public virtual new FreeRentPackage FreeRentPackage {
			get => _freeRentPackage;
			set => SetField (ref _freeRentPackage, value);
		}

		/// <summary>
		/// Оборудование
		/// </summary>
		[Display (Name = "Оборудование")]
		public virtual new Equipment Equipment {
			get => _equipment;
			set => SetField (ref _equipment, value);
		}

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature {
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Название пакета бесплатной аренды
		/// </summary>
		public virtual new string PackageName => FreeRentPackage != null ? FreeRentPackage.Name : "";

		/// <summary>
		/// Выводит имя из оборудования если посерийный учет, иначе из номенклатуры 
		/// </summary>
		/// <value>The name of the equipment.</value>
		public virtual new string EquipmentName { 
			get {
				if(Equipment != null) {
					return Equipment.NomenclatureName;
				} else if(Nomenclature != null){
					return Nomenclature.Name;
				}else {
					return String.Empty;
				}
			} 
		}

		/// <summary>
		/// Серийный номер оборудования
		/// </summary>
		public virtual new string EquipmentSerial => Equipment != null && Equipment.Nomenclature.IsSerial ? Equipment.Serial : "";

		/// <summary>
		/// Строковое представление залога
		/// </summary>
		public virtual new string DepositString => CurrencyWorks.GetShortCurrencyString(Deposit);

		/// <summary>
		/// Строковое представление кол-ва бутылей
		/// </summary>
		public override string WaterAmountString => $"{WaterAmount} {NumberToTextRus.Case(WaterAmount, "бутыль", "бутыли", "бутылей")}";

		/// <summary>
		/// Заголовок 
		/// </summary>
		public virtual new string Title => $"Бесплатная аренда {EquipmentName}";
	}
}

