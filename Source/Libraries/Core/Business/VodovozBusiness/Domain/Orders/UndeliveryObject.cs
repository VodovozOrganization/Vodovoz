using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "объекты недовоза",
		Nominative = "объект недовоза"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class UndeliveryObject : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private DateTime _createDate;
		private string _name;
		private bool _isArchive;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Название ")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Архив")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public virtual string GetFullName => !IsArchive ? Name : $"(Архив) {Name}";

		public virtual string Title => Name;

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено.",
					new[] { nameof(UndeliveryObject) });
			}

			if(Name?.Length > 100)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина названия ({Name.Length}/100).",
					new[] { nameof(Name) });
			}
		}

		#endregion
	}
}
