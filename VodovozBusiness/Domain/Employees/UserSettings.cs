using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using MoreLinq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
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
		private IList<CashSubdivisionSortingSettings> _cashSubdivisionSortingSettings;
		private GenericObservableList<CashSubdivisionSortingSettings> _observableCashSubdivisionSortingSettings;

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

		private bool reorderTabs;
		
		[Display(Name = "Перемещение вкладок")]
		public virtual bool ReorderTabs {
			get => reorderTabs;
			set => SetField(ref reorderTabs, value);
		}

		private bool highlightTabsWithColor;

		[Display(Name = "Выделение вкладок цветом")]
		public virtual bool HighlightTabsWithColor {
			get => highlightTabsWithColor;
			set => SetField(ref highlightTabsWithColor, value);
		}
		
		private bool keepTabColor;

		[Display(Name = "Сохранять цвет вкладки")]
		public virtual bool KeepTabColor {
			get => keepTabColor;
			set => SetField(ref keepTabColor, value);
		}

		Warehouse defaultWarehouse;

		[Display (Name = "Склад")]
		public virtual Warehouse DefaultWarehouse {
			get { return defaultWarehouse; }
			set {
				SetField (ref defaultWarehouse, value, () => DefaultWarehouse);
			}
		}

		NomenclatureCategory? defaultSaleCategory;

		[Display(Name = "Номенклатура на продажу")]
		public virtual NomenclatureCategory? DefaultSaleCategory {
			get { return defaultSaleCategory; }
			set { SetField(ref defaultSaleCategory, value, () => DefaultSaleCategory); }
		}

		private bool logisticDeliveryOrders;

		/// <summary>
		/// Для установки фильра заказов для обычной доставки
		/// </summary>
		[Display(Name = "Доставка")]
		public virtual bool LogisticDeliveryOrders {
			get => logisticDeliveryOrders;
			set => SetField(ref logisticDeliveryOrders, value, () => LogisticDeliveryOrders);
		}

		private bool logisticServiceOrders;

		/// <summary>
		/// Для установки фильтра заказов с сервисным обслуживанием (выезд мастеров)
		/// </summary>
		[Display(Name = "Сервисное обслуживание")]
		public virtual bool LogisticServiceOrders {
			get => logisticServiceOrders;
			set => SetField(ref logisticServiceOrders, value, () => LogisticServiceOrders);
		}

		private bool logisticChainStoreOrders;

		/// <summary>
		/// Для установки фильтра заказов для сетевых магазинов
		/// </summary>
		[Display(Name = "Сетевые магазины")]
		public virtual bool LogisticChainStoreOrders {
			get => logisticChainStoreOrders;
			set => SetField(ref logisticChainStoreOrders, value, () => LogisticChainStoreOrders);
		}

		/// <summary>
        /// Использовать отдел сотрудника
        /// </summary>

        private bool useEmployeeSubdivision;
        [Display(Name = "Использовать отдел сотрудника")]
        public virtual bool UseEmployeeSubdivision
        {
            get => useEmployeeSubdivision;
            set => SetField(ref useEmployeeSubdivision, value, () => UseEmployeeSubdivision);
        }


        /// <summary>
        /// Для установки фильтра подразделений
        /// </summary>
        private Subdivision defaultSubdivision;

        [Display(Name = "Подразделение")]
        public virtual Subdivision DefaultSubdivision
        {
            get { return defaultSubdivision; }
            set
            {
                SetField(ref defaultSubdivision, value, () => DefaultSubdivision);
            }
        }

        /// <summary>
        /// Для установки дефолтного контрагента в отчете по оплатам
        /// </summary>
        private Counterparty defaultCounterparty;
        [Display(Name = "Контрагент")]
        public virtual Counterparty DefaultCounterparty
        {
            get => defaultCounterparty;
            set => SetField(ref defaultCounterparty, value);
        }

        /// <summary>
        /// Статус рекламации
        /// </summary>
        private ComplaintStatuses? defaultComplaintStatus;

        [Display(Name = "Статус рекламации")]
        public virtual ComplaintStatuses? DefaultComplaintStatus
        {
            get { return defaultComplaintStatus; }
            set
            {
                SetField(ref defaultComplaintStatus, value, () => DefaultComplaintStatus);
            }
        }

        [Display(Name = "Настройки сортировки касс")]
        public virtual IList<CashSubdivisionSortingSettings> CashSubdivisionSortingSettings
        {
	        get => _cashSubdivisionSortingSettings;
	        set => SetField(ref _cashSubdivisionSortingSettings, value);
        }

        public virtual GenericObservableList<CashSubdivisionSortingSettings> ObservableCashSubdivisionSortingSettings =>
	        _observableCashSubdivisionSortingSettings
	        ?? (_observableCashSubdivisionSortingSettings =
		        new GenericObservableList<CashSubdivisionSortingSettings>(CashSubdivisionSortingSettings));

        #endregion

        public UserSettings ()
		{
		}

		public UserSettings (User user)
		{
			User = user;
		}

		public virtual void UpdateCashSortingIndices()
		{
			var index = 1;
			CashSubdivisionSortingSettings.ForEach(x => x.SortingIndex = index++);
		}

		/// <summary>
		/// Актуализация настроек сортировки
		/// </summary>
		/// <param name="availableSubdivisions"></param>
		/// <returns></returns>
		public virtual bool UpdateCashSortingSettings(IList<Subdivision> availableSubdivisions)
		{
			if(!CashSubdivisionSortingSettings.Any())
			{
				var index = 1;
				availableSubdivisions.ForEach(subdivision =>
					CashSubdivisionSortingSettings.Add(new CashSubdivisionSortingSettings(index++, this, subdivision)));
				return availableSubdivisions.Any();
			}

			var availableSubdivisionsIds = availableSubdivisions.Select(y => y.Id).ToList();
			var notAvailableAnymore = CashSubdivisionSortingSettings
				.Where(x => availableSubdivisionsIds.IndexOf(x.CashSubdivision.Id) == -1).ToList();
			foreach(var item in notAvailableAnymore)
			{//убираем кассы, к которым больше нет доступа
				CashSubdivisionSortingSettings.Remove(item);
			}
			if(notAvailableAnymore.Any())
			{
				UpdateCashSortingIndices();
			}

			var listedIds = CashSubdivisionSortingSettings.Select(x => x.CashSubdivision.Id).ToList();
			var notListedAsAvailable = availableSubdivisions.Where(x => listedIds.IndexOf(x.Id) == -1).ToList();
			int lastIndex = -1;
			if(CashSubdivisionSortingSettings.Any())
			{
				lastIndex = CashSubdivisionSortingSettings.Max(x => x.SortingIndex);
			}
			foreach(var item in notListedAsAvailable)
			{//добавляем кассы, к которым появился доступ
				CashSubdivisionSortingSettings.Add(new CashSubdivisionSortingSettings(++lastIndex, this, item));
			}

			return notListedAsAvailable.Any() || notAvailableAnymore.Any();
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
