using System;
using System.Collections.Generic;
using System.IO;
using Gamma.Binding;
using Gamma.Utilities;
using NHibernate.AdoNet;
using NHibernate.Cfg;
using QS.Banks.Domain;
using QS.BusinessCommon.Domain;
using QS.Dialog.Gtk;
using QS.HistoryLog;
using QS.Permissions;
using QS.Print;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Widgets.GtkUI;
using QSBusinessCommon;
using QSDocTemplates;
using QSOrmProject;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using Vodovoz.Core;
using Vodovoz.Core.Permissions;
using Vodovoz.Dialogs;
using Vodovoz.Dialogs.Cash;
using Vodovoz.Dialogs.Cash.CashTransfer;
using Vodovoz.Dialogs.Client;
using Vodovoz.Dialogs.DocumentDialogs;
using Vodovoz.Dialogs.Employees;
using Vodovoz.Dialogs.Fuel;
using Vodovoz.Dialogs.Goods;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain;
using Vodovoz.Domain.Accounting;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.StoredResources;
using Vodovoz.Filters.GtkViews;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.Infrastructure.Permissions;
using Vodovoz.JournalColumnsConfigs;
using Vodovoz.Services.Permissions;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Organization;
using Vodovoz.ViewModels.Suppliers;
using Vodovoz.ViewModels.WageCalculation;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Views;
using Vodovoz.Views.Complaints;
using Vodovoz.Views.Employees;
using Vodovoz.Views.Logistic;
using Vodovoz.Views.Organization;
using Vodovoz.Views.Suppliers;
using Vodovoz.Views.WageCalculation;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;
using Vodovoz.ViewWidgets.AdvancedWageParameterViews;
using Vodovoz.Views.Warehouse;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewWidgets.PromoSetAction;
using Vodovoz.Views.Contacts;
using Vodovoz.ViewModels.Users;
using Vodovoz.Views.Users;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewWidgets.Permissions;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ReportsParameters;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels.BusinessTasks;
using Vodovoz.Views.BusinessTasks;
using Vodovoz.Footers.ViewModels;
using Vodovoz.Footers.Views;
using Vodovoz.Journals.FilterViewModels;

