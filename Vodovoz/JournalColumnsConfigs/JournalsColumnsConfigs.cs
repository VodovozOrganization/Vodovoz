using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Journal.GtkUI;
using QSProjectsLib;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.Representations;
using Vodovoz.JournalViewModels.Organization;
using Vodovoz.JournalViewModels.Employees;

namespace Vodovoz.JournalColumnsConfigs
{
	public static class JournalsColumnsConfigs
	{
		public static void RegisterColumns()
		{
			//OrderJournalViewModel
			TreeViewColumnsConfigFactory.Register<OrderJournalViewModel>(
				() => FluentColumnsConfig<OrderJournalNode>.Create()
					.AddColumn("Номер").SetDataProperty(node => node.Id.ToString())
					.AddColumn("Дата").SetDataProperty(node => node.Date.ToString("d"))
					.AddColumn("Автор").SetDataProperty(node => node.Author)
					.AddColumn("Время").SetDataProperty(node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
					.AddColumn("Статус").SetDataProperty(node => node.StatusEnum.GetEnumTitle())
					.AddColumn("Бутыли").AddTextRenderer(node => node.BottleAmount.ToString())
					.AddColumn("Кол-во с/о").AddTextRenderer(node => node.SanitisationAmount.ToString())
					.AddColumn("Клиент").SetDataProperty(node => node.Counterparty)
					.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
					.AddColumn("Коор.").AddTextRenderer(x => x.Coordinates)
					.AddColumn("Район доставки").SetDataProperty(node => node.IsSelfDelivery ? "-" : node.DistrictName)
					.AddColumn("Адрес").SetDataProperty(node => node.Address)
					.AddColumn("Изменил").SetDataProperty(node => node.LastEditor)
					.AddColumn("Послед. изменения").AddTextRenderer(node => node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString() : string.Empty)
					.AddColumn("Номер звонка").SetDataProperty(node => node.DriverCallId)
					.AddColumn("OnLine заказ №").SetDataProperty(node => node.OnLineNumber)
					.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
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
					.AddColumn("№ п/п").AddTextRenderer(node => node.SequenceNumber.ToString()).XAlign(0.5f)
					.AddColumn("№ жалобы").AddTextRenderer(node => node.Id.ToString()).XAlign(0.5f)
					.AddColumn("Тип").AddTextRenderer(node => node.TypeString).XAlign(0.5f)
					.AddColumn("Дата").AddTextRenderer(node => node.DateString).XAlign(0.5f)
					.AddColumn("Статус").AddTextRenderer(node => node.StatusString).XAlign(0.5f)
					.AddColumn("В работе у").AddTextRenderer(node => node.WorkInSubdivision).XAlign(0f)
					.AddColumn("Дата план.\nзавершения").AddTextRenderer(node => node.PlannedCompletionDate).XAlign(0.5f)
					.AddColumn("Клиент и адрес").AddTextRenderer(node => node.ClientNameWithAddress).XAlign(0f)
					.AddColumn("Виновный").AddTextRenderer(node => node.Guilties).XAlign(0f)
					.AddColumn("Проблема").AddTextRenderer(node => node.ComplaintText).XAlign(0f)
					.AddColumn("Автор").AddTextRenderer(node => node.Author).XAlign(0f)
					.AddColumn("Штрафы").AddTextRenderer(node => node.Fines).XAlign(0.5f)
					.AddColumn("Результат").AddTextRenderer(node => node.ResultText).XAlign(0f)
					.AddColumn("Дата факт.\nзавершения").AddTextRenderer(node => node.ActualCompletionDateString).XAlign(0.5f)
					.AddColumn("Дни").AddTextRenderer(node => node.DaysInWork).XAlign(0.5f)
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
		}
	}
}