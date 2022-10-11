using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "причины отсутствия переноса",
		Nominative = "причина отсутствия переноса")]
	[EntityPermission]
	[HistoryTrace]

	public class UndeliveryTransferAbsenceReason : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private DateTime _createDate;
		private string _name;
		private bool _isArchive;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		[Display(Name = "Причина")]
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
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название должно быть указано.", new[] { nameof(Name) });
			}

			if(Name?.Length > 50)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина названия ({Name.Length}/50).",
					new[] { nameof(Name) });
			}
		}

		#endregion
	}
}
