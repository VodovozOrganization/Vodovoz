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
	}
}

