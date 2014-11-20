using System;
using System.Data.Bindings;
using QSOrmProject;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Пакеты платной аренды")]
	public class PaidRentPackage: IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual int RentPeriod { get; set; }
		public virtual string Name { get; set; }
		public virtual decimal Price { get; set; }
		public virtual bool Daily { get; set; }
		public virtual Nomenclature DepositService { get; set; }
		public virtual Nomenclature RentService { get; set; }
		#endregion

		public string RentPeriodString { 
			get {
				if (Daily)
					return "Посуточно";
				else
					return String.Format("{0} мес.", RentPeriod);
			} 
		}

		public PaidRentPackage()
		{
			Name = String.Empty;
		}
	}
}
