using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Journal.GtkUI;
using QSProjectsLib;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.JournalViewModels.Employees;
using Vodovoz.JournalViewModels.Organization;
using Vodovoz.JournalViewModels.Suppliers;
using Vodovoz.JournalViewModels.WageCalculation;
using Vodovoz.Representations;

namespace Vodovoz.JournalColumnsConfigs
{
	public static class JournalsColumnsConfigs
	{
		static Gdk.Color colorBlack = new Gdk.Color(0, 0, 0);
		static Gdk.Color colorRed = new Gdk.Color(0xff, 0, 0);

		public static void RegisterColumns()
		{
			//OrderJournalViewModel
			TreeViewColumnsConfigFactory.Register<OrderJournalViewModel>(
				() => FluentColumnsConfig<OrderJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
					.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
					.AddColumn("Автор").AddTextRenderer(node => node.Author)
					.AddColumn("Время").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
					.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
					.AddColumn("Бутыли").AddTextRenderer(node => node.BottleAmount.ToString())
					.AddColumn("Кол-во с/о").AddTextRenderer(node => node.SanitisationAmount.ToString())
					.AddColumn("Клиент").AddTextRenderer(node => node.Counterparty)
					.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
					.AddColumn("Коор.").AddTextRenderer(x => x.Coordinates)
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
					.AddColumn("Бутыли").AddTextRenderer(node => node.BottleAmount.ToString())
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
					.AddColumn("№ жалобы").HeaderAlignment(0.5f)
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
						.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
						.XAlign(0f)
					.AddColumn("Виновный").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.Guilties)
						.XAlign(0f)
					.AddColumn("Проблема").HeaderAlignment(0.5f)
						.AddTextRenderer(node => node.ComplaintText)
						.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
						.XAlign(0f)
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
					.Finish()
			);

			//RequestsToSuppliersJournalViewModel
			TreeViewColumnsConfigFactory.Register<RequestsToSuppliersJournalViewModel>(
				() => FluentColumnsConfig<RequestToSupplierJournalNode>.Create()
					.AddColumn("Номер")
						.HeaderAlignment(0.5f)
						.AddTextRenderer(n => n.Id.ToString())
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
		}
	}
}