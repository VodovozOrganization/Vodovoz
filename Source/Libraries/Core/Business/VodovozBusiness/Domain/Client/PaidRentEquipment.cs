using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;

namespace Vodovoz.Domain.Client
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки ПА соглашения",
		Nominative = "строка ПА соглашения")]
	public class PaidRentEquipment : PaidRentEquipmentEntity
	{
		PaidRentPackage _paidRentPackage;
		Equipment _equipment;
		Nomenclature _nomenclature;

		[Display (Name = "Пакет платной аренды")]
		public virtual new PaidRentPackage PaidRentPackage {
			get => _paidRentPackage;
			set => SetField (ref _paidRentPackage, value);
		}

		[Display (Name = "Оборудование")]
		public virtual new Equipment Equipment {
			get => _equipment;
			set => SetField (ref _equipment, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature {
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
	}
}

