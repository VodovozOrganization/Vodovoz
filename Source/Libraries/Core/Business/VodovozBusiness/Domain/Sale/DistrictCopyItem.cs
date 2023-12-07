using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "записи о наличии копий районов",
		Nominative = "запись о наличии копии района")]
	public class DistrictCopyItem : PropertyChangedBase, IDomainObject
	{
		private District _district;
		private District _copiedToDistrict;
		public virtual int Id { get; set; }

		[Display(Name = "Район, который был скопирован")]
		public virtual District District
		{ 
			get => _district; 
			set => SetField(ref _district, value);
		}

		[Display(Name = "Копия района")]
		public virtual District CopiedToDistrict
		{
			get => _copiedToDistrict;
			set => SetField(ref _copiedToDistrict, value);
		}

		public virtual string Title => $"Район {District?.Title}({District?.Id}) скопирован в {CopiedToDistrict?.Title}({CopiedToDistrict?.Id})";
	}
}
