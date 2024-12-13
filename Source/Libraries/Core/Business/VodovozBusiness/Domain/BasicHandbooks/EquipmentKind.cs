using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "виды оборудования",
		Nominative = "вид оборудования")]
	[EntityPermission]
	public class EquipmentKind : PropertyChangedBase, IDomainObject
	{
		private EquipmentType _equipmentType;

		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		WarrantyCardType warrantyCardType;
		[Display (Name="Тип гарантийного талона")]
		public virtual WarrantyCardType WarrantyCardType{
			get{ return warrantyCardType; }
			set{ SetField (ref warrantyCardType, value, () => WarrantyCardType); }
		}

		[Display(Name = "Тип оборудования")]
		public virtual EquipmentType EquipmentType
		{
			get { return _equipmentType; }
			set { SetField(ref _equipmentType, value); }
		}
		#endregion

		public EquipmentKind ()
		{
			Name = String.Empty;
		}
	}

	public enum WarrantyCardType{
		[Display(Name="Нет")]
		WithoutCard,
		[Display (Name = "Гарrантийный талон на кулера")]
		CoolerWarranty,
		[Display (Name = "Гарантийный талон на помпы")]
		PumpWarranty
	}

	public enum EquipmentType
	{
		[Display(Name = "Кулер")]
		Cooler,
		[Display(Name = "Помпа")]
		Pump,
		[Display(Name = "Прочее")]
		Other
	}
}
