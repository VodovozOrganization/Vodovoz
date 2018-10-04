using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.Repositories;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrdersPanelView : Gtk.Bin, IPanelView
	{
		public UndeliveredOrdersPanelView()
		{
			this.Build();
			Gdk.Color wh = new Gdk.Color(255, 255, 255);
			Gdk.Color gr = new Gdk.Color(223, 223, 223);
			yTreeView.ColumnsConfig = ColumnsConfigFactory.Create<object[]>()
				.AddColumn("Виновный")
					.AddTextRenderer(n => n[0].ToString())
					.WrapWidth(150).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddTextRenderer(n => n[1].ToString())
					.WrapWidth(50).WrapMode(Pango.WrapMode.WordChar)
				.RowCells()
					.AddSetter<CellRenderer>((c, n) => c.CellBackgroundGdk = (int)n[2] % 2 == 0 ? wh : gr)
				.Finish();
		}

		DateTime StartDate { get; set; }
		DateTime EndDate { get; set; }
		List<object[]> guilties = new List<object[]>();

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => guilties.Any();

		public void OnCurrentObjectChanged(object changedObject)
		{
			Refresh();
		}

		public void Refresh()
		{
			StartDate = (InfoProvider as IUndeliveredOrdersInfoProvider).StartDate;
			EndDate = (InfoProvider as IUndeliveredOrdersInfoProvider).EndDate;
			lblCaption.Markup = String.Format(
				"<u><b>За интервал дат с\n{0} по {1}\nв недовозах виновны:</b></u>",
				StartDate.ToString("dd.MM.yyyy"),
				EndDate.ToString("dd.MM.yyyy")
			);

			guilties = new List<object[]>(UndeliveredOrdersRepository.GetGuiltyAndCountForDates(InfoProvider.UoW, StartDate, EndDate));
			yTreeView.ItemsDataSource = guilties;
		}

		#endregion
	}
}
