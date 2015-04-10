using System;
using System.Data.Bindings;
using QSOrmProject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Пакеты платной аренды")]
	public class PaidRentPackage: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Необходимо заполнить название пакета платной аренды.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		decimal priceDaily;

		public virtual decimal PriceDaily {
			get { return priceDaily; }
			set { SetField (ref priceDaily, value, () => PriceDaily); }
		}

		Nomenclature rentServiceDaily;

		public virtual Nomenclature RentServiceDaily {
			get { return rentServiceDaily; }
			set { SetField (ref rentServiceDaily, value, () => RentServiceDaily); }
		}

		decimal priceMonthly;

		public virtual decimal PriceMonthly {
			get { return priceMonthly; }
			set { SetField (ref priceMonthly, value, () => PriceMonthly); }
		}

		Nomenclature rentServiceMonthly;

		public virtual Nomenclature RentServiceMonthly {
			get { return rentServiceMonthly; }
			set { SetField (ref rentServiceMonthly, value, () => RentServiceMonthly); }
		}

		EquipmentType equipmentType;

		public virtual EquipmentType EquipmentType {
			get { return equipmentType; }
			set { SetField (ref equipmentType, value, () => EquipmentType); }
		}

		decimal deposit;

		public virtual decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		Nomenclature depositService;

		public virtual Nomenclature DepositService {
			get { return depositService; }
			set { SetField (ref depositService, value, () => DepositService); }
		}

		#endregion

		public PaidRentPackage ()
		{
			Name = String.Empty;
		}

		public virtual string PriceDailyString { get { return String.Format ("{0} руб.", PriceDaily); } }

		public virtual string PriceMonthlyString { get { return String.Format ("{0} руб.", PriceMonthly); } }


	}

	public interface IPaidRentEquipmentOwner
	{
		IList<PaidRentEquipment> Equipment { get; set; }
	}
}
