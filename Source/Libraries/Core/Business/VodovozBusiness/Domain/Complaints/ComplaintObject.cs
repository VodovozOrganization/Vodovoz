using System;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "объекты рекламаций",
		Nominative = "объект рекламаций"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintObject : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private DateTime _createDate;
		private string _name;
		private bool _isArchive;
		private DateTime? _archiveDate;
		private IList<Subdivision> _subdivisions = new List<Subdivision>();

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime CreateDate
		{
			get => _createDate;
			
			set => SetField(ref _createDate, value);
		}

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

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено.",
					new[] { nameof(ComplaintObject) });
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
