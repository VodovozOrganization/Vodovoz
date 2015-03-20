using System;
using System.Data.Bindings;
using QSOrmProject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Бесплатные пакеты аренды")]
	public class FreeRentPackage: IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		[Range (1, 200, ErrorMessage = "Минимальное количество воды в пакете аренды не может быть равно нулю.")]
		public virtual int MinWaterAmount { get; set; }

		[Required (ErrorMessage = "Необходимо заполнить название пакета.")]
		public virtual string Name { get; set; }

		public virtual decimal Deposit { get; set; }

		public virtual EquipmentType EquipmentType { get; set; }

		public virtual Nomenclature DepositService { get; set; }

		#endregion

		public FreeRentPackage ()
		{
			Name = String.Empty;
		}
	}

	public interface IFreeRentPackageOwner
	{
		IList<FreeRentPackage> FreeRentPackages { get; set; }
	}
}
