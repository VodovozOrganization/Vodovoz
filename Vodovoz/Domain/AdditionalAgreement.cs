using System;
using QSOrmProject;
using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Дополнительные соглашения")]
	public class AdditionalAgreement : IDomainObject
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

		public virtual DateTime IssueDate { get; set; }

		public virtual DateTime StartDate { get; set; }

		public virtual DeliveryPoint DeliveryPoint { get; set; }

		#endregion

		public AdditionalAgreement ()
		{
			AgreementNumber = String.Empty;
		}

		public virtual string AgreementTypeTitle { get { return Type.GetEnumTitle (); } }

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

