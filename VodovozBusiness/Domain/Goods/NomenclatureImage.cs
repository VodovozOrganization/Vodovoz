using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods
{

	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "изображения номенклатуры",
		Nominative = "изображение номенклатуры")]
	public class NomenclatureImage : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}

		[Display(Name = "Размер картинки")]
		public virtual int Size {
			get { return image?.Length ?? 0; }
			set {  }
		}

		private byte[] image;
		[Display(Name = "Изображение")]
		public virtual byte[] Image {
			get => image;
			set => SetField(ref image, value);
		}

		#endregion

		public NomenclatureImage() { }
		
		public NomenclatureImage(Nomenclature nomenclature, byte[] image)
		{
			Nomenclature = nomenclature;
			Image = image;
		}
	}
}
