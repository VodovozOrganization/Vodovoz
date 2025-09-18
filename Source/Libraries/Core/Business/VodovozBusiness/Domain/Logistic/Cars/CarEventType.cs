using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "виды событий ТС",
		Nominative = "вид события ТС")]
	[EntityPermission]
	[HistoryTrace]

	public partial class CarEventType : PropertyChangedBase, IDomainObject, IValidatableObject, IArchivable, INamedDomainObject
	{
		private string _name;
		private string _shortName;
		private bool _needComment;
		private bool _isArchive;
		private bool _isDoNotShowInOperation;
		private bool _isAttachWriteOffDocument;
		private AreaOfResponsibility? _areaOfResponsibility;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Название ")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Сокращенное название")]
		public virtual string ShortName
		{
			get => _shortName;
			set => SetField(ref _shortName, value);
		}

		[Display(Name = "Обязательность комментария")]
		public virtual bool NeedComment
		{
			get => _needComment;
			set => SetField(ref _needComment, value);
		}

		[Display(Name = "Архив")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Не отображать в эксплуатации ТС")]
		public virtual bool IsDoNotShowInOperation
		{
			get => _isDoNotShowInOperation;
			set => SetField(ref _isDoNotShowInOperation, value);
		}

		[Display(Name = "Прикреплять акт списания")]
		public virtual bool IsAttachWriteOffDocument
		{
			get => _isAttachWriteOffDocument;
			set => SetField(ref _isAttachWriteOffDocument, value);
		}

		[Display(Name = "Зона ответственности отдела")]
		public virtual AreaOfResponsibility? AreaOfResponsibility
		{
			get => _areaOfResponsibility;
			set => SetField(ref _areaOfResponsibility, value);
		}
		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено.",
					new[] { nameof(CarEventType) });
			}

			if(string.IsNullOrEmpty(ShortName))
			{
				yield return new ValidationResult("Сокращённое название должно быть заполнено.",
					new[] { nameof(CarEventType) });
			}

			if(Name?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина названия ({Name.Length}/255).",
					new[] { nameof(Name) });
			}

			if(ShortName?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина сокращённого названия ({ShortName.Length}/255).",
					new[] { nameof(ShortName) });
			}

			if(AreaOfResponsibility == null)
			{
				yield return new ValidationResult($"Зона ответственности должна быть заполнена.",
					new[] { nameof(AreaOfResponsibility) });
			}
		}

		#endregion
	}
}
