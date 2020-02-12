using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Employees
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "настройки пользователей",
		Nominative = "настройки пользователя")]
	[EntityPermission]
	public class UserSettings: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		User user;

		[Display (Name = "Пользователь")]
		public virtual User User {
			get { return user; }
			set { SetField (ref user, value, () => User); }
		}

		ToolbarStyle toolbarStyle = ToolbarStyle.Both;

		[Display (Name = "Стиль панели")]
		public virtual ToolbarStyle ToolbarStyle {
			get { return toolbarStyle; }
			set { SetField (ref toolbarStyle, value, () => ToolbarStyle); }
		}

		IconsSize toolBarIconsSize = IconsSize.Large;

		[Display (Name = "Размер иконок панели")]
		public virtual IconsSize ToolBarIconsSize {
			get { return toolBarIconsSize; }
			set { SetField (ref toolBarIconsSize, value, () => ToolBarIconsSize); }
		}

		Warehouse defaultWarehouse;

		[Display (Name = "Склад")]
		public virtual Warehouse DefaultWarehouse {
			get { return defaultWarehouse; }
			set {
				SetField (ref defaultWarehouse, value, () => DefaultWarehouse);
			}
		}

		int journalDaysToAft;
		[Obsolete("Нужно выпиливать. Было сделано для запоминания интервала дат в фильтре. Потеряло актульность с введением журнала с динамической подгрузкой.")]
		[Display (Name = "Дней в фильтре журнала заказов назад")]
		public virtual int JournalDaysToAft {
			get { return journalDaysToAft; }
			set {
				SetField (ref journalDaysToAft, value, () => JournalDaysToAft);
			}
		}

		int journalDaysToFwd;

		[Obsolete("Нужно выпиливать. Было сделано для запоминания интервала дат в фильтре. Потеряло актульность с введением журнала с динамической подгрузкой.")]
		[Display(Name = "Дней в фильтре журнала заказов вперёд")]
		public virtual int JournalDaysToFwd {
			get { return journalDaysToFwd; }
			set {
				SetField(ref journalDaysToFwd, value, () => JournalDaysToFwd);
			}
		}

		NomenclatureCategory? defaultSaleCategory;

		[Display(Name = "Номенклатура на продажу")]
		public virtual NomenclatureCategory? DefaultSaleCategory {
			get { return defaultSaleCategory; }
			set { SetField(ref defaultSaleCategory, value, () => DefaultSaleCategory); }
		}

		#endregion

		public UserSettings ()
		{
		}

		public UserSettings (User user)
		{
			User = user;
		}
	}

	public enum IconsSize
	{
		ExtraSmall,
		Small,
		Middle,
		Large
	}

	public class ToolBarIconsSizeStringType : NHibernate.Type.EnumStringType
	{
		public ToolBarIconsSizeStringType () : base (typeof(IconsSize))
		{
		}
	}

	public enum ToolbarStyle
	{
		Icons,
		Text,
		Both,
		BothHoriz
	}

	public class ToolbarStyleStringType : NHibernate.Type.EnumStringType
	{
		public ToolbarStyleStringType () : base (typeof(ToolbarStyle))
		{
		}
	}

}

