using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using Gtk;
using QS.Journal.GtkUI;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.JournalNodes;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.JournalViewModels.Suppliers;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using Vodovoz.Representations;
using Vodovoz.JournalViewModels;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.Nodes.Cash;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	public static class JournalsColumnsConfigs
	{
		static Gdk.Color colorBlack = new Gdk.Color(0, 0, 0);
		static Gdk.Color colorRed = new Gdk.Color(0xfe, 0x5c, 0x5c);
		static Gdk.Color colorPink = new Gdk.Color(0xff, 0xc0, 0xc0);
		static Gdk.Color colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
		static Gdk.Color colorDarkGrey = new Gdk.Color(0x80, 0x80, 0x80);
		static Gdk.Color colorLightGreen = new Color(0xc0, 0xff, 0xc0);

		public static void RegisterColumns()
		{
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
					.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.Status == DistrictsSetStatus.Closed ? colorDarkGrey : colorBlack)
					.Finish()
			);
			
			//DistrictJournalViewModel
			TreeViewColumnsConfigFactory.Register<DistrictJournalViewModel>(
				() => FluentColumnsConfig<DistrictJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("Зарплатный район").AddTextRenderer(node => node.WageDistrict)
					.AddColumn("Статус набора районов").AddTextRenderer(node => node.DistrictsSetStatus.GetEnumTitle())
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
					.AddColumn("Послед. изменения").AddTextRenderer(node => node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString() : string.Empty)
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
						.WrapMode(Pango.WrapMode.WordChar)
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
				() => FluentColumnsConfig<Representations.DebtorJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(x => x.AddressId > 0 ? x.AddressId.ToString() : x.ClientId.ToString())
					.AddColumn("Клиент").AddTextRenderer(node => node.ClientName)
					.AddColumn("Адрес").AddTextRenderer(node => String.IsNullOrWhiteSpace(node.AddressName) ? "Самовывоз" : node.AddressName)
					.AddColumn("ОПФ").AddTextRenderer(node => node.OPF.GetEnumTitle())
					.AddColumn("Последний заказ по адресу").AddTextRenderer(node => node.LastOrderDate != null ? node.LastOrderDate.Value.ToString("dd / MM / yyyy") : string.Empty)
					.AddColumn("Кол-во отгруженных в последнюю реализацию бутылей").AddNumericRenderer(node => node.LastOrderBottles)
					.AddColumn("Долг по таре (по адресу)").AddNumericRenderer(node => node.DebtByAddress)
					.AddColumn("Долг по таре (по клиенту)").AddNumericRenderer(node => node.DebtByClient)
					.AddColumn("Ввод остат.").AddTextRenderer(node => node.IsResidueExist)
					.AddColumn("Резерв").AddNumericRenderer(node => node.Reserve)
					.RowCells().AddSetter<CellRendererText>((CellRendererText c, Representations.DebtorJournalNode n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//CounterpartyJournalViewModel
			TreeViewColumnsConfigFactory.Register<CounterpartyJournalViewModel>(
				() => FluentColumnsConfig<CounterpartyJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
					.AddColumn("Вн.номер").AddTextRenderer(x => x.InternalId.ToString())
					.AddColumn("Тег").AddTextRenderer(x => x.Tags, useMarkup: true)
					.AddColumn("Контрагент").AddTextRenderer(node => node.Name).WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
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
					.AddColumn("Номер").SetDataProperty(node => node.Id.ToString())
					.AddColumn("Дата").SetDataProperty(node => node.Date.ToString("d"))
					.AddColumn("Автор").SetDataProperty(node => node.Author)
					.AddColumn("Статус").SetDataProperty(node => node.StatusEnum.GetEnumTitle())
					.AddColumn("Тип оплаты").SetDataProperty(node => node.PaymentTypeEnum.GetEnumTitle())
					.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
					.AddColumn("Клиент").SetDataProperty(node => node.Counterparty)
					.AddColumn("Вариант оплаты").SetDataProperty(node => node.PayOption)
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
					.AddColumn("Документ").AddTextRenderer(node => string.Format("Ввод остатков №{0}", node.Id)).SearchHighlight()
					.AddColumn("Дата").AddTextRenderer(node => node.DateString)
					.AddColumn("Контрагент").AddTextRenderer(NodeType => NodeType.Counterparty)
					.AddColumn("Точка доставки").AddTextRenderer(NodeType => NodeType.DeliveryPoint)
					.AddColumn("Автор").AddTextRenderer(node => node.Author)
					.AddColumn("Изменил").AddTextRenderer(node => node.LastEditor)
					.AddColumn("Послед. изменения").AddTextRenderer(node => node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString() : string.Empty)
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
						.WrapWidth(300).WrapMode(Pango.WrapMode.WordChar)
						.XAlign(0f)
					.AddColumn("Виновный").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Guilties)
						.XAlign(0f)
					.AddColumn("Проблема").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ComplaintText)
						.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
						.XAlign(0f)
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
						.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
						.XAlign(0f)
					.AddColumn("Дата факт.\nзавершения").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ActualCompletionDateString)
						.XAlign(0.5f)
					.AddColumn("Дни").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.DaysInWork)
						.XAlign(0.5f)
					.RowCells()
					.AddSetter<CellRenderer>(
						(cell, node) => {
							var color = colorWhite;
							if(node.Status != Domain.Complaints.ComplaintStatuses.Closed && node.LastPlannedCompletionDate.Date < DateTime.Today)
								color = colorPink;
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

			//SubdivisionsJournalViewModel
			TreeViewColumnsConfigFactory.Register<FinesJournalViewModel>(
				() => FluentColumnsConfig<FineJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
					.AddColumn("Сотудники").AddTextRenderer(node => node.EmployeesName)
					.AddColumn("Сумма штрафа").AddTextRenderer(node => node.FineSumm.ToString())
					.AddColumn("Причина штрафа").AddTextRenderer(node => node.FineReason)
					.Finish()
			);

			//NomenclaturesJournalViewModel
			TreeViewColumnsConfigFactory.Register<NomenclaturesJournalViewModel>(
				() => FluentColumnsConfig<NomenclatureJournalNode>.Create()
					.AddColumn("Код")
						.AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Номенклатура")
						.SetDataProperty(node => node.Name)
					.AddColumn("Категория")
						.SetDataProperty(node => node.Category.GetEnumTitle())
					.AddColumn("Кол-во")
						.AddTextRenderer(node => node.InStockText)
					.AddColumn("Зарезервировано")
						.AddTextRenderer(node => node.ReservedText)
					.AddColumn("Доступно")
						.AddTextRenderer(node => node.AvailableText)
						.AddSetter((cell, node) => cell.ForegroundGdk = node.Available > 0 ? colorBlack : colorRed)
					.AddColumn("Код в ИМ")
						.AddTextRenderer(node => node.OnlineStoreExternalId)
					.Finish()
			);

			//NomenclaturesJournalViewModel
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
			
			//NomenclaturesJournalViewModel
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
						.SetDataProperty(n => n.Name)
					.AddColumn("Дата")
						.HeaderAlignment(0.5f)
						.SetDataProperty(n => n.Created.ToString("G"))
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
					.AddColumn("По умолчанию для новых сотрудников")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.IsDefaultLevelString)
					.AddColumn("По умолчанию для новых сотрудников (Наши авто)")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.IsDefaultLevelOurCarsString)
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
						.WrapMode(Pango.WrapMode.WordChar)
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
						.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? colorDarkGrey : colorBlack)
					.Finish()
			);

			//CarJournalViewModel
			TreeViewColumnsConfigFactory.Register<CarJournalViewModel>(
				() => FluentColumnsConfig<CarJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
					.AddColumn("Модель").AddTextRenderer(x => x.Model).WrapWidth(300).WrapMode(Pango.WrapMode.WordChar)
					.AddColumn("Гос. номер").AddTextRenderer(x => x.RegistrationNumber)
					.AddColumn("Водитель").AddTextRenderer(x => x.DriverName)
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
					.AddColumn("OSM").AddTextRenderer(x => x.FoundOnOsm ? "Да" : "")
					.AddColumn("Испр.").AddTextRenderer(x => x.FixedInOsm ? "Да" : "")
					.AddColumn("Адрес")
						.AddTextRenderer(node => node.CompiledAddress)
						.WrapMode(Pango.WrapMode.WordChar)
						.WrapWidth(1000)
					.AddColumn("Адрес из 1с").AddTextRenderer(x => x.Address1c)
					.AddColumn("Клиент").AddTextRenderer(x => x.Counterparty)
					.AddColumn("Номер").AddTextRenderer(x => x.IdString)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
					.Finish()
			);

			//PaymentJournalViewModel
			TreeViewColumnsConfigFactory.Register<PaymentsJournalViewModel>(
				() => FluentColumnsConfig<PaymentJournalNode>.Create()
					.AddColumn("№").AddTextRenderer(x => x.PaymentNum.ToString())
					.AddColumn("Дата").AddTextRenderer(x => x.Date.ToShortDateString())
					.AddColumn("Cумма").AddTextRenderer(x => x.Total.ToString())
					.AddColumn("Заказы").AddTextRenderer(x => x.Orders)
					.AddColumn("Плательщик").AddTextRenderer(x => x.Counterparty)
						.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
					.AddColumn("Получатель").AddTextRenderer(x => x.Organization)
					.AddColumn("Назначение платежа").AddTextRenderer(x => x.PaymentPurpose)
						.WrapWidth(600).WrapMode(Pango.WrapMode.WordChar)
					.AddColumn("Категория дохода/расхода")
						.AddTextRenderer(x => x.ProfitCategory).XAlign(0.5f)
					.AddColumn("")
					.RowCells().AddSetter<CellRenderer>(
						(c, n) => {
							var color = colorWhite;

							if(n.Status == PaymentState.undistributed)
								color = colorPink;

							if(n.Status == PaymentState.distributed)
								color = colorLightGreen;

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
						.WrapMode(Pango.WrapMode.WordChar)
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
						.Editing(true)
						.XAlign(0.5f)
					.AddColumn("")
					.RowCells()
						.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? colorDarkGrey : colorBlack)
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
						.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? colorDarkGrey : colorBlack)
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
					.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? colorDarkGrey : colorBlack)
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
						.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? colorDarkGrey : colorBlack)
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
							c.ForegroundGdk = n.Status == DistrictsSetStatus.Closed ? colorDarkGrey : colorBlack)
					.Finish()
			);
		}
	}
}