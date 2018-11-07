using System;
using System.ComponentModel.DataAnnotations;
using Gtk;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Employees
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "настройки пользователей",
		Nominative = "настройки пользователя")]
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

		[Display (Name = "Дней в фильтре журнала заказов назад")]
		public virtual int JournalDaysToAft {
			get { return journalDaysToAft; }
			set {
				SetField (ref journalDaysToAft, value, () => JournalDaysToAft);
			}
		}

		int journalDaysToFwd;

		[Display(Name = "Дней в фильтре журнала заказов вперёд")]
		public virtual int JournalDaysToFwd {
			get { return journalDaysToFwd; }
			set {
				SetField(ref journalDaysToFwd, value, () => JournalDaysToFwd);
			}
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

	public class ToolbarStyleStringType : NHibernate.Type.EnumStringType
	{
		public ToolbarStyleStringType () : base (typeof(ToolbarStyle))
		{
		}
	}

}

