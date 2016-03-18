using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NHibernate;
using NHibernate.Criterion;
using QSOrmProject;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "оборудование",
		Nominative = "оборудование",
		Prepositional = "оборудовании"
	)]
	public class Equipment: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		bool onDuty;

		[Display (Name = "Дежурный куллер")]
		public virtual bool OnDuty {
			get { return onDuty; }
			set { SetField (ref onDuty, value, () => OnDuty); }
		}

		//string serial;

		[Display (Name = "Серийный номер")]
		public virtual string Serial {
			get { return Id > 0 ? Id.ToString () : "не определён"; }
			//set { SetField (ref serial, value, () => Serial); }
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		DateTime lastServiceDate;

		[Display (Name = "Последняя сан. обработка")]
		public virtual DateTime LastServiceDate {
			get { return lastServiceDate; }
			set { SetField (ref lastServiceDate, value, () => LastServiceDate); }
		}

		DateTime? warrantyEndDate;

		[Display (Name = "Окончание гарантии")]
		public virtual DateTime? WarrantyEndDate {
			get { return warrantyEndDate; }
			set { SetField (ref warrantyEndDate, value, () => WarrantyEndDate); }
		}

		#endregion

		public virtual DateTime? NextServiceDate {
			get{ 
				if (LastServiceDate == DateTime.MinValue)
					return null;
				return LastServiceDate.AddMonths(6);
			}
		}

		public virtual string Title { 
			get { return Nomenclature == null ? String.Empty : String.Format ("{0} (с/н: {1})", Nomenclature.Model, Serial); } 
		}

		public virtual string NomenclatureName { get { return Nomenclature == null ? String.Empty : Nomenclature.Name; } }

		public Equipment ()
		{
			Comment = String.Empty;
		}

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (LastServiceDate > DateTime.Now)
				yield return new ValidationResult ("Дата последней санитарной обработки не может быть в будущем.");
			if (Serial == String.Empty)
				yield return new ValidationResult ("Серийный номер должен быть заполнен.");
			if (Nomenclature == null)
				yield return new ValidationResult ("Должна быть указана номенклатура.");
		}

		#endregion
	}

	public static class EquipmentWorks
	{
		public static ICriterion FilterUsedEquipment (ISession session)
		{
			var fAgreements = session.CreateCriteria<FreeRentAgreement> ().List<FreeRentAgreement> ();
			var nAgreements = session.CreateCriteria<NonfreeRentAgreement> ().List<NonfreeRentAgreement> ();
			var dAgreements = session.CreateCriteria<DailyRentAgreement> ().List<DailyRentAgreement> ();
			var IDs = new List<int> ();
			foreach (FreeRentAgreement fr in fAgreements)
				foreach (FreeRentEquipment eq in fr.Equipment)
					if (eq.Equipment != null)
						IDs.Add (eq.Equipment.Id);
			foreach (NonfreeRentAgreement nfr in nAgreements)
				foreach (PaidRentEquipment eq in nfr.Equipment)
					if (eq.Equipment != null)
						IDs.Add (eq.Equipment.Id);
			foreach (DailyRentAgreement dr in dAgreements)
				foreach (PaidRentEquipment eq in dr.Equipment)
					if (eq.Equipment != null)
						IDs.Add (eq.Equipment.Id);
			int[] arr = new int[IDs.Count];
			IDs.CopyTo (arr, 0);

			return Restrictions.Not (Restrictions.In ("Id", arr)); 
		}
	}
}

