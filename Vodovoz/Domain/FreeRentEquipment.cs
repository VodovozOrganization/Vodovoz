using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Оборудование для бесплатной аренды")]
	public class FreeRentEquipment : IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual FreeRentPackage FreeRentPackage { get; set; }

		public virtual Equipment Equipment { get; set; }

		public virtual Decimal Deposit { get; set; }

		public virtual int WaterAmount { get; set; }

		public virtual string PackageName { get { return FreeRentPackage != null ? FreeRentPackage.Name : ""; } }

		public virtual string EquipmentName { get { return Equipment != null ? Equipment.NomenclatureName : ""; } }

		public virtual string DepositString { get { return String.Format ("{0} р.", Deposit); } }

		public virtual string WaterAmountString { get { return String.Format ("{0} бутылей", WaterAmount); } }

		public FreeRentEquipment ()
		{
		}
	}
}

