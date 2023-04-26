using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "цены",
		Nominative = "цена")]
	public class NomenclaturePriceBase : PropertyChangedBase, IDomainObject
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

		public virtual NomenclaturePriceType Type { get; }

		#endregion

		public enum NomenclaturePriceType
		{
			[Display(Name = "Обычная")]
			General,
			[Display(Name = "Альтернативная")]
			Alternative
		}
	}
}

