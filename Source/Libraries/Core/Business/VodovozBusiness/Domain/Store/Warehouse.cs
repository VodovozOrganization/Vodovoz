using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Domain.Store
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "склады",
		Nominative = "склад")]
	[EntityPermission]
	[HistoryTrace]
	public class Warehouse : WarehouseEntity
	{
		private Subdivision _owningSubdivision;
		private Subdivision _movementDocumentsNotificationsSubdivisionRecipient;

		#region Свойства

		[Display(Name = "Подразделение-владелец")]
		public virtual new Subdivision OwningSubdivision
		{
			get => _owningSubdivision;
			set => SetField(ref _owningSubdivision, value);
		}

		[Display(Name = "Подразделение-получатель уведомлений о перемещениях на данный склад")]
		public virtual new Subdivision MovementDocumentsNotificationsSubdivisionRecipient
		{
			get => _movementDocumentsNotificationsSubdivisionRecipient;
			set => SetField(ref _movementDocumentsNotificationsSubdivisionRecipient, value);
		}

		#endregion

		#region IValidatableObject implementation

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
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
