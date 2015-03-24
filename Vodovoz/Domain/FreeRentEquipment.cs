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

		public FreeRentEquipment ()
		{
		}
	}
}

