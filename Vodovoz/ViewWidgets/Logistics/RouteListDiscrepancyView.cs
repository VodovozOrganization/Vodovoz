using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListDiscrepancyView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		public RouteListDiscrepancyView()
		{
			Build();
			Configure();
			Items = new List<Discrepancy>();
		}

		public RouteList RouteList { get; set; }

		List<Discrepancy> items;
		public List<Discrepancy> Items {
			get => items;
			set {
				items = value;
                ytreeRouteListDiscrepancyItemsView.ItemsDataSource = items;
			}
		}

		/// <summary>
		/// Перезапись встроенного свойства Sensitive
		/// Sensitive теперь работает только с таблицей
		/// К сожалению Gtk обходит этот параметр, если выставлять Sensitive какому-либо элементу управления выше по дереву
		/// </summary>
		public new bool Sensitive
        {
			get => ytreeRouteListDiscrepancyItemsView.Sensitive;
			set => ytreeRouteListDiscrepancyItemsView.Sensitive = value;
		}

		public IList<RouteListControlNotLoadedNode> ItemsLoaded { get; set; }

		protected void Configure()
		{
			var colorRed = new Gdk.Color(0xee, 0x66, 0x66);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
            ytreeRouteListDiscrepancyItemsView.ColumnsConfig = ColumnsConfigFactory.Create<Discrepancy>()
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Недопо-\nгрузка")
					.AddNumericRenderer(node => node.FromWarehouse)
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddColumn("Выгру-\nзка")
					.AddNumericRenderer(node => node.ToWarehouse)
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddColumn("Не-\nдовоз")
					.AddTextRenderer(node => node.Returns)
				.AddColumn("От \nклиента")
					.AddNumericRenderer(node => node.PickedUpFromClient)
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddColumn("Расхо-\nждения")
					.AddNumericRenderer(node => node.Remainder)
						.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 1, 0))
				.AddColumn("Ущерб")
					.AddTextRenderer(x => x.SumOfDamage.ToString("C"))
				.AddColumn("Штраф")
					.AddToggleRenderer(x => x.UseFine).ToggledEvent(UseFine_Toggled)
				.RowCells()
					.AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.Remainder == 0 ? colorWhite : colorRed)
				.Finish();
		}

		public event EventHandler FineChanged;

		void UseFine_Toggled(object o, ToggledArgs args)
		{
			//Вызываем через Application.Invoke что бы событие вызывалось уже после того как поле обновилось.
			Application.Invoke(delegate {
				FineChanged?.Invoke(this, EventArgs.Empty);
			});
		}

		public void FindDiscrepancies(IList<RouteListItem> items, List<EntityRepositories.Logistic.ReturnsNode> allReturnsToWarehouse) {
			Items = RouteList.GetDiscrepancies(ItemsLoaded, allReturnsToWarehouse);
		}
	}

	public class EquipmentKindGroupingResult
	{
		public EquipmentKind EquipmentKind { get; set; }
		public int Amount { get; set; }
		public static EquipmentKindGroupingResult Selector(EquipmentKind kind, IEnumerable<int> amounts)
		{
			return new EquipmentKindGroupingResult {
				EquipmentKind = kind,
				Amount = amounts.Sum()
			};
		}
	}
}