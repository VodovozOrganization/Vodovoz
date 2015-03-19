using System;
using QSOrmProject;
using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubject ("Дополнительные соглашения")]
	public class AdditionalAgreement : IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		[Required (ErrorMessage = "Номер доп. соглашения должен быть заполнен.")]
		public virtual string AgreementNumber { get; set; }

		public virtual AgreementType Type {
			get {
				if (this is NonfreeRentAgreement)
					return AgreementType.NonfreeRent;
				if (this is FreeRentAgreement)
					return AgreementType.FreeRent;
				if (this is WaterSalesAgreement)
					return AgreementType.WaterSales;
				if (this is RepairAgreement)
					return AgreementType.Repair;
				return AgreementType.EquipmentSales;
			}

		}

		public virtual CounterpartyContract Contract { get; set; }

		public virtual DateTime IssueDate { get; set; }

		public virtual DateTime StartDate { get; set; }

		public virtual DeliveryPoint DeliveryPoint { get; set; }

		public virtual string AgreementDeliveryPoint { get { return DeliveryPoint != null ? DeliveryPoint.Point : "Не указана"; } }

		public virtual string AgreementTypeTitle { get { return Type.GetEnumTitle (); } }

		public virtual bool IsNew { get; set; }

		#endregion

		public AdditionalAgreement ()
		{
			AgreementNumber = String.Empty;
		}


		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			int count = 0;
			foreach (AdditionalAgreement agreement in Contract.AdditionalAgreements)
				if (agreement.AgreementNumber == this.AgreementNumber)
					count++;
			if (count > 1)
				yield return new ValidationResult ("Доп. соглашение с таким номером уже существует.", new[] { "AgreementNumber" });
		}

		#endregion
	}

	public class NonfreeRentAgreement : AdditionalAgreement
	{
		#region Свойства

		public virtual PaidRentPackage RentPackage { get; set; }

		#endregion
	}

	public class FreeRentAgreement : AdditionalAgreement
	{
		#region Свойства

		public virtual FreeRentPackage RentPackage { get; set; }

		#endregion
	}

	public class WaterSalesAgreement : AdditionalAgreement
	{
		#region Свойства

		public virtual bool IsFixedPrice { get; set; }

		public virtual decimal FixedPrice { get; set; }

		public override IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			foreach (ValidationResult result in base.Validate (validationContext))
				yield return result;
			var agreements = new List<AdditionalAgreement> ();
			foreach (AdditionalAgreement agreement in Contract.AdditionalAgreements)
				if (agreement is WaterSalesAgreement)
					agreements.Add (agreement);
			if (agreements.FindAll (m => m.DeliveryPoint == this.DeliveryPoint).Count > 1) {
				if (DeliveryPoint != null)
					yield return new ValidationResult ("Доп. соглашение для данной точки доставки уже существует.", new[] { "DeliveryPoint" });
				else
					yield return new ValidationResult ("Общее доп. соглашение по продаже воды уже существует. " +
					"Пожалуйста, укажите точку доставки или перейдите к существующему соглашению.", new[] { "DeliveryPoint" });
			}
		}

		#endregion
	}

	public class RepairAgreement : AdditionalAgreement
	{
		#region Свойства

		#endregion
	}

	public enum AgreementType
	{
		[ItemTitleAttribute ("Платная аренда")]
		NonfreeRent,
		[ItemTitleAttribute ("Бесплатная аренда")]
		FreeRent,
		[ItemTitleAttribute ("Продажа воды")]
		WaterSales,
		[ItemTitleAttribute ("Продажа оборудования")]
		EquipmentSales,
		[ItemTitleAttribute ("Ремонт")]
		Repair
	}

	public class AgreementTypeStringType : NHibernate.Type.EnumStringType
	{
		public AgreementTypeStringType () : base (typeof(AgreementType))
		{
		}
	}
}

