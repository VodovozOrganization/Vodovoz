using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.PrintableDocuments;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Core.Domain.Users.Settings
{
	/// <summary>
	/// Настройки пользователя
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "настройки пользователя",
		AccusativePlural = "настройки пользователей",
		Genitive = "настроек пользователя",
		GenitivePlural = "настроек пользователей",
		Nominative = "настройки пользователя",
		NominativePlural = "настройки пользователей",
		Prepositional = "настройках пользователя",
		PrepositionalPlural = "настройках пользователей")]
	[EntityPermission]
	public class UserSettings : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private User _user;
		private ToolbarStyle _toolbarStyle = ToolbarStyle.Both;
		private IconsSize _toolBarIconsSize = IconsSize.Large;
		private bool _reorderTabs;
		private bool _highlightTabsWithColor;
		private bool _keepTabColor;
		private bool _hideComplaintNotification;
		private Warehouse _defaultWarehouse;
		private NomenclatureCategory? _defaultSaleCategory;
		private bool _logisticDeliveryOrders;
		private bool _logisticServiceOrders;
		private bool _logisticChainStoreOrders;
		private bool _useEmployeeSubdivision;
		private int? _defaultSubdivisionId;
		private int? _defaultCounterpartyId;
		private string _fuelControlApiLogin;
		private string _fuelControlApiPassword;
		private string _fuelControlApiKey;
		private string _fuelControlApiSessionId;
		private DateTime? _fuelControlApiSessionExpirationDate;

		private ComplaintStatuses? _defaultComplaintStatus;
		private string _salesBySubdivisionsAnalitycsReportWarehousesString;
		private string _salesBySubdivisionsAnalitycsReportSubdivisionsString;
		private string _carIsNotAtLineReportIncludedEventTypeIdsString;
		private string _carIsNotAtLineReportExcludedEventTypeIdsString;

		private IObservableList<CashSubdivisionSortingSettings> _cashSubdivisionSortingSettings = new ObservableList<CashSubdivisionSortingSettings>();
		private IObservableList<DocumentPrinterSetting> _documentPrinterSettings = new ObservableList<DocumentPrinterSetting>();

		private string _movementDocumentsNotificationUserSelectedWarehousesString;

		public UserSettings()
		{
		}

		public UserSettings(User user)
		{
			User = user;
			CarIsNotAtLineReportIncludedEventTypeIdsString = string.Empty;
			CarIsNotAtLineReportExcludedEventTypeIdsString = string.Empty;
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Пользователь
		/// </summary>
		[Display(Name = "Пользователь")]
		public virtual User User
		{
			get => _user;
			set => SetField(ref _user, value);
		}

		/// <summary>
		/// Стиль панели
		/// </summary>
		[Display(Name = "Стиль панели")]
		public virtual ToolbarStyle ToolbarStyle
		{
			get => _toolbarStyle;
			set => SetField(ref _toolbarStyle, value);
		}

		/// <summary>
		/// Размер иконок панели
		/// </summary>
		[Display(Name = "Размер иконок панели")]
		public virtual IconsSize ToolBarIconsSize
		{
			get => _toolBarIconsSize;
			set => SetField(ref _toolBarIconsSize, value);
		}

		/// <summary>
		/// Перемещение вкладок
		/// </summary>
		[Display(Name = "Перемещение вкладок")]
		public virtual bool ReorderTabs
		{
			get => _reorderTabs;
			set => SetField(ref _reorderTabs, value);
		}

		/// <summary>
		/// Выделение вкладок цветом
		/// </summary>
		[Display(Name = "Выделение вкладок цветом")]
		public virtual bool HighlightTabsWithColor
		{
			get => _highlightTabsWithColor;
			set => SetField(ref _highlightTabsWithColor, value);
		}

		/// <summary>
		/// Сохранять цвет вкладки
		/// </summary>
		[Display(Name = "Сохранять цвет вкладки")]
		public virtual bool KeepTabColor
		{
			get => _keepTabColor;
			set => SetField(ref _keepTabColor, value);
		}

		/// <summary>
		/// Скрывать уведомления об открытых рекламациях
		/// </summary>
		[Display(Name = "Скрыть уведомления об открытых рекламациях")]
		public virtual bool HideComplaintNotification
		{
			get => _hideComplaintNotification;
			set => SetField(ref _hideComplaintNotification, value);
		}

		/// <summary>
		/// Склад по умолчанию
		/// </summary>
		[Display(Name = "Склад")]
		public virtual Warehouse DefaultWarehouse
		{
			get => _defaultWarehouse;
			set => SetField(ref _defaultWarehouse, value);
		}

		/// <summary>
		/// Тип номенклатуры на продажу по умолчанию
		/// </summary>
		[Display(Name = "Номенклатура на продажу")]
		public virtual NomenclatureCategory? DefaultSaleCategory
		{
			get => _defaultSaleCategory;
			set => SetField(ref _defaultSaleCategory, value);
		}

		/// <summary>
		/// Для установки фильра заказов для обычной доставки
		/// </summary>
		[Display(Name = "Доставка")]
		public virtual bool LogisticDeliveryOrders
		{
			get => _logisticDeliveryOrders;
			set => SetField(ref _logisticDeliveryOrders, value);
		}

		/// <summary>
		/// Для установки фильтра заказов с сервисным обслуживанием (выезд мастеров)
		/// </summary>
		[Display(Name = "Сервисное обслуживание")]
		public virtual bool LogisticServiceOrders
		{
			get => _logisticServiceOrders;
			set => SetField(ref _logisticServiceOrders, value);
		}

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
		[Display(Name = "Использовать отдел сотрудника")]
		public virtual bool UseEmployeeSubdivision
		{
			get => _useEmployeeSubdivision;
			set => SetField(ref _useEmployeeSubdivision, value);
		}

		/// <summary>
		/// Для установки фильтра подразделений
		/// </summary>
		[Display(Name = "Подразделение")]
		[HistoryIdentifier(TargetType = typeof(SubdivisionEntity))]
		public virtual int? DefaultSubdivisionId
		{
			get => _defaultSubdivisionId;
			set => SetField(ref _defaultSubdivisionId, value);
		}

		/// <summary>
		/// Для установки дефолтного контрагента в отчете по оплатам
		/// </summary>
		[Display(Name = "Контрагент")]
		[HistoryIdentifier(TargetType = typeof(CounterpartyEntity))]
		public virtual int? DefaultCounterpartyId
		{
			get => _defaultCounterpartyId;
			set => SetField(ref _defaultCounterpartyId, value);
		}

		/// <summary>
		/// Логин API управления топливом
		/// </summary>
		[Display(Name = "Логин API управления топливом")]
		public virtual string FuelControlApiLogin
		{
			get => _fuelControlApiLogin;
			set
			{
				if(SetField(ref _fuelControlApiLogin, value))
				{
					ResetFuelControlAPiSessionData();
				}
			}
		}

		/// <summary>
		/// Пароль API управления топливом
		/// </summary>
		[Display(Name = "Пароль API управления топливом")]
		public virtual string FuelControlApiPassword
		{
			get => _fuelControlApiPassword;
			set
			{
				if(SetField(ref _fuelControlApiPassword, value))
				{
					ResetFuelControlAPiSessionData();
				}
			}
		}

		/// <summary>
		/// Ключ API управления топливом
		/// </summary>
		[Display(Name = "Ключ API управления топливом")]
		public virtual string FuelControlApiKey
		{
			get => _fuelControlApiKey;
			set
			{
				if(SetField(ref _fuelControlApiKey, value))
				{
					ResetFuelControlAPiSessionData();
				}
			}
		}

		/// <summary>
		/// Идентификатор сессии API управления топливом
		/// </summary>
		[Display(Name = "Id сессии API управления топливом")]
		public virtual string FuelControlApiSessionId
		{
			get => _fuelControlApiSessionId;
			set => SetField(ref _fuelControlApiSessionId, value);
		}

		/// <summary>
		/// Дата истечения сессии работы с API управления топливом
		/// </summary>
		[Display(Name = "Дата истечения сессии работы с API управления топливом")]
		public virtual DateTime? FuelControlApiSessionExpirationDate
		{
			get => _fuelControlApiSessionExpirationDate;
			set => SetField(ref _fuelControlApiSessionExpirationDate, value);
		}

		/// <summary>
		/// Есть ли у пользователя данные для авторизации в API управления топливом
		/// </summary>
		public virtual bool IsUserHasAuthDataForFuelControlApi =>
			!string.IsNullOrWhiteSpace(FuelControlApiLogin)
			&& !string.IsNullOrWhiteSpace(FuelControlApiPassword)
			&& !string.IsNullOrWhiteSpace(FuelControlApiKey);

		/// <summary>
		/// Требуется ли авторизация в API управления топливом
		/// </summary>
		public virtual bool IsNeedToLoginFuelControlApi =>
			string.IsNullOrWhiteSpace(FuelControlApiSessionId)
			|| !FuelControlApiSessionExpirationDate.HasValue
			|| FuelControlApiSessionExpirationDate <= DateTime.Today;

		/// <summary>
		/// Сброс данных сессии API управления топливом
		/// </summary>
		private void ResetFuelControlAPiSessionData()
		{
			FuelControlApiSessionId = string.Empty;
			FuelControlApiSessionExpirationDate = null;

			OnPropertyChanged(nameof(IsUserHasAuthDataForFuelControlApi));
		}

		/// <summary>
		/// Статус рекламации
		/// </summary>
		[Display(Name = "Статус рекламации")]
		public virtual ComplaintStatuses? DefaultComplaintStatus
		{
			get => _defaultComplaintStatus;
			set => SetField(ref _defaultComplaintStatus, value);
		}

		/// <summary>
		/// Настройки сортировки касс
		/// </summary>
		[Display(Name = "Настройки сортировки касс")]
		public virtual IObservableList<CashSubdivisionSortingSettings> CashSubdivisionSortingSettings
		{
			get => _cashSubdivisionSortingSettings;
			set => SetField(ref _cashSubdivisionSortingSettings, value);
		}

		/// <summary>
		/// Склады для отчета по продажам по подразделениям
		/// </summary>
		public virtual string SalesBySubdivisionsAnalitycsReportWarehousesString
		{
			get => _salesBySubdivisionsAnalitycsReportWarehousesString;
			set => SetField(ref _salesBySubdivisionsAnalitycsReportWarehousesString, value);
		}

		/// <summary>
		/// Подразделения для отчета по продажам по подразделениям
		/// </summary>
		public virtual string SalesBySubdivisionsAnalitycsReportSubdivisionsString
		{
			get => _salesBySubdivisionsAnalitycsReportSubdivisionsString;
			set => SetField(ref _salesBySubdivisionsAnalitycsReportSubdivisionsString, value);
		}

		/// <summary>
		/// Выбранные пользователем склады для отчета по продажам по подразделениям
		/// </summary>
		[Display(Name = "Выбранные пользователем склады для отслеживания наличия перемещений ожидающих приемки")]
		public virtual string MovementDocumentsNotificationUserSelectedWarehousesString
		{
			get => _movementDocumentsNotificationUserSelectedWarehousesString;
			set => SetField(ref _movementDocumentsNotificationUserSelectedWarehousesString, value);
		}

		/// <summary>
		/// Склады для отчета по продажам по подразделениям
		/// </summary>
		[PropertyChangedAlso(nameof(SalesBySubdivisionsAnalitycsReportWarehousesString))]
		public virtual IEnumerable<int> SalesBySubdivisionsAnalitycsReportWarehouses
		{
			get => string.IsNullOrWhiteSpace(SalesBySubdivisionsAnalitycsReportWarehousesString) ? Enumerable.Empty<int>() : SalesBySubdivisionsAnalitycsReportWarehousesString
				.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => int.Parse(x));
			set => SalesBySubdivisionsAnalitycsReportWarehousesString = string.Join(", ", value);
		}

		/// <summary>
		/// Подразделения для отчета по продажам по подразделениям
		/// </summary>
		[PropertyChangedAlso(nameof(SalesBySubdivisionsAnalitycsReportSubdivisionsString))]
		public virtual IEnumerable<int> SalesBySubdivisionsAnalitycsReportSubdivisions
		{
			get => string.IsNullOrWhiteSpace(SalesBySubdivisionsAnalitycsReportSubdivisionsString) ? Enumerable.Empty<int>() : SalesBySubdivisionsAnalitycsReportSubdivisionsString
				.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => int.Parse(x));
			set => SalesBySubdivisionsAnalitycsReportSubdivisionsString = string.Join(", ", value);
		}

		/// <summary>
		/// Склады для отслеживания наличия документов перемещений
		/// </summary>
		[PropertyChangedAlso(nameof(MovementDocumentsNotificationUserSelectedWarehousesString))]
		public virtual IEnumerable<int> MovementDocumentsNotificationUserSelectedWarehouses
		{
			get => string.IsNullOrWhiteSpace(MovementDocumentsNotificationUserSelectedWarehousesString) ? Enumerable.Empty<int>() : MovementDocumentsNotificationUserSelectedWarehousesString
				.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => int.Parse(x));
			set => MovementDocumentsNotificationUserSelectedWarehousesString = string.Join(", ", value);
		}

		/// <summary>
		/// Выбранные пользователем типы событий отчета по простоям на включение в отчет
		/// </summary>
		[Display(Name = "Выбранные пользователем типы событий отчета по простоям на включение в отчет")]
		public virtual string CarIsNotAtLineReportIncludedEventTypeIdsString
		{
			get => _carIsNotAtLineReportIncludedEventTypeIdsString;
			set => SetField(ref _carIsNotAtLineReportIncludedEventTypeIdsString, value);
		}

		/// <summary>
		/// Выбранные пользователем типы событий отчета по простоям на исключение из отчета
		/// </summary>
		[Display(Name = "Выбранные пользователем типы событий отчета по простоям на исключение из отчета")]
		public virtual string CarIsNotAtLineReportExcludedEventTypeIdsString
		{
			get => _carIsNotAtLineReportExcludedEventTypeIdsString;
			set => SetField(ref _carIsNotAtLineReportExcludedEventTypeIdsString, value);
		}

		/// <summary>
		/// Типы включаемых в отчет по простоям событий, которые выбраны пользователем
		/// </summary>
		[PropertyChangedAlso(nameof(CarIsNotAtLineReportIncludedEventTypeIdsString))]
		public virtual IEnumerable<int> CarIsNotAtLineReportIncludedEventTypeIds
		{
			get => string.IsNullOrWhiteSpace(CarIsNotAtLineReportIncludedEventTypeIdsString) ? Enumerable.Empty<int>() : CarIsNotAtLineReportIncludedEventTypeIdsString
				.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => int.Parse(x));
			set => CarIsNotAtLineReportIncludedEventTypeIdsString = string.Join(", ", value);
		}

		/// <summary>
		/// Тип исключаемых из отчета по простоям событий, которые выбраны пользователем
		/// </summary>
		[PropertyChangedAlso(nameof(CarIsNotAtLineReportExcludedEventTypeIdsString))]
		public virtual IEnumerable<int> CarIsNotAtLineReportExcludedEventTypeIds
		{
			get => string.IsNullOrWhiteSpace(CarIsNotAtLineReportExcludedEventTypeIdsString) ? Enumerable.Empty<int>() : CarIsNotAtLineReportExcludedEventTypeIdsString
				.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => int.Parse(x));
			set => CarIsNotAtLineReportExcludedEventTypeIdsString = string.Join(", ", value);
		}

		/// <summary>
		/// Настройки принтеров для документов
		/// </summary>
		[Display(Name = "Настройки принтеров для документов")]
		public virtual IObservableList<DocumentPrinterSetting> DocumentPrinterSettings
		{
			get => _documentPrinterSettings;
			set => SetField(ref _documentPrinterSettings, value);
		}

		/// <summary>
		/// Получение настройки принтера документа по типу документа
		/// </summary>
		/// <param name="documentType"></param>
		/// <returns></returns>
		public virtual DocumentPrinterSetting GetPrinterSettingByDocumentType(CustomPrintDocumentType documentType) =>
			DocumentPrinterSettings
				.Where(s => s.DocumentType == documentType)
				.FirstOrDefault();

		/// <summary>
		/// Обновление индексов сортировки касс
		/// </summary>
		public virtual void UpdateCashSortingIndices()
		{
			for(int i = 1; i <= CashSubdivisionSortingSettings.Count; i++)
			{
				CashSubdivisionSortingSettings[i].SortingIndex = i;
			}
		}

		/// <summary>
		/// Актуализация настроек сортировки
		/// </summary>
		/// <param name="availableSubdivisions"></param>
		/// <returns></returns>
		public virtual bool UpdateCashSortingSettings(IEnumerable<int> availableSubdivisionsIds)
		{
			var availableSubdvisionsIdsArray = availableSubdivisionsIds.ToArray();

			if(!CashSubdivisionSortingSettings.Any())
			{
				for(int i = 0; i < availableSubdivisionsIds.Count(); i++)
				{
					CashSubdivisionSortingSettings.Add(new CashSubdivisionSortingSettings(i + 1, Id, availableSubdvisionsIdsArray[i]));
				}

				return availableSubdvisionsIdsArray.Any();
			}

			var notAvailableAnymore = CashSubdivisionSortingSettings
				.Where(x => x.CashSubdivisionId != null
					&& !availableSubdvisionsIdsArray.Contains(x.CashSubdivisionId.Value))
				.ToList();

			// убираем кассы, к которым больше нет доступа
			foreach(var item in notAvailableAnymore)
			{
				CashSubdivisionSortingSettings.Remove(item);
			}

			if(notAvailableAnymore.Any())
			{
				UpdateCashSortingIndices();
			}

			var listedIds = CashSubdivisionSortingSettings.Select(x => x.CashSubdivisionId).ToList();
			var notListedAsAvailable = availableSubdvisionsIdsArray.Where(x => listedIds.IndexOf(x) == -1).ToList();
			int lastIndex = -1;

			if(CashSubdivisionSortingSettings.Any())
			{
				lastIndex = CashSubdivisionSortingSettings.Max(x => x.SortingIndex);
			}

			// добавляем кассы, к которым появился доступ
			foreach(var item in notListedAsAvailable)
			{
				CashSubdivisionSortingSettings.Add(new CashSubdivisionSortingSettings(++lastIndex, Id, item));
			}

			return notListedAsAvailable.Any() || notAvailableAnymore.Any();
		}
	}
}
