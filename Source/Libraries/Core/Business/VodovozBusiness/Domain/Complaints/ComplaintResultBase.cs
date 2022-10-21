using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Complaints
{
	public abstract class ComplaintResultBase : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private bool _isArchive;
		private const int _nameLimit = 255;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name {
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "В архиве")]
		public virtual bool IsArchive {
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public abstract string Title { get; }
		public abstract ComplaintResultType ComplaintResultType { get; }

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Необходимо заполнить название", new[] { nameof(Name) });
			}
			
			if(Name != null && Name.Length > _nameLimit)
			{
				yield return new ValidationResult($"Длина названия не должна превышать {_nameLimit} символов", new[] { nameof(Name) });
			}
		}
	}
}