namespace Vodovoz
{
	partial class MainClass
	{
		static void CreateProjectParam()
		{
			UserDialog.RequestWidth = 900;
			UserDialog.RequestHeight = 700;

			PermissionsSettings.PresetPermissions.Add("can_activate_districts_set", new PresetUserPermissionSource("can_activate_districts_set", "Пользователь может активировать версию районов", string.Empty));
			PermissionsSettings.PresetPermissions.Add("driver_terminal", new PresetUserPermissionSource("driver_terminal", "ВНИМАНИЕ! Аккаунт будет использоватся только для печати документов МЛ", "Для использования отдельного окна для печати документов МЛ без доступа к остальным частям системы."));
			PermissionsSettings.PresetPermissions.Add("max_loan_amount", new PresetUserPermissionSource("max_loan_amount", "Установка максимального кредита", "Пользователь имеет права для установки максимальной суммы кредита."));
			PermissionsSettings.PresetPermissions.Add("logistican", new PresetUserPermissionSource("logistican", "Логист", "Пользователь является логистом."));
			PermissionsSettings.PresetPermissions.Add("logistic_changedeliverytime", new PresetUserPermissionSource("logistic_changedeliverytime", "Логистика. Изменение времени доставки при ведении МЛ", "Пользователь может изменять время доставки в диалоге ведения маршрутного листа"));
			PermissionsSettings.PresetPermissions.Add("money_manage_bookkeeping", new PresetUserPermissionSource("money_manage_bookkeeping", "Управление деньгами. Бухгалтерия.", "Пользователь имеет доступ к денежным операциям во вкладке \"Бухгалтерия\"."));
			PermissionsSettings.PresetPermissions.Add("money_manage_cash", new PresetUserPermissionSource("money_manage_cash", "Управление деньгами. Касса.", "Пользователь имеет доступ к денежным операциям во вкладке \"Касса\"."));
			PermissionsSettings.PresetPermissions.Add("routelist_unclosing", new PresetUserPermissionSource("routelist_unclosing", "Касса. Отмена закрытия маршрутных листов", "Пользователь может переводить маршрутные листы из статуса Закрыт в статус Сдается"));
			PermissionsSettings.PresetPermissions.Add("can_delete", new PresetUserPermissionSource("can_delete", "Удаление заказов и маршрутных листов", "Пользователь может удалять заказы и маршрутные листы в журналах."));
			PermissionsSettings.PresetPermissions.Add("can_delete_fines", new PresetUserPermissionSource("can_delete_fines", "Удаление и изменение штрафов", "Пользователь может удалять и изменять штрафы."));
			PermissionsSettings.PresetPermissions.Add("can_close_orders", new PresetUserPermissionSource("can_close_orders", "Закрытие заказов", "Пользователь может закрывать заказы вручную."));
			PermissionsSettings.PresetPermissions.Add("can_edit_wage", new PresetUserPermissionSource("can_edit_wage", "Установка заработной платы ", "Пользователь может устанавливать тип заработной платы и ставку."));
			PermissionsSettings.PresetPermissions.Add("change_driver_wage", new PresetUserPermissionSource("change_driver_wage", "Изменение типа расчета ЗП в МЛ", "Пользователь может устанавливать для МЛ другой расчет заработной платы."));
			PermissionsSettings.PresetPermissions.Add("can_create_and_arc_nomenclatures", new PresetUserPermissionSource("can_create_and_arc_nomenclatures", "Создание и архивирование номенклатур", "Пользователь может создавать номенклатуры и устанавливать галочку архивный."));
			PermissionsSettings.PresetPermissions.Add("can_delete_counterparty_and_deliverypoint", new PresetUserPermissionSource("can_delete_counterparty_and_deliverypoint", "Удаление контрагентов и точек доставки", "Пользователь может удалять контрагентов и точки доставки."));
			PermissionsSettings.PresetPermissions.Add("can_arc_counterparty_and_deliverypoint", new PresetUserPermissionSource("can_arc_counterparty_and_deliverypoint", "Архивирование контрагентов и точек доставки", "Пользователь может устанавливать галочку архивный для контрагентов и точек доставки."));
			PermissionsSettings.PresetPermissions.Add("can_set_common_additionalagreement", new PresetUserPermissionSource("can_set_common_additionalagreement", "Возврат общего доп.соглашения", "Пользователь может нажать кнопку 'Вернуть общий' в доп.соглашении."));
			PermissionsSettings.PresetPermissions.Add("can_confirm_routelist_with_overweight", new PresetUserPermissionSource("can_confirm_routelist_with_overweight", "Подтверждение МЛ с перегрузом", "Пользователь может подтверждать МЛ, суммарный вес товаров и оборудования в котором превышает грузоподъемность автомобиля."));
			PermissionsSettings.PresetPermissions.Add("can_set_contract_closer", new PresetUserPermissionSource("can_set_contract_closer", "Установка крыжика 'Закрывашка по контракту'", "Пользователю доступна возможность установки крыжика 'Закрывашка по контракту'."));
			PermissionsSettings.PresetPermissions.Add("can_can_create_order_in_advance", new PresetUserPermissionSource("can_can_create_order_in_advance", "Проведение накладных задним числом", "Пользователь может создавать заказы с датой доставки более ранней, чем текущая дата."));
			PermissionsSettings.PresetPermissions.Add("can_create_several_orders_for_date_and_deliv_point", new PresetUserPermissionSource("can_create_several_orders_for_date_and_deliv_point", "Создание нескольких заказов для точки доставки на одну дату доставки.", "Пользователь может создавать несколько заказов для одной и той же точки доставки на одну и туже дату доставки."));
			PermissionsSettings.PresetPermissions.Add("can_add_spares_to_order", new PresetUserPermissionSource("can_add_spares_to_order", "Продажа запчастей", "Пользователь может добавлять запчасти, которые отмечены как \"Не для продажи\", в заказ на продажу."));
			PermissionsSettings.PresetPermissions.Add("can_add_bottles_to_order", new PresetUserPermissionSource("can_add_bottles_to_order", "Продажа тары", "Пользователь может добавлять тару, которая отмечена как \"Не для продажи\", в заказ на продажу."));
			PermissionsSettings.PresetPermissions.Add("can_add_materials_to_order", new PresetUserPermissionSource("can_add_materials_to_order", "Продажа сырья", "Пользователь может добавлять сырьё, которое отмечено как \"Не для продажи\", в заказ на продажу."));
			PermissionsSettings.PresetPermissions.Add("can_add_equipment_not_for_sale_to_order", new PresetUserPermissionSource("can_add_equipment_not_for_sale_to_order", "Продажа оборудования", "Пользователь может добавлять оборудование, которое отмечено как \"Не для продажи\", в заказ на продажу."));
			PermissionsSettings.PresetPermissions.Add("can_edit_delivery_schedule", new PresetUserPermissionSource("can_edit_delivery_schedule", "Изменение времени доставки", "Пользователь может изменять время доставки."));
			PermissionsSettings.PresetPermissions.Add("can_edit_undeliveries", new PresetUserPermissionSource("can_edit_undeliveries", "Изменение недовозов", "Пользователь может изменять недовозы, в т.ч. менять их статус."));
			PermissionsSettings.PresetPermissions.Add("can_close_undeliveries", new PresetUserPermissionSource("can_close_undeliveries", "Закрытие недовозов", "Пользователь может переводить статус недовоза в \"Закрыт\""));
			PermissionsSettings.PresetPermissions.Add("can_archive_warehouse", new PresetUserPermissionSource("can_archive_warehouse", "Архивирование склада", "Пользователь может архивировать склады."));
			PermissionsSettings.PresetPermissions.Add("access_to_salaries", new PresetUserPermissionSource("access_to_salaries", "Доступ к зарплатам и отчётам по ним", "Пользователю предоставляется доступ к отчётам по зарплатам водителей и экспедиторов"));
			PermissionsSettings.PresetPermissions.Add("access_to_fines_bonuses", new PresetUserPermissionSource("access_to_fines_bonuses", "Доступ к премиям, штрафам и отчётам по ним", "Пользователю предоставляется доступ к отчету по штрафам и премиям, а так же ко вкладке \"Штрафы и премии\" в \"Кадры\""));
			PermissionsSettings.PresetPermissions.Add("can_move_order_from_closed_to_acepted", new PresetUserPermissionSource("can_move_order_from_closed_to_acepted", "Перевод заказа из \"Закрыт\" в \"Принят\"", "Пользователь может вернуть заказ, находящийся в статусе \"Закрыт\", в статус \"Принят\". Это касается только заказов, закрытых без доставки, то есть те, у которых нет МЛ."));
			PermissionsSettings.PresetPermissions.Add("can_accept_cashles_service_orders", new PresetUserPermissionSource("can_accept_cashles_service_orders", "Проведение безналичного заказа на \"Выезд мастера\"", "Пользователь может подтверждать заказы по безналу типа \"Выезд мастера\". В случае отсутствия этого права, пользователю будет доступен только перевод заказа в статус \"Ожидание оплаты\"."));
			PermissionsSettings.PresetPermissions.Add("can_change_trainee_to_driver", new PresetUserPermissionSource("can_change_trainee_to_driver", "Перевод стажера в водителя или экспедитора", "Позволяет перевести стажера в статус водителя или экспедитора"));
			PermissionsSettings.PresetPermissions.Add("database_maintenance", new PresetUserPermissionSource("database_maintenance", "Обслуживание базы данных", "Предоставить пользователю права на доступ ко вкладке База --> Обслуживание"));
			PermissionsSettings.PresetPermissions.Add("can_edit_online_store", new PresetUserPermissionSource("can_edit_online_store", "Изменение для онлайн магазина", "Пользователь может изменять группы товаров влияющие на выгрузку в интернет магазин."));
			PermissionsSettings.PresetPermissions.Add("can_edit_delivery_price_rules", new PresetUserPermissionSource("can_edit_delivery_price_rules", "Создание и изменение правил доставки", "Пользователь может создавать и изменять правила доставки.\nДля установки цен доставки отдельных прав не нужно."));
			PermissionsSettings.PresetPermissions.Add("can_set_free_delivery", new PresetUserPermissionSource("can_set_free_delivery", "Отключение для точки доставки платной доставки", "Пользователь может отмечать точки доставки флагом \"Всегда бесплатная доставка\""));
			PermissionsSettings.PresetPermissions.Add("allow_load_selfdelivery", new PresetUserPermissionSource("allow_load_selfdelivery", "Разрешение отгрузки самовывоза", "Пользователь может переводить заказ с самовывозом в статус на погрузку"));
			PermissionsSettings.PresetPermissions.Add("accept_cashless_paid_selfdelivery", new PresetUserPermissionSource("accept_cashless_paid_selfdelivery", "Разрешение отметки оплаты самовывоза", "Пользователь может отмечать заказ с самовывозом по безналу как оплаченный"));
			PermissionsSettings.PresetPermissions.Add("can_edit_logistic_areas", new PresetUserPermissionSource("can_edit_logistic_areas", "Доступ к редактированию логистических районов", "Пользователь может редактировать логистические районы"));
			PermissionsSettings.PresetPermissions.Add("can_send_not_loaded_route_lists_en_route", new PresetUserPermissionSource("can_send_not_loaded_route_lists_en_route", "Разрешение отправки недогруженых МЛ в путь", "Пользователь может отправлять недогруженные маршрутные листы в путь"));
			PermissionsSettings.PresetPermissions.Add("can_confirm_mileage_for_our_GAZelles_Larguses", new PresetUserPermissionSource("can_confirm_mileage_for_our_GAZelles_Larguses", "Разрешение подтверждать киллометраж для наших ГАЗелей и Ларгусов", "Разрешение подтверждать киллометраж для наших ГАЗелей и Ларгусов"));
			PermissionsSettings.PresetPermissions.Add("can_close_deliveries_for_counterparty", new PresetUserPermissionSource("can_close_deliveries_for_counterparty", "Возможность закрыть поставки для клиента", "Возможность закрыть поставки для клиента"));
			PermissionsSettings.PresetPermissions.Add("can_edit_cashier_review_comment", new PresetUserPermissionSource("can_edit_cashier_review_comment", "Комментарий по проверке кассы", "Возможность изменять комментарий по проверке кассы"));
			PermissionsSettings.PresetPermissions.Add("can_edit_fuelwriteoff_document_date", new PresetUserPermissionSource("can_edit_fuelwriteoff_document_date", "Смена даты в акте выдачи топлива", "Возможность изменять дату в акте выдачи топлива"));
			PermissionsSettings.PresetPermissions.Add("can_edit_price_discount_from_route_list", new PresetUserPermissionSource("can_edit_price_discount_from_route_list", "Изменение цены и скидки в закрытии МЛ", "Возможность изменять цену и скидку в заказе из закрытия МЛ"));
			PermissionsSettings.PresetPermissions.Add("can_edit_order", new PresetUserPermissionSource("can_edit_order", "Изменение заказа", "Для тех, у кого нет права, засеривается кнопка в Заказе \"Редактировать\""));
			PermissionsSettings.PresetPermissions.Add("can_complete_complaint_discussion", new PresetUserPermissionSource("can_complete_complaint_discussion", "Завершение обсуждения в жалобе", "Дает возможность пользователю завершить обсуждение в жалобе"));
			PermissionsSettings.PresetPermissions.Add("can_change_fuel_card_number", new PresetUserPermissionSource("can_change_fuel_card_number", "Изменение номера ТК в карточке автомобиля", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_change_car_load_and_unload_docs", new PresetUserPermissionSource("can_change_car_load_and_unload_docs", "Редактирование талонов разгрузки и погрузки по закрытым МЛ", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_change_district_wage_type", new PresetUserPermissionSource("can_change_district_wage_type", "Изменение зарплатного типа района", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_change_cars_volume_weight_consumption", new PresetUserPermissionSource("can_change_cars_volume_weight_consumption", "Изменение в автомобиле расхода топлива, объема груза, грузоподъемности", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_edit_counterparty_details", new PresetUserPermissionSource("can_edit_counterparty_details", "Редактирование реквизитов контрагента", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_edit_order_extra_cash", new PresetUserPermissionSource("can_edit_order_extra_cash", "Редактирование доп. нала в заказе", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_fire_employees", new PresetUserPermissionSource("can_fire_employees", "Возможность поставить статус \"Уволен\" сотруднику", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_accept_movement_document_dicrepancy", new PresetUserPermissionSource("can_accept_movement_document_dicrepancy", "Подтверждение расхождений в документа перемещения ТМЦ", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_manage_users", new PresetUserPermissionSource("can_manage_users", "Изменение существующего и создание нового пользователей в диалоге сотрудника", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_change_undelivery_problem_source", new PresetUserPermissionSource("can_change_undelivery_problem_source", "Изменение источника проблемы в недовозах", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_add_online_store_nomenclatures_to_order", new PresetUserPermissionSource("can_add_online_store_nomenclatures_to_order", "Добавление номенклатур интернет магазина в заказ", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_change_driver_surcharge", new PresetUserPermissionSource("can_change_driver_surcharge", "Изменение доплаты водителя", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_retrieve_routelist_en_route", new PresetUserPermissionSource("can_retrieve_routelist_en_route", "Возможность вернуть статус МЛ в путь", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_change_cars_bottles_from_address", new PresetUserPermissionSource("can_change_cars_bottles_from_address", "Изменение в автомобиле количества забираемых бутылей с адреса", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_change_accepted_movement_doc", new PresetUserPermissionSource("can_change_accepted_movement_doc", "Изменение документа перемещения в статусе \"Принят\"", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_manage_drivers_and_forwarders", new PresetUserPermissionSource("can_manage_drivers_and_forwarders", "Создание и изменение водителей и экспедиторов", string.Empty));
			PermissionsSettings.PresetPermissions.Add("can_manage_office_workers", new PresetUserPermissionSource("can_manage_office_workers", "Создание и изменение офисных работников", string.Empty));

			UserDialog.UserPermissionViewsCreator = () => {
				return new List<IUserPermissionTab> {
					new SubdivisionForUserEntityPermissionWidget(),
					new PresetPermissionsView()
				};
			};

			UserDialog.PermissionViewsCreator = () => {
				return new List<IPermissionsView> { new PermissionMatrixView(new PermissionMatrix<WarehousePermissions, Warehouse>(), "Доступ к складам", "warehouse_access") };
			};

			WarehousePermissionService.WarehousePermissionValidatorFactory = new WarehousePermissionValidatorFactory();
		}

		static void ConfigureViewModelWidgetResolver()
		{
			ViewModelWidgetResolver.Instance = new BasedOnNameViewModelWidgetResolver();

			//Регистрация вкладок
			ViewModelWidgetResolver.Instance
				.RegisterWidgetForTabViewModel<DistrictViewModel, DistrictView>()
				.RegisterWidgetForTabViewModel<DistrictsSetActivationViewModel, DistrictsSetActivationView>()
				.RegisterWidgetForTabViewModel<FuelTransferDocumentViewModel, FuelTransferDocumentView>()
				.RegisterWidgetForTabViewModel<FuelIncomeInvoiceViewModel, FuelIncomeInvoiceView>()
				.RegisterWidgetForTabViewModel<ClientCameFromViewModel, ClientCameFromView>()
				.RegisterWidgetForTabViewModel<ExpenseCategoryViewModel, ExpenseCategoryView>()
				.RegisterWidgetForTabViewModel<FuelTypeViewModel, FuelTypeView>()
				.RegisterWidgetForTabViewModel<FuelWriteoffDocumentViewModel, FuelWriteoffDocumentView>()
				.RegisterWidgetForTabViewModel<ResidueViewModel, ResidueView>()
				.RegisterWidgetForTabViewModel<FineTemplateViewModel, FineTemplateView>()
				.RegisterWidgetForTabViewModel<ComplaintViewModel, ComplaintView>()
				.RegisterWidgetForTabViewModel<CreateComplaintViewModel, CreateComplaintView>()
				.RegisterWidgetForTabViewModel<CreateInnerComplaintViewModel, CreateInnerComplaintView>()
				.RegisterWidgetForTabViewModel<ComplaintSourceViewModel, ComplaintSourceView>()
				.RegisterWidgetForTabViewModel<ComplaintResultViewModel, ComplaintResultView>()
				.RegisterWidgetForTabViewModel<SubdivisionViewModel, SubdivisionView>()
				.RegisterWidgetForTabViewModel<FineViewModel, FineView>()
				.RegisterWidgetForTabViewModel<RequestToSupplierViewModel, RequestToSupplierView>()
				.RegisterWidgetForTabViewModel<WageDistrictViewModel, WageDistrictView>()
				.RegisterWidgetForTabViewModel<WageDistrictLevelRatesViewModel, WageDistrictLevelRatesView>()
				.RegisterWidgetForTabViewModel<EmployeeWageParameterViewModel, WageParameterView>()
				.RegisterWidgetForTabViewModel<SalesPlanViewModel, SalesPlanView>()
				.RegisterWidgetForTabViewModel<RouteListsOnDayViewModel, RouteListsOnDayView>()
				.RegisterWidgetForTabViewModel<FuelDocumentViewModel, FuelDocumentView>()
				.RegisterWidgetForTabViewModel<ComplaintKindViewModel, ComplaintKindView>()
				.RegisterWidgetForTabViewModel<MovementDocumentViewModel, MovementDocumentView>()
				.RegisterWidgetForTabViewModel<PhoneTypeViewModel, PhoneTypeView>()
				.RegisterWidgetForTabViewModel<EmailTypeViewModel, EmailTypeView>()
				.RegisterWidgetForTabViewModel<UserSettingsViewModel, UserSettingsView>()
				.RegisterWidgetForTabViewModel<PaymentLoaderVM, PaymentLoaderView>()
				.RegisterWidgetForTabViewModel<ManualPaymentMatchingVM, ManualPaymentMatchingView>()
				.RegisterWidgetForTabViewModel<ClientTaskViewModel, ClientTaskView>()
				.RegisterWidgetForTabViewModel<PaymentTaskViewModel, PaymentTaskView>()
				.RegisterWidgetForTabViewModel<DistrictsSetViewModel, DistrictsSetView>()
				.RegisterWidgetForTabViewModel<AcceptBeforeViewModel, AcceptBeforeView>()
				;

			//Регистрация виджетов
			ViewModelWidgetResolver.Instance
				.RegisterWidgetForWidgetViewModel<DistrictsSetJournalFilterViewModel, DistrictsSetJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<DistrictJournalFilterViewModel, DistrictJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<SelectableParameterReportFilterViewModel, SelectableParameterReportFilterView>()
				.RegisterWidgetForWidgetViewModel<ComplaintFilterViewModel, ComplaintFilterView>()
				.RegisterWidgetForWidgetViewModel<CounterpartyJournalFilterViewModel, CounterpartyFilterView>()
				.RegisterWidgetForWidgetViewModel<DebtorsJournalFilterViewModel, DebtorsFilterView>()
				.RegisterWidgetForWidgetViewModel<EmployeeFilterViewModel, EmployeeFilterView>()
				.RegisterWidgetForWidgetViewModel<OrderJournalFilterViewModel, OrderFilterView>()
				.RegisterWidgetForWidgetViewModel<ClientCameFromFilterViewModel, ClientCameFromFilterView>()
				.RegisterWidgetForWidgetViewModel<ResidueFilterViewModel, ResidueFilterView>()
				.RegisterWidgetForWidgetViewModel<FineFilterViewModel, FineFilterView>()
				.RegisterWidgetForWidgetViewModel<SubdivisionFilterViewModel, SubdivisionFilterView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureFilterViewModel, NomenclaturesFilterView>()
				.RegisterWidgetForWidgetViewModel<RequestsToSuppliersFilterViewModel, RequestsToSuppliersFilterView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureStockFilterViewModel, NomenclatureStockFilterView>()
				.RegisterWidgetForWidgetViewModel<OrderForMovDocJournalFilterViewModel, OrderForMovDocFilterView>()
				.RegisterWidgetForWidgetViewModel<BottlesCountAdvancedWageParameterViewModel, BottlesCountAdvancedWageParameterWidget>()
				.RegisterWidgetForWidgetViewModel<DeliveryTimeAdvancedWageParameterViewModel, DeliveryTimeAdvancedWagePrameterView>()
				.RegisterWidgetForWidgetViewModel<AdvancedWageParametersViewModel, AdvancedWageParametersView>()
				.RegisterWidgetForWidgetViewModel<AddFixPriceActionViewModel, AddFixPriceActionView>()
				.RegisterWidgetForWidgetViewModel<CarJournalFilterViewModel, CarFilterView>()
				.RegisterWidgetForWidgetViewModel<PresetSubdivisionPermissionsViewModel, PresetPermissionsView>()
				.RegisterWidgetForWidgetViewModel<DeliveryPointJournalFilterViewModel, DeliveryPointJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<PaymentsJournalFilterViewModel, PaymentsFilterView>()
				.RegisterWidgetForWidgetViewModel<CallTaskFilterViewModel, CallTaskFilterView>()
				.RegisterWidgetForWidgetViewModel<BusinessTasksJournalFooterViewModel, BusinessTasksJournalFooterView>()
				.RegisterWidgetForWidgetViewModel<BusinessTasksJournalActionsViewModel, BusinessTasksJournalActionsView>()
				;

			DialogHelper.FilterWidgetResolver = ViewModelWidgetResolver.Instance;
		}

		static void ConfigureJournalColumnsConfigs()
		{
			JournalsColumnsConfigs.RegisterColumns();
		}

		static void CreateBaseConfig()
		{
			logger.Info("Настройка параметров базы...");
			//Увеличиваем таймоут
			QSMain.ConnectionString += ";ConnectionTimeout=120";

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
											.Dialect<NHibernate.Spatial.Dialect.MySQL57SpatialDialect>()
						 					.ConnectionString(QSMain.ConnectionString)
											.AdoNetBatchSize(100)
											.ShowSql()
											.FormatSql();

			// Настройка ORM
			OrmConfig.ConfigureOrm(db_config, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(QS.Project.HibernateMapping.UserBaseMap)),
				System.Reflection.Assembly.GetAssembly (typeof(HibernateMapping.OrganizationMap)),
				System.Reflection.Assembly.GetAssembly (typeof(Bank)),
				System.Reflection.Assembly.GetAssembly (typeof(HistoryMain)),
			},
								  (cnf) => cnf.DataBaseIntegration(
									  dbi => { dbi.BatchSize = 100; dbi.Batcher<MySqlClientBatchingBatcherFactory>(); }
									 ));
			#region Dialogs mapping

			OrmMain.ClassMappingList = new List<IOrmObjectMapping> {
				//Простые справочники
				OrmObjectMapping<CullingCategory>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<Nationality>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<Citizenship>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<Manufacturer>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<EquipmentColors>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<User>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<UserSettings>.Create().Dialog<UserSettingsView>(),
				OrmObjectMapping<FuelType>.Create().Dialog<FuelTypeDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Стоимость", x => x.Cost.ToString()).End(),
				OrmObjectMapping<MovementWagon>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				//Остальные справочники
				OrmObjectMapping<CarProxyDocument>.Create().Dialog<CarProxyDlg>(),
				OrmObjectMapping<M2ProxyDocument>.Create().Dialog<M2ProxyDlg>(),
				OrmObjectMapping<CommentTemplate>.Create().Dialog<CommentTemplateDlg>().DefaultTableView().SearchColumn("Шаблон комментария", x => x.Comment).End(),
				OrmObjectMapping<FineTemplate>.Create().Dialog<FineTemplateDlg>().DefaultTableView().SearchColumn("Шаблон комментария", x => x.Reason).End(),
				OrmObjectMapping<PremiumTemplate>.Create().Dialog<PremiumTemplateDlg>().DefaultTableView().SearchColumn("Шаблон комментария", x => x.Reason).End(),
				OrmObjectMapping<MeasurementUnits>.Create ().Dialog<MeasurementUnitsDlg>().DefaultTableView().SearchColumn("ОКЕИ", x => x.OKEI).SearchColumn("Название", x => x.Name).Column("Точность", x => x.Digits.ToString()).End(),
				OrmObjectMapping<Contact>.Create().Dialog <ContactDlg>()
					.DefaultTableView().SearchColumn("Фамилия", x => x.Surname).SearchColumn("Имя", x => x.Name).SearchColumn("Отчество", x => x.Patronymic).End(),
				OrmObjectMapping<Order>.Create().Dialog <OrderDlg>().PopupMenu(OrderPopupMenu.GetPopupMenu),
				OrmObjectMapping<UndeliveredOrder>.Create().Dialog<UndeliveredOrderDlg>(),
				OrmObjectMapping<Organization>.Create().Dialog<OrganizationDlg>().DefaultTableView().Column("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<DeliverySchedule>.Create().Dialog<DeliveryScheduleDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Время доставки", x => x.DeliveryTime).End(),
				OrmObjectMapping<ProductSpecification>.Create().Dialog<ProductSpecificationDlg>().DefaultTableView().SearchColumn("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<EquipmentType>.Create().Dialog<EquipmentTypeDlg>().DefaultTableView().Column("Название",equipmentType=>equipmentType.Name).End(),
				//Связанное с клиентом
				OrmObjectMapping<Proxy>.Create().Dialog<ProxyDlg>()
					.DefaultTableView().SearchColumn("Номер", x => x.Number).SearchColumn("С", x => x.StartDate.ToShortDateString()).SearchColumn("По", x => x.ExpirationDate.ToShortDateString()).End(),
				OrmObjectMapping<DeliveryPoint>.Create().Dialog<DeliveryPointDlg>().DefaultTableView().SearchColumn("ID", x => x.Id.ToString()).Column("Адрес", x => x.Title).End(),
				OrmObjectMapping<PaidRentPackage>.Create().Dialog<PaidRentPackageDlg>()
					.DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип оборудования", x => x.EquipmentType.Name).SearchColumn("Цена в сутки", x => CurrencyWorks.GetShortCurrencyString (x.PriceDaily)).SearchColumn("Цена в месяц", x => CurrencyWorks.GetShortCurrencyString (x.PriceMonthly)).End(),
				OrmObjectMapping<FreeRentPackage>.Create().Dialog<FreeRentPackageDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип оборудования", x => x.EquipmentType.Name).OrderAsc(x => x.Name).End(),
				OrmObjectMapping<FreeRentAgreement>.Create().Dialog<FreeRentAgreementDlg>(),
				OrmObjectMapping<DailyRentAgreement>.Create().Dialog<DailyRentAgreementDlg>(),
				OrmObjectMapping<NonfreeRentAgreement>.Create().Dialog<NonFreeRentAgreementDlg>(),
				OrmObjectMapping<SalesEquipmentAgreement>.Create().Dialog<EquipSalesAgreementDlg>(),
				OrmObjectMapping<WaterSalesAgreement>.Create().Dialog<WaterAgreementDlg>(),
				OrmObjectMapping<RepairAgreement>.Create().Dialog<RepairAgreementDlg>(),
				OrmObjectMapping<Counterparty>.Create().Dialog<CounterpartyDlg>().DefaultTableView().SearchColumn("Название", x => x.FullName).End(),
				OrmObjectMapping<Tag>.Create().Dialog<TagDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<CounterpartyContract>.Create().Dialog<CounterpartyContractDlg>(),
				OrmObjectMapping<DocTemplate>.Create().Dialog<DocTemplateDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип", x => x.TemplateType.GetEnumTitle()).End(),
				OrmObjectMapping<TransferOperationDocument>.Create().Dialog<TransferOperationDocumentDlg>(),
				//Справочники с фильтрами
				OrmObjectMapping<Equipment>.Create().Dialog<EquipmentDlg>().JournalFilter<EquipmentFilter>()
					.DefaultTableView().Column("Код", x => x.Id.ToString()).SearchColumn("Номенклатура", x => x.NomenclatureName).Column("Тип", x => x.Nomenclature.Type.Name).SearchColumn("Серийный номер", x => x.Serial).Column("Дата последней обработки", x => x.LastServiceDate.ToShortDateString ()).End(),
				//Логисткика
				OrmObjectMapping<RouteList>.Create().Dialog<RouteListCreateDlg>()
					.DefaultTableView().SearchColumn("Номер", x => x.Id.ToString()).Column("Дата", x => x.Date.ToShortDateString()).Column("Статус", x => x.Status.GetEnumTitle ()).SearchColumn("Водитель", x => String.Format ("{0} - {1}", x.Driver.FullName, x.Car.Title)).End(),
				OrmObjectMapping<RouteColumn>.Create().DefaultTableView().Column("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<DeliveryShift>.Create().Dialog<DeliveryShiftDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Диапазон времени", x => x.DeliveryTime).End(),
				OrmObjectMapping<DeliveryDaySchedule>.Create().Dialog<DeliveryDayScheduleDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				//Сервис
				OrmObjectMapping<ServiceClaim>.Create().Dialog<ServiceClaimDlg>().DefaultTableView().Column("Номер", x => x.Id.ToString()).Column("Тип", x => x.ServiceClaimType.GetEnumTitle()).Column("Оборудование", x => x.Equipment.Title).Column("Подмена", x => x.ReplacementEquipment != null ? "Да" : "Нет").Column("Точка доставки", x => x.DeliveryPoint.Title).End(),
				//Касса
				OrmObjectMapping<IncomeCategory>.Create ().Dialog<CashIncomeCategoryDlg>().DefaultTableView ().Column("Код", x => x.Id.ToString()).Column ("Название", e => e.Name).Column ("Тип документа", e => e.IncomeDocumentType.GetEnumTitle()).End (),
				OrmObjectMapping<ExpenseCategory>.Create ().Dialog<CashExpenseCategoryDlg>().DefaultTableView ().Column("Код", x => x.Id.ToString()).SearchColumn ("Название", e => e.Name).Column ("Тип документа", e => e.ExpenseDocumentType.GetEnumTitle()).TreeConfig(new RecursiveTreeConfig<ExpenseCategory>(x => x.Parent, x => x.Childs)).End (),
				OrmObjectMapping<Income>.Create ().Dialog<CashIncomeDlg> (),
				OrmObjectMapping<Expense>.Create ().Dialog<CashExpenseDlg> (),
				OrmObjectMapping<AdvanceReport>.Create ().Dialog<AdvanceReportDlg> (),
				OrmObjectMapping<Fine>.Create ().Dialog<FineDlg> (),
				OrmObjectMapping<Premium>.Create ().Dialog<PremiumDlg> (),
				OrmObjectMapping<IncomeCashTransferDocument>.Create().Dialog<IncomeCashTransferDlg>(),
				OrmObjectMapping<CommonCashTransferDocument>.Create().Dialog<CommonCashTransferDlg>(),
				//Банкинг
				OrmObjectMapping<AccountIncome>.Create (),
				OrmObjectMapping<AccountExpense>.Create (),
				//Склад
				OrmObjectMapping<Warehouse>.Create().Dialog<WarehouseDlg>().DefaultTableView().Column("Название", w=>w.Name).Column("В архиве", w=>w.IsArchive ? "Да":"").End(),
				OrmObjectMapping<RegradingOfGoodsTemplate>.Create().Dialog<RegradingOfGoodsTemplateDlg>().DefaultTableView().Column("Название", w=>w.Name).End()
			};

			#region Складские документы
			OrmMain.AddObjectDescription<IncomingInvoice>().Dialog<IncomingInvoiceDlg>();
			OrmMain.AddObjectDescription<IncomingWater>().Dialog<IncomingWaterDlg>();
			OrmMain.AddObjectDescription<WriteoffDocument>().Dialog<WriteoffDocumentDlg>();
			OrmMain.AddObjectDescription<InventoryDocument>().Dialog<InventoryDocumentDlg>();
			OrmMain.AddObjectDescription<ShiftChangeWarehouseDocument>().Dialog<ShiftChangeWarehouseDocumentDlg>();
			OrmMain.AddObjectDescription<RegradingOfGoodsDocument>().Dialog<RegradingOfGoodsDocumentDlg>();
			OrmMain.AddObjectDescription<SelfDeliveryDocument>().Dialog<SelfDeliveryDocumentDlg>();
			OrmMain.AddObjectDescription<CarLoadDocument>().Dialog<CarLoadDocumentDlg>();
			OrmMain.AddObjectDescription<CarUnloadDocument>().Dialog<CarUnloadDocumentDlg>();
			#endregion

			#region Goods
			OrmMain.AddObjectDescription<Nomenclature>().Dialog<NomenclatureDlg>().JournalFilter<NomenclatureFilter>().DefaultTableView().SearchColumn("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).Column("Тип", x => x.CategoryString).End();
			OrmMain.AddObjectDescription<Folder1c>().Dialog<Folder1cDlg>().DefaultTableView().SearchColumn("Код 1С", x => x.Code1c).SearchColumn("Название", x => x.Name).TreeConfig(new RecursiveTreeConfig<Folder1c>(x => x.Parent, x => x.Childs)).End();
			OrmMain.AddObjectDescription<ProductGroup>().Dialog<ProductGroupDlg>().EditPermision("can_edit_online_store").DefaultTableView().SearchColumn("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).TreeConfig(new RecursiveTreeConfig<ProductGroup>(x => x.Parent, x => x.Childs)).End();
			#endregion

			#region Простые справочники
			OrmMain.AddObjectDescription<DiscountReason>()
				   .DefaultTableView()
				   .SearchColumn("Название", x => x.Name)
				   .End();
			OrmMain.AddObjectDescription<GeographicGroup>()
				   .Dialog<GeographicGroupDlg>()
				   .DefaultTableView()
				   .SearchColumn("Название", x => x.Name)
				   .Column("Код", x => x.Id.ToString())
				   .End();
			OrmMain.AddObjectDescription<TariffZone>()
				   .DefaultTableView()
				   .SearchColumn("Номер", x => x.Id.ToString())
				   .SearchColumn("Название", x => x.Name)
				   .End();
			OrmMain.AddObjectDescription<NonReturnReason>()
				   .DefaultTableView()
				   .SearchColumn("Код", x => x.Id.ToString())
				   .SearchColumn("Название", x => x.Name)
				   .End();
			OrmMain.AddObjectDescription<PaymentFrom>()
				   .DefaultTableView()
				   .SearchColumn("Код", x => x.Id.ToString())
				   .SearchColumn("Название", x => x.Name)
				   .End();
			OrmMain.AddObjectDescription<Post>()
				   .DefaultTableView()
				   .SearchColumn("Код", x => x.Id.ToString())
				   .SearchColumn("Название", x => x.Name)
				   .End();

			#endregion

			#region неПростые справочники
			OrmMain.AddObjectDescription<Subdivision>().Dialog<SubdivisionDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Руководитель", x => x.Chief == null ? "" : x.Chief.ShortName).SearchColumn("Номер", x => x.Id.ToString()).TreeConfig(new RecursiveTreeConfig<Subdivision>(x => x.ParentSubdivision, x => x.ChildSubdivisions)).End();
			OrmMain.AddObjectDescription<TypeOfEntity>()
				   .Dialog<TypeOfEntityDlg>()
				   .DefaultTableView()
				   .SearchColumn("Тип документа", x => TypeOfEntityRepository.GetEntityNameByString(x.Type))
				   .SearchColumn("Название документа", x => x.CustomName)
				   .SearchColumn("Код", x => x.Id.ToString())
				   .Column("Активно", x => !x.IsActive ? "нет" : String.Empty)
				   .SearchColumn("Имя класса", x => x.Type)
				   .OrderAsc(x => x.CustomName)
				   .End();
			OrmMain.AddObjectDescription<Employee>().Dialog<EmployeeDlg>().DefaultTableView()
				   .Column("Код", x => x.Id.ToString())
				   .SearchColumn("Ф.И.О.", x => x.FullName)
				   .Column("Категория", x => x.Category.GetEnumTitle())
				   .OrderAsc(x => x.LastName).OrderAsc(x => x.Name).OrderAsc(x => x.Patronymic)
				   .End();
			OrmMain.AddObjectDescription<Trainee>().Dialog<TraineeDlg>().DefaultTableView()
				   .Column("Код", x => x.Id.ToString())
				   .SearchColumn("Ф.И.О.", x => x.FullName)
				   .OrderAsc(x => x.LastName).OrderAsc(x => x.Name).OrderAsc(x => x.Patronymic)
				   .End();
			OrmMain.AddObjectDescription<DeliveryPriceRule>().Dialog<DeliveryPriceRuleDlg>().DefaultTableView()
				   .Column("< 19л б.", x => x.Water19LCount.ToString())
				   .Column("< 6л б.", x => x.Water6LCount)
				   .Column("< 1,5л б.", x => x.Water1500mlCount)
				   .Column("< 0,6л б.", x => x.Water600mlCount)
				   .SearchColumn("Описание правила", x => x.ToString())
				   .End();
			OrmMain.AddObjectDescription<Car>().Dialog<CarsDlg>().DefaultTableView()
				   .SearchColumn("Код", x => x.Id.ToString())
				   .SearchColumn("Модель а/м", x => x.Model)
				   .SearchColumn("Гос. номер", x => x.RegistrationNumber)
				   .SearchColumn("Водитель", x => x.Driver != null ? x.Driver.FullName : string.Empty)
				   .End();
			OrmMain.AddObjectDescription<Certificate>().Dialog<CertificateDlg>().DefaultTableView()
				   .SearchColumn("Имя", x => x.Name)
				   .Column("Тип", x => x.TypeOfCertificate.GetEnumTitle())
				   .SearchColumn("Номер", x => x.Id.ToString())
				   .SearchColumn("Начало срока", x => x.StartDate.HasValue ? x.StartDate.Value.ToString("dd.MM.yyyy") : "Ошибка!")
				   .SearchColumn("Окончание срока", x => x.ExpirationDate.HasValue ? x.ExpirationDate.Value.ToString("dd.MM.yyyy") : "Бессрочно")
				   .Column("Архивный?", x => x.IsArchive ? "Да" : string.Empty)
				   .OrderAsc(x => x.IsArchive)
				   .OrderAsc(x => x.Id)
				   .End();
			OrmMain.AddObjectDescription<StoredImageResource>().Dialog<ImageLoaderDlg>().DefaultTableView()
				   .SearchColumn("Номер", x => x.Id.ToString())
				   .SearchColumn("Название", x => x.Name)
				   .End();
			OrmMain.AddObjectDescription<DeliveryPointCategory>().Dialog<DeliveryPointCategoryDlg>().DefaultTableView()
				   .SearchColumn("Код", x => x.Id.ToString())
				   .SearchColumn("Название", x => x.Name)
				   .Column("В архиве?", x => x.IsArchive ? "Да" : "Нет")
				   .OrderAsc(x => x.Name)
				   .End();
			OrmMain.AddObjectDescription<CounterpartyActivityKind>().Dialog<CounterpartyActivityKindDlg>().DefaultTableView()
				   .SearchColumn("Код", x => x.Id.ToString())
				   .SearchColumn("Название", x => x.Name)
				   .End();

			#endregion

			OrmMain.ClassMappingList.AddRange(QSBanks.QSBanksMain.GetModuleMaping());

			#endregion

			HistoryMain.Enable();
			TemplatePrinter.InitPrinter();
			ImagePrinter.InitPrinter();

			//Настройка ParentReference
			ParentReferenceConfig.AddActions(new ParentReferenceActions<Organization, Account> {
				AddNewChild = (o, a) => o.AddAccount(a)
			});
			ParentReferenceConfig.AddActions(new ParentReferenceActions<Counterparty, Account> {
				AddNewChild = (c, a) => c.AddAccount(a)
			});
			ParentReferenceConfig.AddActions(new ParentReferenceActions<Employee, Account> {
				AddNewChild = (c, a) => c.AddAccount(a)
			});
			ParentReferenceConfig.AddActions(new ParentReferenceActions<Trainee, Account> {
				AddNewChild = (c, a) => c.AddAccount(a)
			});
		}

		public static void CreateTempDir()
		{
			var userId = QSMain.User?.Id;

			if(userId == null)
				return;

			var tempVodUserPath = Path.Combine(Path.GetTempPath(), "Vodovoz", userId.ToString());
			DirectoryInfo dirInfo = new DirectoryInfo(tempVodUserPath);

			if(!dirInfo.Exists) 
				dirInfo.Create();
		}

		public static void ClearTempDir()
		{
			var userId = QSMain.User?.Id;

			if(userId == null)
				return;

			var tempVodUserPath = Path.Combine(Path.GetTempPath(), "Vodovoz", userId.ToString());
			DirectoryInfo dirInfo = new DirectoryInfo(tempVodUserPath);

			if(dirInfo.Exists) 
			{
				foreach(FileInfo file in dirInfo.EnumerateFiles()) {
					file.Delete();
				}
				foreach(DirectoryInfo dir in dirInfo.EnumerateDirectories()) {
					dir.Delete(true);
				}

				dirInfo.Delete();
			}
		}
	}
}
