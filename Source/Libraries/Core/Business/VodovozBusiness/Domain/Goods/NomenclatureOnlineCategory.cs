using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Онлайн типы номенклатур",
		Nominative = "Онлайн тип номенклатуры")]
	[HistoryTrace]
	public class NomenclatureOnlineCategory : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private NomenclatureOnlineGroup _nomenclatureOnlineGroup;
		
		public virtual int Id { get; set; }
		
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Онлайн группа")]
		public virtual NomenclatureOnlineGroup NomenclatureOnlineGroup
		{
			get => _nomenclatureOnlineGroup;
			set => SetField(ref _nomenclatureOnlineGroup, value);
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Не заполнено наименование");
			}
			
			if(NomenclatureOnlineGroup is null)
			{
				yield return new ValidationResult("Не выбрана онлайн группа");
			}
		}
	}
}
