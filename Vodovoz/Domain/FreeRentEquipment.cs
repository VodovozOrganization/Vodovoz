using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Оборудование для бесплатной аренды")]
	public class FreeRentEquipment : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual FreeRentPackage FreeRentPackage { get; set; }

		public virtual Equipment Equipment { get; set; }

		Decimal deposit;

		public virtual Decimal Deposit {
			get {
				return deposit;
			}
			set {
				SetField (ref deposit, value, () => Deposit);
			}
		}

		int waterAmount;

		public virtual int WaterAmount {
			get {
				return waterAmount;
			}
			set {
				SetField (ref waterAmount, value, () => WaterAmount);
			}
		}

		public virtual string PackageName { get { return FreeRentPackage != null ? FreeRentPackage.Name : ""; } }

		public virtual string EquipmentName { get { return Equipment != null ? Equipment.NomenclatureName : ""; } }

		public virtual string DepositString { get { return String.Format ("{0} р.", Deposit); } }

		public virtual string WaterAmountString { get { return String.Format ("{0} бутылей", WaterAmount); } }

		public FreeRentEquipment ()
		{
		}
	}
}

