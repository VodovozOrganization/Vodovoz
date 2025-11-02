using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Warehouses
{
	/// <summary>
	/// Склад
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Accusative = "склад",
		AccusativePlural = "склады",
		Genitive = "склада",
		GenitivePlural = "складов",
		Nominative = "склад",
		NominativePlural = "склады",
		Prepositional = "складе",
		PrepositionalPlural = "складах")]
	[EntityPermission]
	[HistoryTrace]
	public class Warehouse : PropertyChangedBase, IDomainObject, IValidatableObject, INamed, IArchivable
	{
		private string _name;
		private bool _canReceiveBottles;
		private bool _canReceiveEquipment;
		private bool _publishOnlineStore;
		private WarehouseUsing _typeOfUse;
		private bool _isArchive;
		private int? _owningSubdivisionId;
		private int? _movementDocumentsNotificationsSubdivisionRecipientId;
		private string _address;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		[Required(ErrorMessage = "Название склада должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Может принимать бутылки
		/// </summary>
		public virtual bool CanReceiveBottles
		{
			get => _canReceiveBottles;
			set => SetField(ref _canReceiveBottles, value);
		}

		/// <summary>
		/// Может принимать оборудование
		/// </summary>
		public virtual bool CanReceiveEquipment
		{
			get => _canReceiveEquipment;
			set => SetField(ref _canReceiveEquipment, value);
		}

		/// <summary>
		/// Публиковать в интернет магазине
		/// </summary>
		[Display(Name = "Публиковать в интернет магазине")]
		public virtual bool PublishOnlineStore
		{
			get => _publishOnlineStore;
			set => SetField(ref _publishOnlineStore, value);
		}

		/// <summary>
		/// Тип использования
		/// </summary>
		[Display(Name = "Тип использования")]
		public virtual WarehouseUsing TypeOfUse
		{
			get => _typeOfUse;
			set => SetField(ref _typeOfUse, value);
		}

		/// <summary>
		/// Архивный склад
		/// </summary>
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		/// <summary>
		/// Адрес склада
		/// </summary>
		[Display(Name = "Адрес склада")]
		public virtual string Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}

		/// <summary>
		/// Подразделение-владелец склада
		/// </summary>
		[Display(Name = "Подразделение-владелец")]
		[HistoryIdentifier(TargetType = typeof(SubdivisionEntity))]
		public virtual int? OwningSubdivisionId
		{
			get => _owningSubdivisionId;
			set => SetField(ref _owningSubdivisionId, value);
		}

		/// <summary>
		/// Подразделение-получатель уведомлений о перемещениях на данный склад
		/// </summary>
		[Display(Name = "Подразделение-получатель уведомлений о перемещениях на данный склад")]
		[HistoryIdentifier(TargetType = typeof(SubdivisionEntity))]
		public virtual int? MovementDocumentsNotificationsSubdivisionRecipientId
		{
			get => _movementDocumentsNotificationsSubdivisionRecipientId;
			set => SetField(ref _movementDocumentsNotificationsSubdivisionRecipientId, value);
		}

		public override string ToString()
		{
			return Name;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(OwningSubdivisionId is null)
			{
				yield return new ValidationResult(
					"К складу должно быть привязано \"Подразделение-владелец\"",
					new[] { nameof(OwningSubdivisionId) });
			}
		}
	}
}
