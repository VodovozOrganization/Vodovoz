using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
			NominativePlural = "Причины отмены онлайн заказа",
			Nominative = "Причина отмены онлайн заказа",
			Prepositional = "Причине отмены онлайн заказа",
			PrepositionalPlural = "Причинах отмены онлайн заказа"
		)
	]
	[EntityPermission]
	[HistoryTrace]
	public class OnlineOrderCancellationReason : PropertyChangedBase, IDomainObject, IValidatableObject
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

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!string.IsNullOrWhiteSpace(Name) && Name.Length > _nameMaxLength)
			{
				yield return new ValidationResult($"Длина названия причины превышена на {_nameMaxLength - Name.Length}");
			}
		}
	}
}
