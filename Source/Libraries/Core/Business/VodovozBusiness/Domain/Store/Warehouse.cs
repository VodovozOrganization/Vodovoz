using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Store
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "склады",
		Nominative = "склад")]
	[EntityPermission]
	[HistoryTrace]
	public class Warehouse : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private Subdivision _movementDocumentsNotificationsSubdivisionRecipient;
		private string _name;
		private bool _canReceiveBottles;
		private bool _canReceiveEquipment;
		private bool _publishOnlineStore;
		private WarehouseUsing _typeOfUse;
		private bool _isArchive;
		private Subdivision _owningSubdivision;

		#region Свойства

		public virtual int Id { get; set; }

		[Required(ErrorMessage = "Название склада должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		public virtual bool CanReceiveBottles
		{
			get => _canReceiveBottles;
			set => SetField(ref _canReceiveBottles, value);
		}

		public virtual bool CanReceiveEquipment
		{
			get => _canReceiveEquipment;
			set => SetField(ref _canReceiveEquipment, value);
		}

		[Display(Name = "Публиковать в интернет магазине")]
		public virtual bool PublishOnlineStore
		{
			get => _publishOnlineStore;
			set => SetField(ref _publishOnlineStore, value);
		}

		[Display(Name = "Тип использования")]
		public virtual WarehouseUsing TypeOfUse
		{
			get => _typeOfUse;
			set => SetField(ref _typeOfUse, value);
		}

		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Подразделение-владелец")]
		public virtual Subdivision OwningSubdivision
		{
			get => _owningSubdivision;
			set => SetField(ref _owningSubdivision, value);
		}

		[Display(Name = "Подразделение-получатель уведомлений о перемещениях на данный склад")]
		public virtual Subdivision MovementDocumentsNotificationsSubdivisionRecipient
		{
			get => _movementDocumentsNotificationsSubdivisionRecipient;
			set => SetField(ref _movementDocumentsNotificationsSubdivisionRecipient, value);
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(OwningSubdivision == null)
			{
				yield return new ValidationResult(
					"К складу должно быть привязано \"Подразделение-владелец\"",
					new[] { nameof(OwningSubdivision) }
				);
			}
		}

		#endregion
	}
}
