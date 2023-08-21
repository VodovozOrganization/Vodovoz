using System;
using System.Collections.Generic;
using System.ComponentModel;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz
{
	[ToolboxItem(true)]
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
		[Browsable(false)]
		public new bool Sensitive
        {
			get => ytreeRouteListDiscrepancyItemsView.Sensitive;
			set => ytreeRouteListDiscrepancyItemsView.Sensitive = value;
		}

		protected void Configure()
		{
			var colorRed = new Gdk.Color(0xee, 0x66, 0x66);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
            ytreeRouteListDiscrepancyItemsView.ColumnsConfig = ColumnsConfigFactory.Create<Discrepancy>()
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Погрузка")
					.AddNumericRenderer(node => node.FromWarehouse)
				.AddColumn("Получено\nот других\nводителей")
					.AddNumericRenderer(node => node.TransferedFromDrivers)
				.AddColumn("Передано\nдругим\nводителям")
					.AddNumericRenderer(node => node.TransferedToAnotherDrivers)
				.AddColumn("Отгружено\nклиентам")
					.AddNumericRenderer(node => node.DeliveredToClient)
				.AddColumn("Получено от\nклиентов")
					.AddNumericRenderer(node => node.PickedUpFromClient)
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddColumn("Выгру-\nзка")
					.AddNumericRenderer(node => node.ToWarehouse)
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
			//Вызываем через Gtk.Application.Invoke что бы событие вызывалось уже после того как поле обновилось.
			Gtk.Application.Invoke(delegate {
				FineChanged?.Invoke(this, EventArgs.Empty);
			});
		}

		public void FindDiscrepancies() {
			Items = RouteList.GetDiscrepancies();
		}
	}
}
