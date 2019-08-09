using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintPanelView : Bin, IPanelView
	{
		readonly IComplaintsRepository complaintsRepository;

		public ComplaintPanelView(IComplaintsRepository complaintsRepository)
		{
			this.complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			this.Build();
			ConfigureWidget();
		}

		void ConfigureWidget()
		{
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
			yTVComplainsResults.ColumnsConfig = ColumnsConfigFactory.Create<object[]>()
				.AddColumn("Итог")
					.AddTextRenderer(n => n[0] != null ? n[0].ToString() : "(результат не выставлен)")
					.WrapWidth(150).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddTextRenderer(n => n[1].ToString())
					.WrapWidth(50).WrapMode(Pango.WrapMode.WordChar)
				.Finish();
		}

		DateTime? StartDate { get; set; }
		DateTime? EndDate { get; set; }
		IList<object[]> guilties = new List<object[]>();

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => guilties.Any();

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			StartDate = (InfoProvider as IComplaintsInfoProvider)?.StartDate;
			EndDate = (InfoProvider as IComplaintsInfoProvider)?.EndDate;
			lblCaption.Markup = string.Format(
				"<u><b>Сводка по жалобам{0}{1}.\nВиновны в закрытых жалобах:</b></u>",
				StartDate.HasValue ? string.Format("\nс {0} ", StartDate.Value.ToString("dd.MM.yyyy")) : string.Empty,
				EndDate.HasValue ? string.Format("по {0}", EndDate.Value.ToString("dd.MM.yyyy")) : "\nза всё время"
			);

			var cnt = complaintsRepository.GetUnclosedComplaintsCount(InfoProvider.UoW);
			lblUnclosedCount.Markup = string.Format(
				"<b>Не закрыто <span foreground='{1}'>{0}</span> шт.:</b>",
				cnt,
				cnt >= 0 ? "red" : "black"
			);

			guilties = new List<object[]>(complaintsRepository.GetGuiltyAndCountForDates(InfoProvider.UoW, StartDate, EndDate));
			yTreeView.ItemsDataSource = guilties;

			var results = complaintsRepository.GetComplaintsResults(InfoProvider.UoW, StartDate, EndDate);
			yTVComplainsResults.SetItemsSource(results);
		}

		#endregion

	}
}
