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
		}
	}
}
