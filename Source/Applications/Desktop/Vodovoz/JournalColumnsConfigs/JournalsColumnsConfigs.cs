﻿using Gamma.Binding;
using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Journal.GtkUI;
using System;
using System.Linq;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using Vodovoz.ViewModels.Journals.JournalViewModels.Roboats;

namespace Vodovoz.JournalColumnsConfigs
{
	public static class JournalsColumnsConfigs
	{
		private static readonly Color _colorWhite = new Color(0xff, 0xff, 0xff);
		private static readonly Color _colorLightGray = new Color(0xcc, 0xcc, 0xcc);

		public static void RegisterColumns()
		{
			var registratorGeneric = typeof(IColumnsConfigRegistrar<,>);
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => p.IsClass
					&& !p.IsAbstract
					&& p.GetInterfaces().Any(x =>
						x.IsGenericType &&
						x.GetGenericTypeDefinition() == registratorGeneric));

			foreach(var type in types)
			{
				Activator.CreateInstance(type);
			}

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
								color = _colorLightGray;
							}
							cell.CellBackgroundGdk = color;
						}
					)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<CashReceiptsJournalViewModel>(
				(vm) => FluentColumnsConfig<CashReceiptJournalNode>.Create()
					.SetTreeModel(() => new RecursiveTreeModel<CashReceiptJournalNode>(vm.Items.Cast<CashReceiptJournalNode>(), vm.RecuresiveConfig))
					.AddColumn(" ┯ Код чека\n └ Код маркир. ").AddNumericRenderer(node => node.ReceiptOrProductCodeId).Editing(false)
					.AddColumn(" ━ Id чека \n").AddTextRenderer(node => node.ReceiptDocId)
					.AddColumn(" ━ Создан \n").AddTextRenderer(node => node.CreatedTime)
					.AddColumn(" ━ Изменен \n").AddTextRenderer(node => node.ChangedTime)
					.AddColumn(" ━ Статус \n").AddTextRenderer(node => node.Status)
					.AddColumn(" ━ Сумма \n").AddNumericRenderer(node => node.ReceiptSum).Digits(2).Editing(false)
					.AddColumn(" ━ Код МЛ \n").AddTextRenderer(node => node.RouteList)	
					.AddColumn(" ━ Водитель \n").AddTextRenderer(node => node.DriverFIO)
					.AddColumn(" ┯ Код заказа\n └ Код стр. заказа ").AddNumericRenderer(node => node.OrderAndItemId).Digits(0).Editing(false)

					.AddColumn(" ┯ Статус фиск. док.\n └ Исх. GTIN ").AddTextRenderer(node => node.FiscalDocStatusOrSourceGtin)
					.AddColumn(" ┯ Номер фиск. док.\n └ Исх. Сер. номер ").AddTextRenderer(node => node.FiscalDocNumberOrSourceCodeInfo)
					.AddColumn(" ┯ Дата фиск. док.\n └ Итог. GTIN ").AddTextRenderer(node => node.FiscalDocDateOrResultGtin)
					.AddColumn(" ┯ Дата статуса фиск. док.\n └ Итог. Сер. номер ").AddTextRenderer(node => node.FiscalDocStatusDateOrResultSerialnumber)


					.AddColumn(" ┯ Ручная отправка\n └ Брак ").AddToggleRenderer(node => node.IsManualSentOrIsDefectiveCode).Editing(false)

					.AddColumn(" ━ Отправлен на \n").AddTextRenderer(node => node.Contact)
					.AddColumn(" ━ Причина не отскан. бутылей \n").AddTextRenderer(node => node.UnscannedReason).WrapMode(Pango.WrapMode.Word).WrapWidth(350)
					.AddColumn(" ━ Описание ошибки \n ").AddTextRenderer(node => node.ErrorDescription).WrapMode(Pango.WrapMode.Word).WrapWidth(350)
					.AddColumn("")
					.RowCells()
					.AddSetter<CellRenderer>(
						(cell, node) => {
							var color = _colorWhite;
							if(node.NodeType == CashReceiptNodeType.Code)
							{
								color = _colorLightGray;
							}
							cell.CellBackgroundGdk = color;
						}
					)
					.Finish()
			);
		}
	}
}
