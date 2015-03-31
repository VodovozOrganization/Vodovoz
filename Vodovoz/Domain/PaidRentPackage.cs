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

		int rentPeriod;

		public virtual int RentPeriod {
			get { return rentPeriod; }
			set { SetField (ref rentPeriod, value, () => RentPeriod); }
		}

		string name;

		[Required (ErrorMessage = "Необходимо заполнить название пакета платной аренды.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		decimal price;

		public virtual decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}

		bool daily;

		public virtual bool Daily {
			get { return daily; }
			set { SetField (ref daily, value, () => Daily); }
		}

		Nomenclature depositService;

		public virtual Nomenclature DepositService {
			get { return depositService; }
			set { SetField (ref depositService, value, () => DepositService); }
		}

		Nomenclature rentService;

		public virtual Nomenclature RentService {
			get { return rentService; }
			set { SetField (ref rentService, value, () => RentService); }
		}

		#endregion

		public string RentPeriodString { 
			get {
				if (Daily)
					return "Посуточно";
				else
					return String.Format ("{0} мес.", RentPeriod);
			} 
		}

		public PaidRentPackage ()
		{
			Name = String.Empty;
		}
	}

	public interface IPaidRentEquipmentOwner
	{
		IList<PaidRentEquipment> Equipment { get; set; }
	}
}
