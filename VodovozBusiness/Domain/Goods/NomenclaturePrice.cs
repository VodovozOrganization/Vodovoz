using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "цены",
		Nominative = "цена")]
	public partial class NomenclaturePrice: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value, () => Nomenclature); }
		}

		int minCount;

		[Display (Name = "Минимальное количество")]
		public virtual int MinCount {
			get { return minCount; }
			set { SetField (ref minCount, value, () => MinCount); }
		}

		decimal price;

		[Display (Name = "Стоимость")]
		public virtual decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}

		#endregion
	}
}

