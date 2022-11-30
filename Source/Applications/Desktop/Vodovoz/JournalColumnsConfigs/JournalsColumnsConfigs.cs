using Gamma.Binding;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using Gtk;
using QS.Journal.GtkUI;
using QSProjectsLib;
using System;
using System.Globalization;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.Domain.Proposal;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.JournalNodes;
using Vodovoz.Journals;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using Vodovoz.Journals.Nodes.Rent;
using Vodovoz.JournalViewModels;
using Vodovoz.JournalViewModels.Suppliers;
using Vodovoz.Representations;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints.ComplaintResults;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes.Flyers;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using Vodovoz.ViewModels.Journals.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints.ComplaintResults;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Flyers;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.HistoryTrace;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Proposal;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;
using Vodovoz.ViewModels.Journals.JournalViewModels.Users;
using Vodovoz.ViewModels.Journals.JournalViewModels.Retail;
using Vodovoz.ViewModels.Journals.JournalViewModels.Roboats;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.Journals.JournalViewModels.Security;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Journals.Nodes.Cash;
using DebtorJournalNode = Vodovoz.ViewModels.Journals.JournalNodes.DebtorJournalNode;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	public static class JournalsColumnsConfigs
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorRed = new Color(0xfe, 0x5c, 0x5c);
		private static readonly Color _colorPink = new Color(0xff, 0xc0, 0xc0);
		private static readonly Color _colorWhite = new Color(0xff, 0xff, 0xff);
		private static readonly Color _colorLightGrey = new Color(0xcc, 0xcc, 0xcc);
		private static readonly Color _colorDarkGrey = new Color(0x80, 0x80, 0x80);
		private static readonly Color _colorLightGreen = new Color(0xc0, 0xff, 0xc0);
		private static readonly Color _colorBlue = new Color(0x00, 0x18, 0xf9);
		private static readonly Color _colorBabyBlue = new Color(0x89, 0xcf, 0xef);

		public static void RegisterColumns()
		{
			TreeViewColumnsConfigFactory.Register<SalaryByEmployeeJournalViewModel>(
				() => FluentColumnsConfig<EmployeeJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Ф.И.О.").AddTextRenderer(node => node.FullName)
					.AddColumn("Категория").AddEnumRenderer(node => node.EmpCatEnum)
					.AddColumn("Статус").AddEnumRenderer(node => node.Status)
					.AddColumn("Подразделение").AddTextRenderer(node => node.SubdivisionTitle)
					.AddColumn("Баланс").AddNumericRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Balance)).Digits(2)
					.AddColumn("Комментарий по сотруднику").AddTextRenderer(node => node.EmployeeComment)
					.AddColumn("")
					.Finish()
			);

			//WarehouseJournalViewModel
			TreeViewColumnsConfigFactory.Register<WarehouseJournalViewModel>(
				() => FluentColumnsConfig<WarehouseJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("")
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<NomenclatureBalanceByStockJournalViewModel>(
				() => FluentColumnsConfig<NomenclatureBalanceByStockJournalNode>.Create()
					.AddColumn("Склад").AddTextRenderer(node => node.WarehouseName)
					.AddColumn("Кол-во").AddTextRenderer(node => $"{node.NomenclatureAmount:N0}")
					.AddColumn("")
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<DiscountReasonJournalViewModel>(
				() => FluentColumnsConfig<DiscountReasonJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("В архиве?").AddTextRenderer(node => node.IsArchive ? "Да" : "")
					.AddColumn("")
					.Finish()
			);

			//CarManufacturerJournalViewModel
			TreeViewColumnsConfigFactory.Register<CarManufacturerJournalViewModel>(
				() => FluentColumnsConfig<CarManufacturerJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Название").AddTextRenderer(node => node.Title)
					.AddColumn("")
					.Finish()
			);

			//DistrictsSetJournalViewModel
			TreeViewColumnsConfigFactory.Register<DistrictsSetJournalViewModel>(
				() => FluentColumnsConfig<DistrictsSetJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("Статус").AddTextRenderer(node => node.Status.GetEnumTitle())
					.AddColumn("Автор").AddTextRenderer(node => node.Author)
					.AddColumn("Дата создания").AddTextRenderer(node => node.DateCreated.Date.ToString("d")).XAlign(0.5f)
					.AddColumn("Дата активации").AddTextRenderer(node => node.DateActivated != null ? node.DateActivated.Value.Date.ToString("d") : "-").XAlign(0.5f)
					.AddColumn("Дата закрытия").AddTextRenderer(node => node.DateClosed != null ? node.DateClosed.Value.Date.ToString("d") : "-").XAlign(0.5f)
					.AddColumn("Комментарий").AddTextRenderer(node => node.Comment).WrapMode(WrapMode.WordChar).WrapWidth(500).XAlign(0.5f)
					.AddColumn("")
					.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.Status == DistrictsSetStatus.Closed ? _colorDarkGrey : _colorBlack)
					.Finish()
			);

			//DistrictJournalViewModel
			TreeViewColumnsConfigFactory.Register<DistrictJournalViewModel>(
				() => FluentColumnsConfig<DistrictJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("Зарплатный район").AddTextRenderer(node => node.WageDistrict)
					.AddColumn("Статус версии районов").AddTextRenderer(node => node.DistrictsSetStatus.GetEnumTitle())
					.AddColumn("Код версии").AddNumericRenderer(node => node.DistrictsSetId)
					.AddColumn("")
					.Finish()
			);

			//OrderJournalViewModel
			TreeViewColumnsConfigFactory.Register<OrderJournalViewModel>(
				() => FluentColumnsConfig<OrderJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Дата").AddTextRenderer(node => node.Date != null ? ((DateTime)node.Date).ToString("d") : String.Empty)
					.AddColumn("Автор").AddTextRenderer(node => node.Author)
					.AddColumn("Время").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
					.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
					.AddColumn("Тип").AddTextRenderer(node => node.ViewType)
						.WrapMode(WrapMode.WordChar)
						.WrapWidth(100)
					.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
					.AddColumn("Кол-во с/о").AddTextRenderer(node => $"{node.SanitisationAmount:N0}")
					.AddColumn("Клиент").AddTextRenderer(node => node.Counterparty)
					.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
					.AddColumn("Статус оплаты").AddTextRenderer(x =>
						(x.OrderPaymentStatus != OrderPaymentStatus.None) ? x.OrderPaymentStatus.GetEnumTitle() : "")
					.AddColumn("Район доставки").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DistrictName)
					.AddColumn("Адрес").AddTextRenderer(node => node.Address)
					.AddColumn("Изменил").AddTextRenderer(node => node.LastEditor)
					.AddColumn("Послед. изменения").AddTextRenderer(node =>
						node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString(CultureInfo.CurrentCulture) : string.Empty)
					.AddColumn("Номер звонка").AddTextRenderer(node => node.DriverCallId.ToString())
					.AddColumn("OnLine заказ №").AddTextRenderer(node => node.OnLineNumber)
					.AddColumn("Номер заказа интернет-магазина").AddTextRenderer(node => node.EShopNumber)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//OrderForRouteListJournalViewModel
			TreeViewColumnsConfigFactory.Register<OrderForRouteListJournalViewModel>(
				() => FluentColumnsConfig<OrderForRouteListJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Район доставки").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DistrictName)
					.AddColumn("Адрес").AddTextRenderer(node => node.Address)
					.AddColumn("Время").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
					.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
					.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
					.AddColumn("Кол-во с/о").AddTextRenderer(node => $"{node.SanitisationAmount:N0}")
					.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
					.AddColumn("Клиент").AddTextRenderer(node => node.Counterparty)
					.AddColumn("Автор").AddTextRenderer(node => node.Author)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//OrderForMovDocJournalViewModel
			TreeViewColumnsConfigFactory.Register<OrderForMovDocJournalViewModel>(
				() => FluentColumnsConfig<OrderForMovDocJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
					.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
					.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
					.AddColumn("Клиент")
						.AddTextRenderer(node => node.Counterparty)
						.WrapMode(WrapMode.WordChar)
						.WrapWidth(400)
					.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
					.AddColumn("OnLine заказ №").AddTextRenderer(node => node.OnLineNumber)
					.AddColumn("Номер заказа ИМ").AddTextRenderer(node => node.EShopNumber)
					.AddColumn("Адрес").AddTextRenderer(node => node.Address)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//DebtorsJournalViewModel
			TreeViewColumnsConfigFactory.Register<DebtorsJournalViewModel>(
				() => FluentColumnsConfig<DebtorJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(x => x.AddressId > 0 ? x.AddressId.ToString() : x.ClientId.ToString())
					.AddColumn("Клиент").AddTextRenderer(node => node.ClientName)
					.AddColumn("Адрес").AddTextRenderer(node => node.Address)
					.AddColumn("Кол-во точек доставки").AddTextRenderer(node => node.CountOfDeliveryPoint.ToString())
					.AddColumn("ОПФ").AddTextRenderer(node => node.OPF.GetEnumTitle())
					.AddColumn("Последний заказ по адресу").AddTextRenderer(node => node.LastOrderDate != null ? node.LastOrderDate.Value.ToString("dd / MM / yyyy") : string.Empty)
					.AddColumn("Кол-во отгруженных в последнюю реализацию бутылей").AddNumericRenderer(node => node.LastOrderBottles)
					.AddColumn("Долг по таре (по адресу)").AddNumericRenderer(node => node.DebtByAddress)
					.AddColumn("Долг по таре (по клиенту)").AddNumericRenderer(node => node.DebtByClient)
					.AddColumn("Ввод остат.").AddTextRenderer(node => node.IsResidueExist)
					.AddColumn("Резерв").AddNumericRenderer(node => node.Reserve)
					.RowCells().AddSetter<CellRendererText>((CellRendererText c, DebtorJournalNode n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//CounterpartyJournalViewModel
			TreeViewColumnsConfigFactory.Register<CounterpartyJournalViewModel>(
				() => FluentColumnsConfig<CounterpartyJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
					.AddColumn("Вн.номер").AddTextRenderer(x => x.InternalId.ToString())
					.AddColumn("Тег").AddTextRenderer(x => x.Tags, useMarkup: true)
					.AddColumn("Контрагент").AddTextRenderer(node => node.Name).WrapWidth(450).WrapMode(WrapMode.WordChar)
					.AddColumn("Телефоны").AddTextRenderer(x => x.Phones)
					.AddColumn("ИНН").AddTextRenderer(x => x.INN)
					.AddColumn("Договора").AddTextRenderer(x => x.Contracts)
					.AddColumn("Точки доставки").AddTextRenderer(x => x.Addresses)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//SelfDeliveriesJournalViewModel
			TreeViewColumnsConfigFactory.Register<SelfDeliveriesJournalViewModel>(
				() => FluentColumnsConfig<SelfDeliveryJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
					.AddColumn("Автор").AddTextRenderer(node => node.Author)
					.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
					.AddColumn("Тип оплаты").AddTextRenderer(node => node.PaymentTypeEnum.GetEnumTitle())
					.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
					.AddColumn("Клиент").AddTextRenderer(node => node.Counterparty)
					.AddColumn("Вариант оплаты").AddTextRenderer(node => node.PayOption)
					.AddColumn("Сумма безнал").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.OrderCashlessSumTotal))
					.AddColumn("Сумма нал").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.OrderCashSumTotal))
					.AddColumn("Из них возврат").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.OrderReturnSum))
					.AddColumn("Касса приход").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.CashPaid))
					.AddColumn("Касса возврат").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.CashReturn))
					.AddColumn("Касса итог").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.CashTotal))
					.AddColumn("Расхождение по нал.").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.TotalCashDiff))
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//ResidueJournalViewModel
			TreeViewColumnsConfigFactory.Register<ResidueJournalViewModel>(
				() => FluentColumnsConfig<ResidueJournalNode>.Create()
					.AddColumn("Документ").AddTextRenderer(node => $"Ввод остатков №{node.Id}").SearchHighlight()
					.AddColumn("Дата").AddTextRenderer(node => node.DateString)
					.AddColumn("Контрагент").AddTextRenderer(node => node.Counterparty)
					.AddColumn("Точка доставки").AddTextRenderer(node => node.DeliveryPoint)
					.AddColumn("Автор").AddTextRenderer(node => node.Author)
					.AddColumn("Изменил").AddTextRenderer(node => node.LastEditor)
					.AddColumn("Послед. изменения").AddTextRenderer(node =>
						node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString(CultureInfo.CurrentCulture) : string.Empty)
					.Finish()
			);

			//ClientCameFromFilterViewModel
			TreeViewColumnsConfigFactory.Register<ClientCameFromJournalViewModel>(
				() => FluentColumnsConfig<ClientCameFromJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(n => n.Id.ToString())
						.AddColumn("Название").AddTextRenderer(n => n.Name)
						.AddColumn("В архиве").AddTextRenderer(n => n.IsArchive ? "Да" : "Нет")
						.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
						.Finish()
			);

			//CarModelJournalViewModel
			TreeViewColumnsConfigFactory.Register<CarModelJournalViewModel>(
				() => FluentColumnsConfig<CarModelJournalNode>.Create()
					.AddColumn("Код").HeaderAlignment(0.5f).AddTextRenderer(n => n.Id.ToString()).XAlign(0.5f)
					.AddColumn("Производитель").HeaderAlignment(0.5f).AddTextRenderer(n => n.ManufactererName).XAlign(0.5f)
					.AddColumn("Название").HeaderAlignment(0.5f).AddTextRenderer(n => n.Name).XAlign(0.5f)
					.AddColumn("Тип").HeaderAlignment(0.5f).AddTextRenderer(n => n.TypeOfUse.GetEnumTitle()).XAlign(0.5f)
					.AddColumn("")
					.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
					.Finish()
			);

			//ComplaintsJournalViewModel
			TreeViewColumnsConfigFactory.Register<ComplaintsJournalViewModel>(
				() => FluentColumnsConfig<ComplaintJournalNode>.Create()
					.AddColumn("№ п/п").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.SequenceNumber.ToString())
						.XAlign(0.5f)
					.AddColumn("№ рекламации").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Id.ToString())
						.XAlign(0.5f)
					.AddColumn("Тип").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.TypeString)
						.XAlign(0.5f)
					.AddColumn("Дата").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.DateString)
						.XAlign(0.5f)
					.AddColumn("Время").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.TimeString)
						.XAlign(0.5f)
					.AddColumn("Статус").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.StatusString)
						.XAlign(0.5f)
					.AddColumn("В работе у").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.WorkInSubdivision)
						.XAlign(0f)
					.AddColumn("Дата план.\nзавершения").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.PlannedCompletionDate)
						.XAlign(0.5f)
					.AddColumn("Клиент и адрес").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ClientNameWithAddress)
						.WrapWidth(300).WrapMode(WrapMode.WordChar)
						.XAlign(0f)
					.AddColumn("Ответственный").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Guilties)
						.XAlign(0f)
					.AddColumn("Проблема").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ComplaintText)
						.WrapWidth(450).WrapMode(WrapMode.WordChar)
						.XAlign(0f)
					.AddColumn("Объект рекламации").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ComplaintObjectString)
						.XAlign(0.5f)
					.AddColumn("Вид рекламации").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ComplaintKindString)
						.XAlign(0.5f)
					.AddColumn("Автор").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Author)
						.XAlign(0f)
					.AddColumn("Штрафы").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Fines)
						.XAlign(0.5f)
					.AddColumn("Результат").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ResultText)
						.WrapWidth(450).WrapMode(WrapMode.WordChar)
						.XAlign(0f)
					.AddColumn("Дата факт.\nзавершения").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ActualCompletionDateString)
						.XAlign(0.5f)
					.AddColumn("Дни").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.DaysInWork)
						.XAlign(0.5f)
					.AddColumn("Мероприятия").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ArrangementText)
						.XAlign(0f)
					.AddColumn("Результат по клиенту").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ResultOfCounterparty)
						.XAlign(0f)
					.AddColumn("Результат по сотруднику").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ResultOfEmployees)
						.XAlign(0f)
					.RowCells()
					.AddSetter<CellRenderer>(
						(cell, node) => {
							var color = _colorWhite;
							if(node.Status != Domain.Complaints.ComplaintStatuses.Closed && node.LastPlannedCompletionDate.Date < DateTime.Today)
							{
								color = _colorPink;
							}
							cell.CellBackgroundGdk = color;
						}
					)
					.Finish()
			);

			//SubdivisionsJournalViewModel
			TreeViewColumnsConfigFactory.Register<SubdivisionsJournalViewModel>(
				() => FluentColumnsConfig<SubdivisionJournalNode>.Create()
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("Руководитель").AddTextRenderer(node => node.ChiefName)
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
					.Finish()
			);

			//EmployeePostsJournalViewModel
			TreeViewColumnsConfigFactory.Register<EmployeePostsJournalViewModel>(
				() => FluentColumnsConfig<EmployeePostJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Название").AddTextRenderer(node => node.EmployeePostName)
					.Finish()
			);

			//FinesJournalViewModel
			TreeViewColumnsConfigFactory.Register<FinesJournalViewModel>(
				() => FluentColumnsConfig<FineJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
					.AddColumn("Сотудники").AddTextRenderer(node => node.EmployeesName)
					.AddColumn("Сумма штрафа").AddTextRenderer(node => node.FineSumm.ToString(CultureInfo.CurrentCulture))
					.AddColumn("Причина штрафа").AddTextRenderer(node => node.FineReason)
					.Finish()
			);

			//NomenclaturesJournalViewModel
			TreeViewColumnsConfigFactory.Register<NomenclaturesJournalViewModel>(
				() => FluentColumnsConfig<NomenclatureJournalNode>.Create()
					.AddColumn("Код")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Номенклатура")
						.AddTextRenderer(node => node.Name)
					.AddColumn("Категория")
						.AddTextRenderer(node => node.Category.GetEnumTitle())
					.AddColumn("Кол-во")
						.AddTextRenderer(node => node.InStockText)
					.AddColumn("Зарезервировано")
						.AddTextRenderer(node => node.ReservedText)
					.AddColumn("Доступно")
						.AddTextRenderer(node => node.AvailableText)
						.AddSetter((cell, node) => cell.ForegroundGdk = node.Available > 0 ? _colorBlack : _colorRed)
					.AddColumn("Код в ИМ")
						.AddTextRenderer(node => node.OnlineStoreExternalId)
					.Finish()
			);

			//NomenclatureStockBalanceJournalViewModel
			TreeViewColumnsConfigFactory.Register<NomenclatureStockBalanceJournalViewModel>(
				() => FluentColumnsConfig<NomenclatureStockJournalNode>.Create()
					.AddColumn("Код").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Номенклатура").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.NomenclatureName)
					.AddColumn("Кол-во").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.AmountText).XAlign(0.5f)
					.AddColumn("Мин кол-во\n на складе").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.MinCountText).XAlign(0.5f)
					.AddColumn("Разница").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.DiffCountText).XAlign(0.5f)
					.AddColumn("")
					.RowCells().AddSetter<CellRendererText>((c, n) => {
						Color color = new Color(0, 0, 0);
						if(n.StockAmount < 0) {
							color = new Color(255, 30, 30);
						}
						c.ForegroundGdk = color;
					})
					.Finish()
			);

			//WaterJournalViewModel
			TreeViewColumnsConfigFactory.Register<WaterJournalViewModel>(
				() => FluentColumnsConfig<WaterJournalNode>.Create()
					.AddColumn("Код")
					.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Номенклатура")
					.AddTextRenderer(node => node.Name)
					.Finish()
			);

			//RequestsToSuppliersJournalViewModel
			TreeViewColumnsConfigFactory.Register<RequestsToSuppliersJournalViewModel>(
				() => FluentColumnsConfig<RequestToSupplierJournalNode>.Create()
					.AddColumn("Номер")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
					.AddColumn("Статус")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Status.GetEnumTitle())
					.AddColumn("Название")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Name)
					.AddColumn("Дата")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Created.ToString("G"))
					.AddColumn("Автор")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Author)
					.AddColumn("")
					.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//WageDistrictsJournalViewModel
			TreeViewColumnsConfigFactory.Register<WageDistrictsJournalViewModel>(
				() => FluentColumnsConfig<WageDistrictJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
					.AddColumn("Название")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Name)
					.AddColumn("В архиве?")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.IsArchiveString)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//WageDistrictLevelRatesJournalViewModel
			TreeViewColumnsConfigFactory.Register<WageDistrictLevelRatesJournalViewModel>(
				() => FluentColumnsConfig<WageDistrictLevelRatesJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
					.AddColumn("Название")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Name)
					.AddColumn("По умолчанию для новых сотрудников (Найм)")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.IsDefaultLevelString)
					.AddColumn("По умолчанию для новых сотрудников (Наши авто)")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.IsDefaultLevelOurCarsString)
					.AddColumn("По умолчанию для новых сотрудников (Для авто в раскате)")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.IsDefaultLevelRaskatCarsString)
					.AddColumn("В архиве?")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.IsArchiveString)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//SalesPlanJournalViewModel
			TreeViewColumnsConfigFactory.Register<SalesPlanJournalViewModel>(
				() => FluentColumnsConfig<SalesPlanJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
					.AddColumn("Название")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Name)
					.AddColumn("Описание")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Title)
					.AddColumn("В архиве?")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.IsArchiveString)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//EmployeesJournalViewModel
			TreeViewColumnsConfigFactory.Register<EmployeesJournalViewModel>(
				() => FluentColumnsConfig<EmployeeJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
					.AddColumn("Ф.И.О.")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.FullName)
						.WrapMode(WrapMode.WordChar)
						.WrapWidth(600)
					.AddColumn("Категория")
						.MinWidth(200)
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.EmpCatEnum.GetEnumTitle())
					.AddColumn("Статус")
						.AddEnumRenderer(n => n.Status)
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//PromotionalSetJournalViewModel
			TreeViewColumnsConfigFactory.Register<PromotionalSetsJournalViewModel>(
				() => FluentColumnsConfig<PromotionalSetJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
					.AddColumn("Название")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Name)
					.AddColumn("Основание скидки")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.PromoSetDiscountReasonName)
					.AddColumn("В архиве?")
						.HeaderAlignment(0.5f)
						.AddTextRenderer()
						.AddSetter((c, n) => c.Text = n.IsArchive? "Да" : String.Empty)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
					.Finish()
			);

			//CarJournalViewModel
			TreeViewColumnsConfigFactory.Register<CarJournalViewModel>(
				() => FluentColumnsConfig<CarJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
					.AddColumn("Производитель").AddTextRenderer(x => x.ManufacturerName).WrapWidth(300).WrapMode(WrapMode.WordChar)
					.AddColumn("Модель").AddTextRenderer(x => x.ModelName).WrapWidth(300).WrapMode(WrapMode.WordChar)
					.AddColumn("Гос. номер").AddTextRenderer(x => x.RegistrationNumber)
					.AddColumn("Водитель").AddTextRenderer(x => x.DriverName)
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<MovementWagonJournalViewModel>(
				() => FluentColumnsConfig<MovementWagonJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
					.AddColumn("Название").AddTextRenderer(x => x.Name)
					.Finish()
			);

			//DriverComplaintReason
			TreeViewColumnsConfigFactory.Register<DriverComplaintReasonsJournalViewModel>(
				() => FluentColumnsConfig<DriverComplaintReasonJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
					.AddColumn("Название").AddTextRenderer(x => x.Name)
					.AddColumn("Популярная").AddToggleRenderer(x => x.IsPopular).Editing(false)
					.AddColumn("")
					.Finish()
			);

			//PhoneTypeJournalViewModel
			TreeViewColumnsConfigFactory.Register<PhoneTypeJournalViewModel>(
				() => FluentColumnsConfig<PhoneTypeJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
					.AddColumn("Название")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Name)
					.AddColumn("Назначение")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.PhonePurpose.GetEnumTitle())
					.AddColumn("")
					.Finish()
			);

			//EmailTypeJournalViewModel
			TreeViewColumnsConfigFactory.Register<EmailTypeJournalViewModel>(
				() => FluentColumnsConfig<EmailTypeJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
					.AddColumn("Название")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Name)
					.AddColumn("Назначение")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.EmailPurpose.GetEnumTitle())
					.AddColumn("")
					.Finish()
			);

			//DeliveryPointJournalViewModel
			TreeViewColumnsConfigFactory.Register<DeliveryPointJournalViewModel>(
				() => FluentColumnsConfig<DeliveryPointJournalNode>.Create()
					.AddColumn("ФИАС").AddTextRenderer(x => x.FoundInFias ? "Да" : "")
					.AddColumn("Испр.").AddTextRenderer(x => x.FixedInFias ? "Да" : "")
					.AddColumn("Адрес")
						.AddTextRenderer(node => node.CompiledAddress)
						.WrapMode(WrapMode.WordChar)
						.WrapWidth(1000)
					.AddColumn("Адрес из 1с").AddTextRenderer(x => x.Address1c)
					.AddColumn("Клиент").AddTextRenderer(x => x.Counterparty)
					.AddColumn("Номер").AddTextRenderer(x => x.IdString)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<DeliveryPointByClientJournalViewModel>(
				() => FluentColumnsConfig<DeliveryPointByClientJournalNode>.Create()
					.AddColumn("Адрес")
						.AddTextRenderer(node => node.CompiledAddress)
						.WrapMode(WrapMode.WordChar)
						.WrapWidth(1000)
					.AddColumn("Номер").AddTextRenderer(x => x.Id.ToString())
					.AddColumn("")
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//PaymentJournalViewModel
			TreeViewColumnsConfigFactory.Register<PaymentsJournalViewModel>(
				() => FluentColumnsConfig<PaymentJournalNode>.Create()
					.AddColumn("№")
						.AddTextRenderer(x => x.PaymentNum.ToString())
					.AddColumn("Дата")
						.AddTextRenderer(x => x.Date.ToShortDateString())
					.AddColumn("Cумма")
						.AddTextRenderer(x => x.Total.ToString())
					.AddColumn("Заказы")
						.AddTextRenderer(x => x.Orders)
					.AddColumn("Плательщик")
						.AddTextRenderer(x => x.PayerName)
						.WrapWidth(450)
						.WrapMode(WrapMode.WordChar)
					.AddColumn("Контрагент")
						.AddTextRenderer(x => x.CounterpartyName)
						.WrapWidth(450)
						.WrapMode(WrapMode.WordChar)
					.AddColumn("Получатель")
						.AddTextRenderer(x => x.Organization)
					.AddColumn("Назначение платежа")
						.AddTextRenderer(x => x.PaymentPurpose)
						.WrapWidth(600)
						.WrapMode(WrapMode.WordChar)
					.AddColumn("Категория дохода/расхода")
						.AddTextRenderer(x => x.ProfitCategory)
						.XAlign(0.5f)
					.AddColumn("Создан вручную?")
						.AddToggleRenderer(x => x.IsManualCreated)
						.Editing(false)
					.AddColumn("Нераспределенная сумма")
						.AddNumericRenderer(x => x.UnAllocatedSum)
						.Digits(2)
					.AddColumn("")
					.RowCells().AddSetter<CellRenderer>(
						(c, n) => {
							var color = _colorWhite;

							if(n.Status == PaymentState.undistributed)
							{
								color = _colorPink;
							}
							if(n.Status == PaymentState.distributed)
							{
								color = _colorLightGreen;
							}
							if(n.Status == PaymentState.Cancelled)
							{
								color = _colorLightGrey;
							}

							c.CellBackgroundGdk = color;
						})
					.Finish()
			);

			//BusinessTasksJournalViewModel
			TreeViewColumnsConfigFactory.Register<BusinessTasksJournalViewModel>(
				() => FluentColumnsConfig<BusinessTaskJournalNode>.Create()
					.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
					/*.AddColumn("Срочность").AddPixbufRenderer(node =>
						node.ImportanceDegree == ImportanceDegreeType.Important && !node.IsTaskComplete ? img : emptyImg)*/
					.AddColumn("Статус").AddEnumRenderer(node => node.TaskStatus)
					.AddColumn("Клиент").AddTextRenderer(node => node.ClientName ?? string.Empty)
					.AddColumn("Адрес").AddTextRenderer(node => node.AddressName ?? "Самовывоз")
					.AddColumn("Долг по адресу").AddTextRenderer(node => node.DebtByAddress.ToString()).XAlign(0.5f)
					.AddColumn("Долг по клиенту").AddTextRenderer(node => node.DebtByClient.ToString()).XAlign(0.5f)
					.AddColumn("Телефоны").AddTextRenderer(node => node.DeliveryPointPhones == "+7" ? string.Empty : node.DeliveryPointPhones)
						.WrapMode(WrapMode.WordChar)
					.AddColumn("Ответственный").AddTextRenderer(node => node.AssignedEmployeeName ?? string.Empty)
					.AddColumn("Выполнить до").AddTextRenderer(node => node.Deadline.ToString("dd / MM / yyyy  HH:mm"))
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//ReturnTareReasonCategoriesJournalViewModel
			TreeViewColumnsConfigFactory.Register<ReturnTareReasonCategoriesJournalViewModel>(
				() => FluentColumnsConfig<ReturnTareReasonCategoriesJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
						.XAlign(0.5f)
					.AddColumn("Категория причины")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Name)
						.XAlign(0.5f)
					.AddColumn("")
					.Finish()
			);

			//ReturnTareReasonsJournalViewModel
			TreeViewColumnsConfigFactory.Register<ReturnTareReasonsJournalViewModel>(
				() => FluentColumnsConfig<ReturnTareReasonsJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
						.XAlign(0.5f)
					.AddColumn("Причина")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Name)
						.XAlign(0.5f)
					.AddColumn("В архиве?")
						.HeaderAlignment(0.5f)
						.AddToggleRenderer(n => n.IsArchive)
						.Editing()
						.XAlign(0.5f)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
					.Finish()
			);

			//IncomeCategoryJournalViewModel
			TreeViewColumnsConfigFactory.Register<IncomeCategoryJournalViewModel>(
				() => FluentColumnsConfig<IncomeCategoryJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
						.XAlign(0.5f)
					.AddColumn("Уровень 1")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Level1)
						.XAlign(0.5f)
					.AddColumn("Уровень 2")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Level2)
						.XAlign(0.5f)
					.AddColumn("Уровень 3")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Level3)
						.XAlign(0.5f)
					.AddColumn("Уровень 4")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Level4)
						.XAlign(0.5f)
					.AddColumn("Уровень 5")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Level5)
						.XAlign(0.5f)
					.AddColumn("В архиве?")
						.HeaderAlignment(0.5f)
						.AddToggleRenderer(n => n.IsArchive)
						.Editing(false)
						.XAlign(0.5f)
					.AddColumn("Подразделение")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Subdivision)
						.XAlign(0.5f)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
						// .AddSetter<CellRendererText>((c, n) => c. = !n.isFiltered )
					.Finish()
			);

			//IncomeCategoryJournalViewModel
			TreeViewColumnsConfigFactory.Register<ExpenseCategoryJournalViewModel>(
				() => FluentColumnsConfig<ExpenseCategoryJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
						.XAlign(0.5f)
					.AddColumn("Уровень 1")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Level1)
						.XAlign(0.5f)
					.AddColumn("Уровень 2")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Level2)
						.XAlign(0.5f)
					.AddColumn("Уровень 3")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Level3)
						.XAlign(0.5f)
					.AddColumn("Уровень 4")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Level4)
						.XAlign(0.5f)
					.AddColumn("Уровень 5")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Level5)
						.XAlign(0.5f)
					.AddColumn("В архиве?")
						.HeaderAlignment(0.5f)
						.AddToggleRenderer(n => n.IsArchive)
						.Editing(false)
						.XAlign(0.5f)
					.AddColumn("Подразделение")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Subdivision)
						.XAlign(0.5f)
					.AddColumn("")
					.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
					.Finish()
			);

			//PayoutRequestsJournalViewModel
			TreeViewColumnsConfigFactory.Register<PayoutRequestsJournalViewModel>(
				() => FluentColumnsConfig<PayoutRequestJournalNode>.Create()
					.AddColumn("№")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
						.XAlign(0.5f)
					.AddColumn("Дата создания")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Date.ToShortDateString())
						.XAlign(0.5f)
					.AddColumn("Тип документа")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.PayoutRequestDocumentType.GetEnumTitle())
						.WrapWidth(155)
						.WrapMode(WrapMode.WordChar)
						.XAlign(0.5f)
					.AddColumn("Статус")
						.HeaderAlignment(0.5f)
						.AddEnumRenderer(n => n.PayoutRequestState)
						.XAlign(0.5f)
					.AddColumn("Автор")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n =>  n.Author )
						.XAlign(0.5f)
					.AddColumn("Подотчетное лицо /\r\n\tПоставщик")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => !string.IsNullOrWhiteSpace(n.AccountablePerson) ? n.AccountablePerson : n.CounterpartyName)
						.XAlign(0.5f)
					.AddColumn("Сумма")
						.HeaderAlignment(0.5f)
						.AddNumericRenderer(n => CurrencyWorks.GetShortCurrencyString(n.Sum))
						.XAlign(0.5f)
					.AddColumn("Основание")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Basis)
						.XAlign(0.5f)

					.AddColumn("")
					// .RowCells()
					// .AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? colorDarkGrey : colorBlack)
					.Finish()
			);

			//LateArrivalReasonsJournalViewModel
			TreeViewColumnsConfigFactory.Register<LateArrivalReasonsJournalViewModel>(
				() => FluentColumnsConfig<LateArrivalReasonsJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
						.XAlign(0.5f)
					.AddColumn("Причина")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Name)
						.XAlign(0.5f)
					.AddColumn("В архиве?")
						.HeaderAlignment(0.5f)
						.AddToggleRenderer(n => n.IsArchive)
						.Editing(false)
						.XAlign(0.5f)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
					.Finish()
			);

			//FuelDocumentsJournalViewModel
			TreeViewColumnsConfigFactory.Register<FuelDocumentsJournalViewModel>(
				() => FluentColumnsConfig<FuelDocumentJournalNode>.Create()
				.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Тип").AddTextRenderer(node => node.Title)
				.AddColumn("Дата").AddTextRenderer(node => node.CreationDate.ToShortDateString())
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.AddColumn("Сотрудник").AddTextRenderer(node => node.Employee)
				.AddColumn("Статус").AddTextRenderer(node => node.Status)
				.AddColumn("Литры").AddTextRenderer(node => node.Liters.ToString("0"))
				.AddColumn("Статья расх.").AddTextRenderer(node => node.ExpenseCategory)

				.AddColumn("Отправлено из").AddTextRenderer(node => node.SubdivisionFrom)
				.AddColumn("Время отпр.").AddTextRenderer(node => node.SendTime.HasValue ? node.SendTime.Value.ToShortDateString() : "")

				.AddColumn("Отправлено в").AddTextRenderer(node => node.SubdivisionTo)
				.AddColumn("Время принятия").AddTextRenderer(node => node.ReceiveTime.HasValue ? node.ReceiveTime.Value.ToShortDateString() : "")

				.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
				.Finish()
			);

			//FinancialDistrictsSetsJournalViewModel
			TreeViewColumnsConfigFactory.Register<FinancialDistrictsSetsJournalViewModel>(
				() => FluentColumnsConfig<FinancialDistrictsSetsJournalNode>.Create()
					.AddColumn("Код")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Название")
						.AddTextRenderer(node => node.Name)
					.AddColumn("Статус")
						.AddTextRenderer(node => node.Status.GetEnumTitle())
					.AddColumn("Автор")
						.AddTextRenderer(node => node.Author)
					.AddColumn("Дата создания")
						.AddTextRenderer(node => node.DateCreated.Date.ToString("d"))
						.XAlign(0.5f)
					.AddColumn("Дата активации")
						.AddTextRenderer(node => node.DateActivated != null ? node.DateActivated.Value.Date.ToString("d") : "-")
						.XAlign(0.5f)
					.AddColumn("Дата закрытия")
						.AddTextRenderer(node => node.DateClosed != null ? node.DateClosed.Value.Date.ToString("d") : "-")
						.XAlign(0.5f)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) =>
							c.ForegroundGdk = n.Status == DistrictsSetStatus.Closed ? _colorDarkGrey : _colorBlack)
					.Finish()
			);


			//SelectUserJournalViewModel
			TreeViewColumnsConfigFactory.Register<SelectUserJournalViewModel>(
				() => FluentColumnsConfig<UserJournalNode>.Create()
					.AddColumn("Код")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Имя")
						.AddTextRenderer(node => node.Name)
					.AddColumn("Логин")
						.AddTextRenderer(node => node.Login)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) =>
							c.ForegroundGdk = n.Deactivated ? _colorDarkGrey : _colorBlack)
					.Finish()
			);

			//UserJournalViewModel
			TreeViewColumnsConfigFactory.Register<UsersJournalViewModel>(
				() => FluentColumnsConfig<UserJournalNode>.Create()
					.AddColumn("Код")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Имя")
						.AddTextRenderer(node => node.Name)
					.AddColumn("Логин")
						.AddTextRenderer(node => node.Login)
					.AddColumn("Id сотрудника")
						.AddTextRenderer(node => node.EmployeeId.HasValue ? node.EmployeeId.ToString() : string.Empty)
					.AddColumn("ФИО сотрудника")
						.AddTextRenderer(node => node.EmployeeFIO)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) =>
						{
							if(n.Deactivated)
							{
								c.ForegroundGdk = n.IsAdmin ? _colorBabyBlue : _colorDarkGrey;
							}
							else
							{
								c.ForegroundGdk = n.IsAdmin ? _colorBlue : _colorBlack;
							}
						})
					.Finish()
			);

			//HistoryTraceObjectJournalViewModel
			TreeViewColumnsConfigFactory.Register<HistoryTraceObjectJournalViewModel>(
				() => FluentColumnsConfig<HistoryTraceObjectNode>.Create()
					.AddColumn("Имя")
						.AddTextRenderer(node => node.DisplayName)
					.AddColumn("Тип")
						.AddTextRenderer(node => node.ObjectType.ToString())
					.AddColumn("")
					.Finish()
			);

			//HistoryTracePropertyJournalViewModel
			TreeViewColumnsConfigFactory.Register<HistoryTracePropertyJournalViewModel>(
				() => FluentColumnsConfig<HistoryTracePropertyNode>.Create()
					.AddColumn("Имя")
						.AddTextRenderer(node => node.PropertyName)
					.AddColumn("Тип")
						.AddTextRenderer(node => node.PropertyPath)
					.AddColumn("")
					.Finish()
			);

			//ApplicationDevelopmentProposalsJournalViewModel
			TreeViewColumnsConfigFactory.Register<ApplicationDevelopmentProposalsJournalViewModel>(
				() => FluentColumnsConfig<ApplicationDevelopmentProposalsJournalNode>.Create()
					.AddColumn("Код")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Id.ToString())
						.XAlign(0.5f)
					.AddColumn("Дата создания")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.CreationDate.ToString(CultureInfo.CurrentCulture))
						.XAlign(0.5f)
					.AddColumn("Тема")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Title)
						.XAlign(0.5f)
					.AddColumn("Статус")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Status.GetEnumTitle())
						.XAlign(0.5f)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) =>
							c.ForegroundGdk = n.Status == ApplicationDevelopmentProposalStatus.Rejected ? _colorRed : _colorBlack)
					.Finish()
			);

			//RouteListWorkingJournalViewModel
			TreeViewColumnsConfigFactory.Register<RouteListWorkingJournalViewModel>(
				() => FluentColumnsConfig<RouteListJournalNode>.Create()
					.AddColumn("Номер")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Дата")
						.AddTextRenderer(node => node.Date.ToString("d"))
					.AddColumn("Смена")
						.AddTextRenderer(node => node.ShiftName)
					.AddColumn("Статус")
						.AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
					.AddColumn("Водитель и машина")
						.AddTextRenderer(node => node.DriverAndCar)
					.AddColumn("Сдается в кассу")
						.AddTextRenderer(node => node.ClosingSubdivision)
					.AddColumn("Комментарий ЛО")
						.AddTextRenderer(node => node.LogisticiansComment)
						.WrapWidth(300)
						.WrapMode(WrapMode.WordChar)
					.AddColumn("Комментарий по закрытию")
						.AddTextRenderer(node => node.ClosinComments)
						.WrapWidth(300)
						.WrapMode(WrapMode.WordChar)
					.AddColumn("Комментарий по водителю")
						.AddTextRenderer(node => node.DriverComment)
						.WrapWidth(300)
						.WrapMode(WrapMode.WordChar)
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.Foreground = n.NotFullyLoaded ? "Orange" : "Black")
					.Finish()
			);

			//RouteListJournalViewModel
			TreeViewColumnsConfigFactory.Register<RouteListJournalViewModel>(
				() => FluentColumnsConfig<RouteListJournalNode>.Create()
					.AddColumn("Номер")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Дата")
						.AddTextRenderer(node => node.Date.ToString("d"))
					.AddColumn("Смена")
						.AddTextRenderer(node => node.ShiftName)
					.AddColumn("Статус")
						.AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
					.AddColumn("Водитель и машина")
						.AddTextRenderer(node => node.DriverAndCar)
					.AddColumn("Сдается в кассу")
						.AddTextRenderer(node => node.ClosingSubdivision)
					.AddColumn("Комментарий ЛО")
						.AddTextRenderer(node => node.LogisticiansComment)
						.WrapWidth(300)
						.WrapMode(WrapMode.WordChar)
					.AddColumn("Комментарий по закрытию")
						.AddTextRenderer(node => node.ClosinComments)
						.WrapWidth(300)
						.WrapMode(WrapMode.WordChar)
					.AddColumn("Комментарий по водителю")
						.AddTextRenderer(node => node.DriverComment)
						.WrapWidth(300)
						.WrapMode(WrapMode.WordChar)
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.Foreground = n.NotFullyLoaded ? "Orange" : "Black")
					.Finish()
			);

			//RegisteredRMJournalViewModel
			TreeViewColumnsConfigFactory.Register<RegisteredRMJournalViewModel>(
				() => FluentColumnsConfig<RegisteredRMJournalNode>.Create()
					.AddColumn("Имя пользователя")
						.AddTextRenderer(node => node.Username)
					.AddColumn("Домен")
						.AddTextRenderer(node => node.Domain)
					.AddColumn("SID пользователя")
						.AddTextRenderer(node => node.SID)
					.AddColumn("")
					.Finish()
			);

			//DeliveryPointResponsiblePersonTypeJournalViewModel
			TreeViewColumnsConfigFactory.Register<DeliveryPointResponsiblePersonTypeJournalViewModel>(
				() => FluentColumnsConfig<DeliveryPointResponsiblePersonTypeJournalNode>.Create()
					.AddColumn("Номер")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Имя")
						.AddTextRenderer(node => node.Title)
					.AddColumn("")
					.Finish()
			);

			//DeliveryPointResponsiblePersonTypeJournalViewModel
			TreeViewColumnsConfigFactory.Register<SalesChannelJournalViewModel>(
				() => FluentColumnsConfig<SalesChannelJournalNode>.Create()
					.AddColumn("Номер")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Имя")
						.AddTextRenderer(node => node.Title)
					.AddColumn("")
					.Finish()
			);

			//RetailOrderJournalViewModel
			TreeViewColumnsConfigFactory.Register<RetailOrderJournalViewModel>(
				() => FluentColumnsConfig<RetailOrderJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Дата").AddTextRenderer(node => node.Date != null ? ((DateTime)node.Date).ToString("d") : String.Empty)
					.AddColumn("Автор").AddTextRenderer(node => node.Author)
					.AddColumn("Время").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
					.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
					.AddColumn("Тип").AddTextRenderer(node => node.ViewType)
						.WrapMode(WrapMode.WordChar)
						.WrapWidth(100)
					.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
					.AddColumn("Кол-во с/о").AddTextRenderer(node => $"{node.SanitisationAmount:N0}")
					.AddColumn("Клиент").AddTextRenderer(node => node.Counterparty)
					.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
					.AddColumn("Статус оплаты").AddTextRenderer(x =>
						(x.OrderPaymentStatus != OrderPaymentStatus.None) ? x.OrderPaymentStatus.GetEnumTitle() : "")
					.AddColumn("Район доставки").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DistrictName)
					.AddColumn("Адрес").AddTextRenderer(node => node.Address)
					.AddColumn("Изменил").AddTextRenderer(node => node.LastEditor)
					.AddColumn("Послед. изменения").AddTextRenderer(node =>
						node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString(CultureInfo.CurrentCulture) : string.Empty)
					.AddColumn("Номер звонка").AddTextRenderer(node => node.DriverCallId.ToString())
					.AddColumn("OnLine заказ №").AddTextRenderer(node => node.OnLineNumber)
					.AddColumn("Номер заказа интернет-магазина").AddTextRenderer(node => node.EShopNumber)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//RetailCounterpartyJournalViewModel
			TreeViewColumnsConfigFactory.Register<RetailCounterpartyJournalViewModel>(
				() => FluentColumnsConfig<RetailCounterpartyJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
					.AddColumn("Вн.номер").AddTextRenderer(x => x.InternalId.ToString())
					.AddColumn("Тег").AddTextRenderer(x => x.Tags, useMarkup: true)
					.AddColumn("Контрагент").AddTextRenderer(node => node.Name).WrapWidth(450).WrapMode(WrapMode.WordChar)
					.AddColumn("Телефоны").AddTextRenderer(x => x.Phones)
					.AddColumn("ИНН").AddTextRenderer(x => x.INN)
					.AddColumn("Договора").AddTextRenderer(x => x.Contracts)
					.AddColumn("Точки доставки").AddTextRenderer(x => x.Addresses)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//NomenclaturesPlanJournalViewModel
			TreeViewColumnsConfigFactory.Register<NomenclaturesPlanJournalViewModel>(
				() => FluentColumnsConfig<NomenclaturePlanJournalNode>.Create()
					.AddColumn("Код")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Номенклатура")
						.AddTextRenderer(node => node.Name)
					.AddColumn("Категория")
						.AddTextRenderer(node => node.Category.GetEnumTitle())
					.AddColumn("Код в ИМ")
						.AddTextRenderer(node => node.OnlineStoreExternalId)
					.AddColumn("План день")
						.AddTextRenderer(node => node.PlanDay.ToString())
					.AddColumn("План месяц")
						.AddTextRenderer(node => node.PlanMonth.ToString())
					.Finish()
			);

			//OrganizationCashTransferDocumentJournalViewModel
			TreeViewColumnsConfigFactory.Register<OrganizationCashTransferDocumentJournalViewModel>(
				() => FluentColumnsConfig<OrganizationCashTransferDocumentJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Дата создания").AddTextRenderer(node => node.DocumentDate.ToString("d"))
					.AddColumn("Автор").AddTextRenderer(node => node.Author)
					.AddColumn("Орг.откуда").AddTextRenderer(node => node.OrganizationFrom)
					.AddColumn("Орг.куда").AddTextRenderer(node => node.OrganizationTo)
					.AddColumn("Сумма").AddTextRenderer(node => node.TransferedSum.ToString(CultureInfo.CurrentCulture))
					.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
					.Finish()
			);

			//PremiumJournalViewModel
			TreeViewColumnsConfigFactory.Register<PremiumJournalViewModel>(
				() => FluentColumnsConfig<PremiumJournalNode>.Create()
					.AddColumn("Номер").AddNumericRenderer(node => node.Id)
					.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
					.AddColumn("Тип").AddTextRenderer(node => node.ViewType)
						.WrapMode(WrapMode.WordChar)
						.WrapWidth(100)
					.AddColumn("Сотрудники").AddTextRenderer(node => node.EmployeesName)
					.AddColumn("Сумма премии").AddNumericRenderer(node => node.PremiumSum)
					.AddColumn("Причина премии").AddTextRenderer(node => node.PremiumReason)
					.Finish()
			);

			//PremiumTemplateJournalViewModel
			TreeViewColumnsConfigFactory.Register<PremiumTemplateJournalViewModel>(
				() => FluentColumnsConfig<PremiumTemplateJournalNode>.Create()
					.AddColumn("Номер").AddNumericRenderer(node => node.Id)
					.AddColumn("Шаблон комментария").AddTextRenderer(node => node.Reason)
					.AddColumn("Сумма премии").AddNumericRenderer(node => node.PremiumMoney)
					.Finish()
				);

			//CarEventTypeJournalViewModel
			TreeViewColumnsConfigFactory.Register<CarEventTypeJournalViewModel>(
				() => FluentColumnsConfig<CarEventTypeJournalNode>.Create()
					.AddColumn("Номер").AddNumericRenderer(node => node.Id)
					.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
					.AddColumn("Сокращённое\nназвание").AddTextRenderer(node => node.ShortName).WrapWidth(400).WrapMode(WrapMode.WordChar)
					.AddColumn("Комментарий\nобязателен")
						.AddToggleRenderer(node => node.NeedComment)
						.Editing(false)
					.AddColumn("В архиве")
					.AddToggleRenderer(node => node.IsArchive)
						.Editing(false)
						.XAlign(0f)
					.Finish()
				);

			//CarEventJournalViewModel
			TreeViewColumnsConfigFactory.Register<CarEventJournalViewModel>(
				() => FluentColumnsConfig<CarEventJournalNode>.Create()
					.AddColumn("Номер").AddNumericRenderer(node => node.Id)
					.AddColumn("Дата и время создания").AddTextRenderer(node => node.CreateDate.ToString("g"))
					.AddColumn("Событие").AddTextRenderer(node => node.CarEventTypeName).WrapWidth(400).WrapMode(WrapMode.WordChar)
					.AddColumn("Порядковый\nномер ТС").AddNumericRenderer(node => node.CarOrderNumber)
					.AddColumn("Гос.номер ТС").AddTextRenderer(node => node.CarRegistrationNumber)
					.AddColumn("Тип авто").AddTextRenderer(node => node.CarTypeOfUseAndOwnTypeString)
					.AddColumn("Часть города").AddTextRenderer(node => node.GeographicGroups)
					.AddColumn("Водитель").AddTextRenderer(node => node.DriverFullName)
					.AddColumn("Дата начала").AddTextRenderer(node => node.StartDate.ToString("d"))
					.AddColumn("Дата окончания").AddTextRenderer(node => node.EndDate.ToString("d"))
					.AddColumn("Стоимость").AddTextRenderer(node => node.RepairCost.ToString("0.##"))
					.AddColumn("Комментарий").AddTextRenderer(node => node.Comment).WrapWidth(400).WrapMode(WrapMode.WordChar)
					.AddColumn("Автор").AddTextRenderer(node => node.AuthorFullName)
					.Finish()
			);

			//UndeliveredOrdersJournalViewModel
			TreeViewColumnsConfigFactory.Register<UndeliveredOrdersJournalViewModel>(
				() => FluentColumnsConfig<UndeliveredOrderJournalNode>.Create()
				.AddColumn("№").HeaderAlignment(0.5f).AddNumericRenderer(node => node.NumberInList)
				.AddColumn("Код").HeaderAlignment(0.5f).AddTextRenderer(node => node.Id != 0 ? node.Id.ToString() : "")
				.AddColumn("Статус").HeaderAlignment(0.5f).AddTextRenderer(node => node.Status)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Дата\nзаказа").HeaderAlignment(0.5f).AddTextRenderer(node => node.OldOrderDeliveryDate)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Автор\nзаказа").HeaderAlignment(0.5f).AddTextRenderer(node => node.OldOrderAuthor)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Клиент и адрес").HeaderAlignment(0.5f).AddTextRenderer(node => node.ClientAndAddress)
					.WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("Интервал\nдоставки").HeaderAlignment(0.5f).AddTextRenderer(node => node.OldDeliverySchedule)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Количество\nбутылей").HeaderAlignment(0.5f).AddTextRenderer(node => node.UndeliveredOrderItems)
					.WrapWidth(75).WrapMode(WrapMode.WordChar)
				.AddColumn("Статус\nначальный ➔\n ➔ текущий").HeaderAlignment(0.5f).AddTextRenderer(node => node.OldOrderStatus)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Ответственный").HeaderAlignment(0.5f).AddTextRenderer(node => node.Guilty)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Причина").HeaderAlignment(0.5f).AddTextRenderer(node => node.Reason)
					.WrapWidth(200).WrapMode(WrapMode.WordChar)
				.AddColumn("Звонок\nв офис").HeaderAlignment(0.5f).AddTextRenderer(node => node.DriversCall)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Звонок\nклиенту").HeaderAlignment(0.5f).AddTextRenderer(node => node.DispatcherCall)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Водитель").HeaderAlignment(0.5f).AddTextRenderer(node => node.DriverName)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Перенос").HeaderAlignment(0.5f).AddTextRenderer(node => node.TransferDateTime)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Кто недовоз\nзафиксировал").HeaderAlignment(0.5f).AddTextRenderer(node => node.Registrator)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Автор\nнедовоза").HeaderAlignment(0.5f).AddTextRenderer(node => node.UndeliveryAuthor)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Оштрафованные").HeaderAlignment(0.5f).AddTextRenderer(node => node.FinedPeople)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("В работе\nу отдела").HeaderAlignment(0.5f).AddTextRenderer(node => node.InProcessAt)
					.WrapWidth(450).WrapMode(WrapMode.WordChar)
				.Finish()
			);

			//ComplaintObjectJournalViewModel
			TreeViewColumnsConfigFactory.Register<ComplaintObjectJournalViewModel>(
				() => FluentColumnsConfig<ComplaintObjectJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Дата создания").AddTextRenderer(node => node.CreateDate.ToString("g"))
					.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
					.AddColumn("Сопряженные виды").AddTextRenderer(node => node.ComplaintKinds).WrapWidth(400).WrapMode(WrapMode.WordChar)
					.AddColumn("В архиве").AddToggleRenderer(node => node.IsArchive).Editing(false).XAlign(0f)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
					.Finish()
			);

			//ComplaintKindJournalViewModel
			TreeViewColumnsConfigFactory.Register<ComplaintKindJournalViewModel>(
				() => FluentColumnsConfig<ComplaintKindJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
					.AddColumn("Объект рекламаций").AddTextRenderer(node => node.ComplaintObject).WrapWidth(400).WrapMode(WrapMode.WordChar)
					.AddColumn("Подключаемые отделы").AddTextRenderer(node => node.Subdivisions).WrapWidth(400).WrapMode(WrapMode.WordChar)
					.AddColumn("В архиве").AddToggleRenderer(node => node.IsArchive).Editing(false).XAlign(0f)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGrey : _colorBlack)
					.Finish()
			);

			//FlyersJournalViewModel
			TreeViewColumnsConfigFactory.Register<FlyersJournalViewModel>(
				() => FluentColumnsConfig<FlyersJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(n => n.Id)
					.AddColumn("Название").AddTextRenderer(n => n.Name)
					.AddColumn("Дата старта").AddTextRenderer(n => n.StartDate.ToShortDateString())
					.AddColumn("Дата окончания").AddTextRenderer(n =>
						n.EndDate.HasValue ? n.EndDate.Value.ToShortDateString() : "")
					.AddColumn("")
					.Finish()
			);

			//EquipmentKindJournalViewModel
			TreeViewColumnsConfigFactory.Register<EquipmentKindJournalViewModel>(
				() => FluentColumnsConfig<EquipmentKindJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
					.AddColumn("Гарантийный талон").AddTextRenderer(node => node.WarrantyCardType.GetEnumTitle())
					.Finish()
			);

			//ProductGroupJournalViewModel
			TreeViewColumnsConfigFactory.Register<ProductGroupJournalViewModel>(
				() => FluentColumnsConfig<ProductGroupJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.IsArchive ? "grey" : "black")
					.Finish()
			);

			//UndeliveryTransferAbsenceReasonViewModel
			TreeViewColumnsConfigFactory.Register<UndeliveryTransferAbsenceReasonJournalViewModel>(
				() => FluentColumnsConfig<UndeliveryTransferAbsenceReasonJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id.ToString())
					.AddColumn("Причина отсутствия переноса").AddTextRenderer(node => node.Name)
					.AddColumn("Дата создания").AddTextRenderer(node => node.CreateDate.ToShortDateString())
					.Finish()
			);

			//FreeRentPackagesJournalViewModel
			TreeViewColumnsConfigFactory.Register<FreeRentPackagesJournalViewModel>(
				() => FluentColumnsConfig<FreeRentPackagesJournalNode>.Create()
					.AddColumn("Код")
						.AddTextRenderer(n => n.Id.ToString())
					.AddColumn("Название")
						.AddTextRenderer(n => n.Name)
					.AddColumn("Вид оборудования")
						.AddTextRenderer(n => n.EquipmentKindName)
					.AddColumn("")
					.Finish()
			);

			//PaidRentPackagesJournalViewModel
			TreeViewColumnsConfigFactory.Register<PaidRentPackagesJournalViewModel>(
				() => FluentColumnsConfig<PaidRentPackagesJournalNode>.Create()
					.AddColumn("Код")
						.AddTextRenderer(n => n.Id.ToString())
					.AddColumn("Название")
						.AddTextRenderer(n => n.Name)
					.AddColumn("Вид оборудования")
						.AddTextRenderer(n => n.EquipmentKindName)
					.AddColumn("Цена в сутки")
						.AddTextRenderer(n => n.PriceDailyString)
					.AddColumn("Цена в месяц")
						.AddTextRenderer(n => n.PriceMonthlyString)
					.AddColumn("")
					.Finish()
			);

			//EquipmentsNonSerialForRentJournalViewModel
			TreeViewColumnsConfigFactory.Register<NonSerialEquipmentsForRentJournalViewModel>(
				() => FluentColumnsConfig<NomenclatureForRentNode>.Create()
					.AddColumn("Код")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Оборудование")
						.AddTextRenderer (node => node.NomenclatureName)
					.AddColumn("Вид оборудования")
						.AddTextRenderer (node => node.EquipmentKindName)
					.AddColumn("Кол-во")
						.AddTextRenderer (node => node.InStockText)
					.AddColumn("Зарезервировано")
						.AddTextRenderer (node => node.ReservedText)
					.AddColumn("Доступно")
						.AddTextRenderer (node => node.AvailableText)
						.AddSetter((cell, node) => cell.ForegroundGdk = node.Available > 0 ? _colorBlack : _colorRed)
					.AddColumn("")
					.Finish()
			);

			//ComplaintResultsOfCounterpartyJournalViewModel
			TreeViewColumnsConfigFactory.Register<ComplaintResultsOfCounterpartyJournalViewModel>(
				() => FluentColumnsConfig<ComplaintResultsOfCounterpartyJournalNode>.Create()
					.AddColumn("Название")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Name)
					.AddColumn("В архиве?")
						.HeaderAlignment(0.5f)
						.AddToggleRenderer(node => node.IsArchive)
						.Editing(false)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((cell, node) => cell.Foreground = node.IsArchive ? "grey" : "black")
					.Finish()
			);

			//ComplaintResultsOfEmployeesJournalViewModel
			TreeViewColumnsConfigFactory.Register<ComplaintResultsOfEmployeesJournalViewModel>(
				() => FluentColumnsConfig<ComplaintResultsOfEmployeesJournalNode>.Create()
					.AddColumn("Название")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Name)
					.AddColumn("В архиве?")
						.HeaderAlignment(0.5f)
						.AddToggleRenderer(node => node.IsArchive)
						.Editing(false)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((cell, node) => cell.Foreground = node.IsArchive ? "grey" : "black")
					.Finish()
			);

			//RoboAtsCounterpartyNameJournalViewModel
			TreeViewColumnsConfigFactory.Register<RoboAtsCounterpartyNameJournalViewModel>(
				() => FluentColumnsConfig<RoboAtsCounterpartyNameJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Имя").AddTextRenderer(node => node.Name)
					.AddColumn("Ударение").AddTextRenderer(node => node.Accent)
					.AddColumn("Готов для Roboats").AddTextRenderer(node => node.ReadyForRoboats ? "Да" : string.Empty)
					.Finish()
			);

			//RoboAtsCounterpartyPatronymicJournalViewModel
			TreeViewColumnsConfigFactory.Register<RoboAtsCounterpartyPatronymicJournalViewModel>(
				() => FluentColumnsConfig<RoboAtsCounterpartyPatronymicJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Отчество").AddTextRenderer(node => node.Patronymic)
					.AddColumn("Ударение").AddTextRenderer(node => node.Accent)
					.AddColumn("Готов для Roboats").AddTextRenderer(node => node.ReadyForRoboats ? "Да" : string.Empty)
					.Finish()
			);

			//UnAllocatedBalancesJournalViewModel
			TreeViewColumnsConfigFactory.Register<UnallocatedBalancesJournalViewModel>(
				() => FluentColumnsConfig<UnallocatedBalancesJournalNode>.Create()
					.AddColumn("Код клиента")
						.AddNumericRenderer(node => node.CounterpartyId)
					.AddColumn("ИНН")
						.AddTextRenderer(node => node.CounterpartyINN)
					.AddColumn("Наименование")
						.AddTextRenderer(node => node.CounterpartyName)
					.AddColumn("Наша организация")
						.AddTextRenderer(node => node.OrganizationName)
					.AddColumn("Баланс клиента")
						.AddNumericRenderer(node => node.CounterpartyBalance)
						.Digits(2)
					.AddColumn("Долг клиента")
						.AddNumericRenderer(node => node.CounterpartyDebt)
						.Digits(2)
					.Finish()
			);

			//TrackPointJournalViewModel
			TreeViewColumnsConfigFactory.Register<TrackPointJournalViewModel>(
				() => FluentColumnsConfig<TrackPointJournalNode>.Create()
					.AddColumn("Номер МЛ").AddNumericRenderer(node => node.RouteListId)
					.AddColumn("Время").AddTextRenderer(node => node.Time.ToString("G"))
					.AddColumn("Широта").AddNumericRenderer(node => node.Latitude).Digits(8)
					.AddColumn("Долгота").AddNumericRenderer(node => node.Longitude).Digits(8)
					.AddColumn("Время получения").AddTextRenderer(node => node.ReceiveTime.ToString("G"))
					.Finish()
			);

			//DriverTareMessagesJournalViewModel
			TreeViewColumnsConfigFactory.Register<DriverTareMessagesJournalViewModel>(
				() => FluentColumnsConfig<DriverMessageJournalNode>.Create()
					.AddColumn("Дата").AddTextRenderer(node => node.CommentDate.ToString("dd.MM.yy"))
					.AddColumn("Время").AddTextRenderer(node => node.CommentDate.ToString("HH:mm:ss"))
					.AddColumn("ФИО водителя").AddTextRenderer(node => node.DriverName)
					.AddColumn("Телефон водителя").AddTextRenderer(node => node.DriverPhone)
					.AddColumn("№ МЛ").AddNumericRenderer(node => node.RouteListId)
					.AddColumn("№ заказа").AddNumericRenderer(node => node.OrderId)
					.AddColumn("План бут.").AddNumericRenderer(node => node.BottlesReturn)
					.AddColumn("Факт бут.").AddNumericRenderer(node => node.ActualBottlesReturn)
					.AddColumn("Долг бут. по адресу").AddNumericRenderer(node => node.AddressBottlesDebt)
					.AddColumn("Комментарий водителя").AddTextRenderer(node => node.DriverComment)
					.Finish()
			);

			//TariffZoneJournalViewModel
			TreeViewColumnsConfigFactory.Register<TariffZoneJournalViewModel>(
				() => FluentColumnsConfig<TariffZoneJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("Доступна\nдоставка за час").AddTextRenderer(node => node.IsFastDeliveryAvailable ? "Да" : "").XAlign(0.5f)
					.AddColumn("Время работы\nдоставки за час").AddTextRenderer(node => node.FastDeliveryAvailableTime)
					.Finish()
			);
			
			//DeliveryScheduleJournalViewModel
			TreeViewColumnsConfigFactory.Register<DeliveryScheduleJournalViewModel>(
				() => FluentColumnsConfig<DeliveryScheduleJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("Время доставки").AddTextRenderer(node => node.DeliveryTime)
					.AddColumn("Архивный?").AddTextRenderer(node => node.IsArchive ? "Да" : string.Empty)
					.AddColumn("Готов для Roboats").AddTextRenderer(node => node.ReadyForRoboats ? "Да" : string.Empty)
					.AddColumn("")
					.Finish()
			);

			//RoboatsStreetJournalViewModel
			TreeViewColumnsConfigFactory.Register<RoboatsStreetJournalViewModel>(
				() => FluentColumnsConfig<RoboatsStreetJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Улица").AddTextRenderer(node => node.Name)
					.AddColumn("Готов для Roboats").AddTextRenderer(node => node.ReadyForRoboats ? "Да" : string.Empty)
					.AddColumn("")
					.Finish()
			);

			//RoboatsWaterTypeJournalViewModel
			TreeViewColumnsConfigFactory.Register<RoboatsWaterTypeJournalViewModel>(
				() => FluentColumnsConfig<RoboatsWaterTypeJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Номенклатура").AddTextRenderer(node => node.Nomenclature)
					.AddColumn("Готов для Roboats").AddTextRenderer(node => node.ReadyForRoboats ? "Да" : string.Empty)
					.AddColumn("")
					.Finish()
			);

			//RoboatsWaterNomenclatureJournalViewModel
			TreeViewColumnsConfigFactory.Register<RoboatsWaterNomenclatureJournalViewModel>(
				() => FluentColumnsConfig<WaterJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Номенклатура").AddTextRenderer(node => node.Title)
					.AddColumn("")
					.Finish()
			);

			//RoboatsCallsRegistryJournalViewModel
			TreeViewColumnsConfigFactory.Register<RoboatsCallsRegistryJournalViewModel>(
				(vm) => FluentColumnsConfig<RoboatsCallJournalNode>.Create()
					.SetTreeModel(() => new RecursiveTreeModel<RoboatsCallJournalNode>(vm.Items.Cast<RoboatsCallJournalNode>(), vm.RecuresiveConfig))
					.AddColumn("Код").AddNumericRenderer(node => node.EntityId)
					.AddColumn("Время").AddTextRenderer(node => node.Time.ToString("dd.MM.yyyy HH:mm:ss"))
					.AddColumn("Телефон").AddTextRenderer(node => node.Phone)
					.AddColumn("Статус").AddTextRenderer(node => node.CallStatus)
					.AddColumn("Результат").AddTextRenderer(node => node.CallResult)
					.AddColumn("Детали звонка").AddTextRenderer(node => node.Description)
					.RowCells()
					.AddSetter<CellRenderer>(
						(cell, node) => {
							var color = _colorWhite;
							if(node.NodeType == RoboatsCallNodeType.RoboatsCallDetail)
							{
								color = _colorLightGrey;
							}
							cell.CellBackgroundGdk = color;
						}
					)
					.Finish()
			);

			//FastDeliveryAvailabilityHistoryJournalViewModel
			TreeViewColumnsConfigFactory.Register<FastDeliveryAvailabilityHistoryJournalViewModel>(
				() => FluentColumnsConfig<FastDeliveryAvailabilityHistoryJournalNode>.Create()
					.AddColumn("№").AddNumericRenderer(node => node.SequenceNumber)
					.AddColumn("Id").AddNumericRenderer(node => node.Id)
					.AddColumn("Дата и время\nпроверки").AddTextRenderer(node => node.VerificationDateString)
					.AddColumn("Автор заказа").AddTextRenderer(node => node.AuthorString)
					.AddColumn("№ заказа").AddNumericRenderer(node => node.Order)
					.AddColumn("Имя контрагента").AddTextRenderer(node => node.Counterparty)
					.AddColumn("Адрес доставки").AddTextRenderer(node => node.AddressString)
					.AddColumn("Район").AddTextRenderer(node => node.District)
					.AddColumn("Доступно\nдля заказа").AddTextRenderer(node => node.IsValidString)
					.AddColumn("Комментарий логиста /\nПринятые меры").AddTextRenderer(node => node.LogisticianComment)
					.AddColumn("ФИО логиста").AddTextRenderer(node => node.LogisticianNameWithInitials)
					.AddColumn("Дата и время последнего\nсохранения комментария").AddTextRenderer(node => node.LogisticianCommentVersionString)
					.AddColumn("Время реакции в\nчасах : минутах").AddTextRenderer(node => node.LogisticianReactionTime)
					.AddColumn("Ассортимент\nне в запасе").AddTextRenderer(node => node.IsNomenclatureNotInStockSubqueryString)
					.AddColumn("")
					.Finish()
			);
			
			//PaymentsFromJournalViewModel
			TreeViewColumnsConfigFactory.Register<PaymentsFromJournalViewModel>(
				() => FluentColumnsConfig<PaymentFromJournalNode>.Create()
					.AddColumn("№")
						.HeaderAlignment(0.5f)
						.AddNumericRenderer(node => node.Id)
						.XAlign(0.5f)
					.AddColumn("Название")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Name)
						.XAlign(0.5f)
					.AddColumn("Организация\nдля платежей Авангарда")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.OrganizationName)
						.XAlign(0.5f)
					.Finish()
			);

			//GeoGroupJournalViewModel
			TreeViewColumnsConfigFactory.Register<GeoGroupJournalViewModel>(
				() => FluentColumnsConfig<GeoGroupJournalNode>.Create()
					.AddColumn("№")
						.AddNumericRenderer(node => node.Id).WidthChars(4)
					.AddColumn("Название")
						.AddTextRenderer(node => node.Name)
					.Finish()
			);

			//BulkEmailEventReasonJournalViewModel
			TreeViewColumnsConfigFactory.Register<BulkEmailEventReasonJournalViewModel>(
				() => FluentColumnsConfig<BulkEmailEventReasonJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("Архивный").AddToggleRenderer(node => node.IsArchive).Editing(false)
					.AddColumn("")
					.Finish()
			);

			//ResponsibleJournalViewModel
			TreeViewColumnsConfigFactory.Register<ResponsibleJournalViewModel>(
				() => FluentColumnsConfig<ResponsibleJournalNode>.Create()
					.AddColumn("Код").AddNumericRenderer(node => node.Id)
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("В архиве").AddToggleRenderer(node => node.IsArchived).Editing(false)
					.AddColumn("")
					.Finish()
			);

			//PhonesJournalViewModel
			TreeViewColumnsConfigFactory.Register<PhonesJournalViewModel>(
				() => FluentColumnsConfig<PhonesJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Phone)
					.AddColumn("Тип").AddTextRenderer(node => node.Type)
					.AddColumn("")
					.Finish()
			);
			
			//EdoOperatorJournalViewModel
			TreeViewColumnsConfigFactory.Register<EdoOperatorsJournalViewModel>(
				() => FluentColumnsConfig<EdoOpeartorJournalNode>.Create()
					.AddColumn("Номер").AddNumericRenderer(node => node.Id)
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("Брендовое название").AddNumericRenderer(node => node.BrandName)
					.AddColumn("Трёхзначный код").AddNumericRenderer(node => node.Code)
					.AddColumn("")
					.Finish()
			);

			//UserRolesJournalViewModel
			TreeViewColumnsConfigFactory.Register<UserRolesJournalViewModel>(
				() => FluentColumnsConfig<UserRolesJournalNode>.Create()
					.AddColumn("Код")
						.AddNumericRenderer(node => node.Id)
					.AddColumn("Название")
						.AddTextRenderer(node => node.Name)
					.AddColumn("Описание роли")
						.AddTextRenderer(node => node.Description)
					.AddColumn("")
					.Finish()
			);
		}
	}
}
