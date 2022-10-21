using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "вид оформления сотрудника",
		NominativePlural = "виды оформлений сотрудников")]
	[HistoryTrace]
	public class RegistrationType : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _nameLimit = 45;
		private string _name;
		
		public virtual int Id { get; set; }

		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		public override string ToString() => Name;

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult(
					("Название вида оформления должно быть заполнено"),
					new[] {nameof(Name)});
			}
			
			if(Name != null && Name.Length > _nameLimit)
			{
				yield return new ValidationResult(
					($"Длина названия вида оформления превышено на {Name.Length - _nameLimit}"),
					new[] {nameof(Name)});
			}
		}
	}
}
