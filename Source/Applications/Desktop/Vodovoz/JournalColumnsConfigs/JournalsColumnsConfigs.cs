using Gamma.Binding;
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
								color = _colorLightGray;
							}
							cell.CellBackgroundGdk = color;
						}
					)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<TrueMarkReceiptOrdersRegistryJournalViewModel>(
				(vm) => FluentColumnsConfig<TrueMarkReceiptOrderNode>.Create()
					.SetTreeModel(() => new RecursiveTreeModel<TrueMarkReceiptOrderNode>(vm.Items.Cast<TrueMarkReceiptOrderNode>(), vm.RecuresiveConfig))
					.AddColumn("Код").AddNumericRenderer(node => node.EntityId)
					.AddColumn("Время").AddTextRenderer(node => node.Time.HasValue ? node.Time.Value.ToString("dd.MM.yyyy HH:mm:ss") : "")
					.AddColumn("Статус").AddTextRenderer(node => node.Status)
					.AddColumn("Код заказа или\nстроки заказа").AddNumericRenderer(node => node.OrderAndItemId).Digits(0)
					.AddColumn("МЛ").AddNumericRenderer(node => node.RouteListId).Digits(0)
					.AddColumn("Водитель").AddTextRenderer(node => node.DriverFIO)
					.AddColumn("Брак").AddToggleRenderer(node => node.IsDefectiveCode).Editing(false)
					.AddColumn("Чек").AddToggleRenderer(node => node.HasReceipt).Editing(false)
					.AddColumn("Дубль").AddToggleRenderer(node => node.IsDuplicateCode).Editing(false)
					.AddColumn("Источник\nGTIN").AddTextRenderer(node => node.SourceGtin)
					.AddColumn("Результат\nGTIN").AddTextRenderer(node => node.ResultGtin)
					.AddColumn("Источник\nСерийный номер").AddTextRenderer(node => node.SourceSerialnumber)
					.AddColumn("Результат\nСерийный номер").AddTextRenderer(node => node.ResultSerialnumber)
					.AddColumn("Причина не отскани-\nрованных бутылей").AddTextRenderer(node => node.UnscannedReason).WrapMode(Pango.WrapMode.Word).WrapWidth(400)
					.AddColumn("Описание ошибки").AddTextRenderer(node => node.ErrorDescription).WrapMode(Pango.WrapMode.Word).WrapWidth(400)
					.AddColumn("")
					.RowCells()
					.AddSetter<CellRenderer>(
						(cell, node) => {
							var color = _colorWhite;
							if(node.NodeType == TrueMarkOrderNodeType.Order)
							{
								color = _colorLightGray;
								cell.CellBackgroundGdk = color;
							}
						}
					)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<ReceiptsJournalViewModel>(
				(vm) => FluentColumnsConfig<CashReceiptNode>.Create()
					.AddColumn("Код заказа").AddNumericRenderer(node => node.OrderId)
					.AddColumn("Дата доставки").AddTextRenderer(node => node.DeliveryDate.HasValue ? node.DeliveryDate.Value.ToString("dd.MM.yyyy") : "")
					.RowCells()
					.AddColumn("Сумма").AddNumericRenderer(node => node.OrderSum)
					.AddColumn("Самовывоз").AddToggleRenderer(node => node.IsSelfdelivery).Editing(false)
					.AddColumn("Тип оплаты").AddTextRenderer(node => node.PaymentType)
					.AddColumn("Код чека").AddNumericRenderer(node => node.ReceiptId)
					.AddColumn("Время чека").AddTextRenderer(node => node.ReceiptTime.HasValue ? node.ReceiptTime.Value.ToString("dd.MM.yyyy HH:mm:ss") : "")
					.AddColumn("Статус").AddTextRenderer(node => node.Status)
					.AddColumn("МЛ").AddNumericRenderer(node => node.RouteListId).Digits(0)
					.AddColumn("Причина не отскани-\nрованных бутылей").AddTextRenderer(node => node.UnscannedReason).WrapMode(Pango.WrapMode.Word).WrapWidth(400)
					.AddColumn("Водитель").AddTextRenderer(node => node.DriverFIO)
					.AddColumn("Описание ошибки").AddTextRenderer(node => node.ErrorDescription).WrapMode(Pango.WrapMode.Word).WrapWidth(400)
					.AddColumn("")
					.Finish()
			);
		}
	}
}
