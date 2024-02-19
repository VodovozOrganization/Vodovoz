using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Онлайн виды номенклатур",
		Nominative = "Онлайн вид номенклатуры")]
	[HistoryTrace]
	public class NomenclatureOnlineGroup : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private IList<NomenclatureOnlineCategory> _nomenclatureOnlineCategories = new List<NomenclatureOnlineCategory>();

		public virtual int Id { get; set; }
		
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "Онлайн типы")]
		public virtual IList<NomenclatureOnlineCategory> NomenclatureOnlineCategories
		{
			get => _nomenclatureOnlineCategories;
			set => SetField(ref _nomenclatureOnlineCategories, value);
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Не заполнено наименование");
			}
		}
	}
}
