using System;
using System.Data.Bindings;
using QSOrmProject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Пакеты бесплатной аренды", ObjectName = "пакет бесплатной аренды")]
	public class FreeRentPackage: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		int minWaterAmount;

		[Display(Name = "Минимальное количество")]
		[Range (1, 200, ErrorMessage = "Минимальное количество воды в пакете аренды не может быть равно нулю.")]
		public virtual int MinWaterAmount {
			get { return minWaterAmount; }
			set { SetField (ref minWaterAmount, value, () => MinWaterAmount); }
		}

		string name;

		[Display(Name = "Название")]
		[Required (ErrorMessage = "Необходимо заполнить название пакета.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		decimal deposit;

		[Display(Name = "Залог")]
		public virtual decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		EquipmentType equipmentType;

		[Display(Name = "Тип оборудования")]
		public virtual EquipmentType EquipmentType {
			get { return equipmentType; }
			set { SetField (ref equipmentType, value, () => EquipmentType); }
		}

		Nomenclature depositService;
		[Display(Name = "Услуга залога")]
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
