using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{

	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "колонки в маршрутном листе",
		Nominative = "колонка маршрутного листа")]
	[HistoryTrace]
	public class RouteColumn : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private string _shortName;
		private bool _isHighlighted;
		public virtual int Id { get; set; }

		[Display (Name = "Название")]
		public virtual string Name {
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Короткое название")]
		public virtual string ShortName
		{
			get => _shortName;
			set => SetField(ref _shortName, value);
		}

		[Display(Name = "Ячейка выделена")]
		public virtual bool IsHighlighted
		{
			get => _isHighlighted;
			set => SetField(ref _isHighlighted, value);
		}

		public virtual string Title => Name;

		public RouteColumn ()
		{
			Name = string.Empty;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Название номенклатуры должно быть заполнено.", new[] { nameof(Name) });
			}

			if(!string.IsNullOrEmpty(Name) && Name.Length > 20)
			{
				yield return new ValidationResult("Название не должно быть длиннее 20 символов", new[] { nameof(Name) });
			}

			if(!string.IsNullOrEmpty(ShortName) && ShortName.Length > 3)
			{
				yield return new ValidationResult("Короткое название не должно быть длиннее 3 символов", new[] { nameof(ShortName) });
			}
		}
	}
}

