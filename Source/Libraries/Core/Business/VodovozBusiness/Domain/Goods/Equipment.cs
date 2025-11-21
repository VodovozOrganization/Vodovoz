using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Goods
{
	/// <summary>
	/// Оборудование только для посерийного учета
	/// </summary>
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "оборудование",
		Nominative = "оборудование",
		Prepositional = "оборудовании"
	)]
	[EntityPermission]
	public class Equipment: EquipmentEntity, IValidatableObject
	{
		private Nomenclature _nomenclature;
		private Counterparty _assignedToClient;

		#region Свойства

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display (Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature
		{
			get { return _nomenclature; }
			set { SetField (ref _nomenclature, value, () => Nomenclature); }
		}

		/// <summary>
		/// Привязан к клиенту
		/// </summary>
		[Display (Name = "Привязан к клиенту")]
		public virtual new Counterparty AssignedToClient
		{
			get { return _assignedToClient; }
			set {
				SetField (ref _assignedToClient, value, () => AssignedToClient); 
			}
		}

		#endregion

		public Equipment ()
		{
			Comment = string.Empty;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(LastServiceDate > DateTime.Now)
			{
				yield return new ValidationResult ("Дата последней санитарной обработки не может быть в будущем.");
			}

			if(Nomenclature == null)
			{
				yield return new ValidationResult ("Должна быть указана номенклатура.");
			}
		}

		#endregion
	}
}

