using System;
using System.Data.Bindings;
using QSOrmProject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Бесплатные пакеты аренды")]
	public class FreeRentPackage: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		int minWaterAmount;

		[Range (1, 200, ErrorMessage = "Минимальное количество воды в пакете аренды не может быть равно нулю.")]
		public virtual int MinWaterAmount {
			get { return minWaterAmount; }
			set { SetField (ref minWaterAmount, value, () => MinWaterAmount); }
		}

		string name;

		[Required (ErrorMessage = "Необходимо заполнить название пакета.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		decimal deposit;

		public virtual decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		EquipmentType equipmentType;

		public virtual EquipmentType EquipmentType {
			get { return equipmentType; }
			set { SetField (ref equipmentType, value, () => EquipmentType); }
		}

		Nomenclature depositService;

		public virtual Nomenclature DepositService {
			get { return depositService; }
			set { SetField (ref depositService, value, () => DepositService); }
		}

		#endregion

		public FreeRentPackage ()
		{
			Name = String.Empty;
		}
	}

	public interface IFreeRentEquipmentOwner
	{
		IList<FreeRentEquipment> Equipment { get; set; }
	}
}
