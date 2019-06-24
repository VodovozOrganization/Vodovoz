using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Journal.GtkUI;
using QSProjectsLib;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	public static class JournalsColumnsConfigs
	{
		public static void RegisterColumns()
		{
			//OrderJournalViewModel
			TreeViewColumnsConfigFactory.Register<OrderJournalViewModel>(() => FluentColumnsConfig<OrderJournalNode>.Create()
				.AddColumn("Номер").SetDataProperty(node => node.Id.ToString())
				.AddColumn("Дата").SetDataProperty(node => node.Date.ToString("d"))
				.AddColumn("Автор").SetDataProperty(node => node.Author)
				.AddColumn("Время").SetDataProperty(node => node.DeliveryTime)
				.AddColumn("Статус").SetDataProperty(node => node.StatusEnum.GetEnumTitle())
				.AddColumn("Бутыли").AddTextRenderer(node => node.BottleAmount.ToString())
				.AddColumn("Кол-во с/о").AddTextRenderer(node => node.SanitisationAmount.ToString())
				.AddColumn("Клиент").SetDataProperty(node => node.Counterparty)
				.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
				.AddColumn("Коор.").AddTextRenderer(x => x.Latitude.HasValue && x.Longitude.HasValue ? "Есть" : String.Empty)
				.AddColumn("Район доставки").SetDataProperty(node => node.DistrictName)
				.AddColumn("Адрес").SetDataProperty(node => node.Address)
				.AddColumn("Изменил").SetDataProperty(node => node.LastEditor)
				.AddColumn("Послед. изменения").AddTextRenderer(node => node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString() : String.Empty)
				.AddColumn("Номер звонка").SetDataProperty(node => node.DriverCallId)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
				.Finish());

			//CounterpartyJournalViewModel
			TreeViewColumnsConfigFactory.Register<CounterpartyJournalViewModel>(() => FluentColumnsConfig<CounterpartyJournalNode>.Create()
			.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
			.AddColumn("Вн.номер").AddTextRenderer(x => x.InternalId.ToString())
			.AddColumn("Тег").AddTextRenderer(x => x.Tags, useMarkup: true)
			.AddColumn("Контрагент").AddTextRenderer(node => node.Name).WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
			.AddColumn("Телефоны").AddTextRenderer(x => x.Phones)
			.AddColumn("ИНН").AddTextRenderer(x => x.INN)
			.AddColumn("Договора").AddTextRenderer(x => x.Contracts)
			.AddColumn("Точки доставки").AddTextRenderer(x => x.Addresses)
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
			.Finish());
		}


	}
}
