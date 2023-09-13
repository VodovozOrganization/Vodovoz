using MoreLinq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "настройки пользователей",
		Nominative = "настройки пользователя")]
	[EntityPermission]
	public class UserSettings : PropertyChangedBase, IDomainObject
	{
		private IList<CashSubdivisionSortingSettings> _cashSubdivisionSortingSettings;
		private GenericObservableList<CashSubdivisionSortingSettings> _observableCashSubdivisionSortingSettings;
		private string _movementDocumentsNotificationUserSelectedWarehousesString;

		public UserSettings()
		{
		}

		public UserSettings(User user)
		{
			User = user;
		}

		#region Свойства

		public virtual int Id { get; set; }

		private User _user;

		[Display(Name = "Пользователь")]
		public virtual User User
		{
			get => _user;
			set => SetField(ref _user, value);
		}

		/// <summary>
		/// Тема оформления
		/// </summary>
		public virtual string ThemeName
		{
			get => _themeName;
			set => SetField(ref _themeName, value);
		}

		private ToolbarStyle _toolbarStyle = ToolbarStyle.Both;

		[Display(Name = "Стиль панели")]
		public virtual ToolbarStyle ToolbarStyle
		{
			get => _toolbarStyle;
			set => SetField(ref _toolbarStyle, value);
		}

		private IconsSize _toolBarIconsSize = IconsSize.Large;

		[Display(Name = "Размер иконок панели")]
		public virtual IconsSize ToolBarIconsSize
		{
			get => _toolBarIconsSize;
			set => SetField(ref _toolBarIconsSize, value);
		}

		private bool _reorderTabs;

		[Display(Name = "Перемещение вкладок")]
		public virtual bool ReorderTabs
		{
			get => _reorderTabs;
			set => SetField(ref _reorderTabs, value);
		}

		private bool _highlightTabsWithColor;

		[Display(Name = "Выделение вкладок цветом")]
		public virtual bool HighlightTabsWithColor
		{
			get => _highlightTabsWithColor;
			set => SetField(ref _highlightTabsWithColor, value);
		}

		private bool _keepTabColor;

		[Display(Name = "Сохранять цвет вкладки")]
		public virtual bool KeepTabColor
		{
			get => _keepTabColor;
			set => SetField(ref _keepTabColor, value);
		}

		private bool _hideComplaintNotification;

		[Display(Name = "Скрыть уведомления об открытых рекламациях")]
		public virtual bool HideComplaintNotification
		{
			get => _hideComplaintNotification;
			set => SetField(ref _hideComplaintNotification, value);
		}

		private Warehouse _defaultWarehouse;

		[Display(Name = "Склад")]
		public virtual Warehouse DefaultWarehouse
		{
			get => _defaultWarehouse;
			set => SetField(ref _defaultWarehouse, value);
		}

		private NomenclatureCategory? _defaultSaleCategory;

		[Display(Name = "Номенклатура на продажу")]
		public virtual NomenclatureCategory? DefaultSaleCategory
		{
			get => _defaultSaleCategory;
			set => SetField(ref _defaultSaleCategory, value);
		}

		private bool _logisticDeliveryOrders;

		/// <summary>
		/// Для установки фильра заказов для обычной доставки
		/// </summary>
		[Display(Name = "Доставка")]
		public virtual bool LogisticDeliveryOrders
		{
			get => _logisticDeliveryOrders;
			set => SetField(ref _logisticDeliveryOrders, value);
		}

		private bool _logisticServiceOrders;

		/// <summary>
		/// Для установки фильтра заказов с сервисным обслуживанием (выезд мастеров)
		/// </summary>
		[Display(Name = "Сервисное обслуживание")]
		public virtual bool LogisticServiceOrders
		{
			get => _logisticServiceOrders;
			set => SetField(ref _logisticServiceOrders, value);
		}

		private bool _logisticChainStoreOrders;

		/// <summary>
		/// Для установки фильтра заказов для сетевых магазинов
		/// </summary>
		[Display(Name = "Сетевые магазины")]
		public virtual bool LogisticChainStoreOrders
		{
			get => _logisticChainStoreOrders;
			set => SetField(ref _logisticChainStoreOrders, value);
		}

		/// <summary>
		/// Использовать отдел сотрудника
		/// </summary>
		private bool _useEmployeeSubdivision;
		[Display(Name = "Использовать отдел сотрудника")]
		public virtual bool UseEmployeeSubdivision
		{
			get => _useEmployeeSubdivision;
			set => SetField(ref _useEmployeeSubdivision, value);
		}

		/// <summary>
		/// Для установки фильтра подразделений
		/// </summary>
		private Subdivision _defaultSubdivision;

		[Display(Name = "Подразделение")]
		public virtual Subdivision DefaultSubdivision
		{
			get => _defaultSubdivision;
			set => SetField(ref _defaultSubdivision, value);
		}

		/// <summary>
		/// Для установки дефолтного контрагента в отчете по оплатам
		/// </summary>
		private Counterparty _defaultCounterparty;
		[Display(Name = "Контрагент")]
		public virtual Counterparty DefaultCounterparty
		{
			get => _defaultCounterparty;
			set => SetField(ref _defaultCounterparty, value);
		}

		/// <summary>
		/// Статус рекламации
		/// </summary>
		private ComplaintStatuses? _defaultComplaintStatus;
		private string _salesBySubdivisionsAnalitycsReportWarehousesString;
		private string _salesBySubdivisionsAnalitycsReportSubdivisionsString;
		private string _themeName;

		[Display(Name = "Статус рекламации")]
		public virtual ComplaintStatuses? DefaultComplaintStatus
		{
			get => _defaultComplaintStatus;
			set => SetField(ref _defaultComplaintStatus, value);
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

		public virtual string SalesBySubdivisionsAnalitycsReportWarehousesString
		{
			get => _salesBySubdivisionsAnalitycsReportWarehousesString;
			set => SetField(ref _salesBySubdivisionsAnalitycsReportWarehousesString, value);
		}

		public virtual string SalesBySubdivisionsAnalitycsReportSubdivisionsString
		{
			get => _salesBySubdivisionsAnalitycsReportSubdivisionsString;
			set => SetField(ref _salesBySubdivisionsAnalitycsReportSubdivisionsString, value);
		}

		[Display(Name = "Выбранные пользователем склады для отслеживания наличия перемещений ожидающих приемки")]
		public virtual string MovementDocumentsNotificationUserSelectedWarehousesString
		{
			get => _movementDocumentsNotificationUserSelectedWarehousesString;
			set => SetField(ref _movementDocumentsNotificationUserSelectedWarehousesString, value);
		}

		[PropertyChangedAlso(nameof(SalesBySubdivisionsAnalitycsReportWarehousesString))]
		public virtual IEnumerable<int> SalesBySubdivisionsAnalitycsReportWarehouses
		{
			get => string.IsNullOrWhiteSpace(SalesBySubdivisionsAnalitycsReportWarehousesString) ? Enumerable.Empty<int>() : SalesBySubdivisionsAnalitycsReportWarehousesString
				.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => int.Parse(x));
			set => SalesBySubdivisionsAnalitycsReportWarehousesString = string.Join(", ", value);
		}

		[PropertyChangedAlso(nameof(SalesBySubdivisionsAnalitycsReportSubdivisionsString))]
		public virtual IEnumerable<int> SalesBySubdivisionsAnalitycsReportSubdivisions
		{
			get => string.IsNullOrWhiteSpace(SalesBySubdivisionsAnalitycsReportSubdivisionsString) ? Enumerable.Empty<int>() : SalesBySubdivisionsAnalitycsReportSubdivisionsString
				.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => int.Parse(x));
			set => SalesBySubdivisionsAnalitycsReportSubdivisionsString = string.Join(", ", value);
		}

		[PropertyChangedAlso(nameof(MovementDocumentsNotificationUserSelectedWarehousesString))]
		public virtual IEnumerable<int> MovementDocumentsNotificationUserSelectedWarehouses
		{
			get => string.IsNullOrWhiteSpace(MovementDocumentsNotificationUserSelectedWarehousesString) ? Enumerable.Empty<int>() : MovementDocumentsNotificationUserSelectedWarehousesString
				.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => int.Parse(x));
			set => MovementDocumentsNotificationUserSelectedWarehousesString = string.Join(", ", value);
		}

		#endregion
		
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
}
