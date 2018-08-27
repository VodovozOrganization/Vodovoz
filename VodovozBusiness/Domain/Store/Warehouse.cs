using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace Vodovoz.Domain.Store
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "склады",
		Nominative = "склад")]
	public class Warehouse : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название склада должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		bool canReceiveBottles;
		public virtual bool CanReceiveBottles{
			get{ return canReceiveBottles; }
			set{ SetField (ref canReceiveBottles, value, () => CanReceiveBottles); }
		}

		bool canReceiveEquipment;
		public virtual bool CanReceiveEquipment{
			get{ return canReceiveEquipment; }
			set{ SetField (ref canReceiveEquipment, value, () => CanReceiveEquipment); }
		}

		private bool publishOnlineStore;

		[Display(Name = "Публиковать в интернет магазине")]
		public virtual bool PublishOnlineStore {
			get { return publishOnlineStore; }
			set { SetField(ref publishOnlineStore, value, () => PublishOnlineStore); }
		}

		private WarehouseUsing typeOfUse;

		[Display(Name = "Тип использования")]
		public virtual WarehouseUsing TypeOfUse {
			get { return typeOfUse; }
			set { SetField(ref typeOfUse, value, () => TypeOfUse); }
		}

		private bool isArchive;

		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get { return isArchive; }
			set { SetField(ref isArchive, value, () => IsArchive); }
		}

		#endregion

		public Warehouse ()
		{
		}
	}

	public enum WarehouseUsing{
		[Display(Name = "Отгрузка")]
		Shipment,
		[Display(Name = "Производство")]
		Production,
	}

	public class WarehouseUsingStringType : NHibernate.Type.EnumStringType
	{
		public WarehouseUsingStringType() : base(typeof(WarehouseUsing))
		{
		}
	}
}