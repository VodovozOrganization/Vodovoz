using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Причины закрытия заявок на звонок",
		Nominative = "Причина закрытия заявки на звонок",
		Prepositional = "Причине закрытия заявки на звонок",
		PrepositionalPlural = "Причинах закрытия заявок на звонок"
	)]
	[EntityPermission]
	[HistoryTrace]
	public class RequestForCallClosedReason : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _nameMaxLength = 150;
		private string _name;
		private bool _isArchive;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "В архиве?")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public override string ToString()
		{
			var entityName =
				typeof(RequestForCallClosedReason)
					.GetCustomAttribute<AppellativeAttribute>(true)
					.Nominative;
			
			return Id > 0 ? $"{entityName} №{Id}" : $"Новая {entityName.ToLower()}";
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название причины не может быть пустым");
			}
			
			if(!string.IsNullOrWhiteSpace(Name) && Name.Length > _nameMaxLength)
			{
				yield return new ValidationResult($"Длина названия причины превышена на {Name.Length - _nameMaxLength}");
			}
		}
	}
}
