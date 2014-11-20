using System;
using System.Data.Bindings;
using QSOrmProject;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Пакеты бесплатной аренды")]
	public class FreeRentPackage: IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual int MinWaterAmount { get; set; }
		public virtual string Name { get; set; }
		public virtual decimal Deposit { get; set; }
		public virtual EquipmentType EquipmentType { get; set; }
		public virtual Nomenclature DepositService { get; set; }
		#endregion

		public FreeRentPackage()
		{
			Name = String.Empty;
		}
	}
}
