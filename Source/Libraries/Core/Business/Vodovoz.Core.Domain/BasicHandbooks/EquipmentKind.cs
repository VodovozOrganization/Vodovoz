using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.BasicHandbooks
{
	[Appellative(Gender = GrammaticalGender.Masculine,
	NominativePlural = "виды оборудования",
	Nominative = "вид оборудования")]
	[EntityPermission]
	public class EquipmentKind : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _name;
		private EquipmentType _equipmentType;
		private WarrantyCardType _warrantyCardType;

		#region Свойства

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value, () => Id);
		}

		/// <summary>
		/// Название
		/// </summary>
		[Required(ErrorMessage = "Название должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value, () => Name);
		}

		/// <summary>
		/// Тип оборудования
		/// </summary>
		[Display(Name = "Тип оборудования")]
		public virtual EquipmentType EquipmentType
		{
			get => _equipmentType;
			set => SetField(ref _equipmentType, value, () => EquipmentType);
		}

		/// <summary>
		/// Тип гарантийного талона
		/// </summary>
		[Display(Name = "Тип гарантийного талона")]
		public virtual WarrantyCardType WarrantyCardType
		{
			get => _warrantyCardType;
			set => SetField(ref _warrantyCardType, value, () => WarrantyCardType);
		}

		#endregion

		public EquipmentKind()
		{
			Name = string.Empty;
		}
	}
}
