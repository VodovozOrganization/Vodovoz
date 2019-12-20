using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.GtkWidgets;
using Gtk;
using Pango;
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

		Gdk.Color wh = new Gdk.Color(255, 255, 255);
		Gdk.Color gr = new Gdk.Color(230, 230, 230);
		Gdk.Color red = new Gdk.Color(255, 0, 0);


		void ConfigureWidget()
		{
			yTreeView.ColumnsConfig = ColumnsConfigFactory.Create<object>()
				.AddColumn("Виновный")
					.AddTextRenderer(n => GetNodeText(n))
					.AddSetter((c ,n) => c.Alignment = GetAlignment(n))
					.WrapWidth(150).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddTextRenderer(n => GetCount(n))
					.WrapWidth(50).WrapMode(Pango.WrapMode.WordChar)
				.RowCells()
					.AddSetter<CellRenderer>((c, n) => c.CellBackgroundGdk = GetColor(n))
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

		private Pango.Alignment GetAlignment(object node)
		{
			return node is ComplaintGuiltyNode ? Pango.Alignment.Left : Pango.Alignment.Right;
		}

		private string GetNodeText(object node)
		{
			if(node is ComplaintGuiltyNode) {
				return (node as ComplaintGuiltyNode).GuiltyName;
			}
			if(node is ComplaintResultNode) {
				return (node as ComplaintResultNode).Text ?? "Не указано";
			}
			return "";
		}

		private string GetCount(object node)
		{
			if(node is ComplaintGuiltyNode) {
				return (node as ComplaintGuiltyNode).Count.ToString();
			}
			if(node is ComplaintResultNode) {
				return (node as ComplaintResultNode).Count.ToString();
			}
			return "";
		}

		private Gdk.Color GetColor(object node)
		{
			if(node is ComplaintGuiltyNode) {
				return gr;
			}
			if(node is ComplaintResultNode) {
				return wh;
			}
			return red;
		}

		DateTime? StartDate { get; set; }
		DateTime? EndDate { get; set; }
		IList<ComplaintGuiltyNode> guilties = new List<ComplaintGuiltyNode>();

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => guilties.Any();

		public void OnCurrentObjectChanged(object changedObject) => Application.Invoke((s, arg) => Refresh());

		public void Refresh()
		{
			StartDate = (InfoProvider as IComplaintsInfoProvider)?.StartDate;
			EndDate = (InfoProvider as IComplaintsInfoProvider)?.EndDate;
			lblCaption.Markup = string.Format(
				"<u><b>Сводка по жалобам{0}{1}.\nСписок виновных:</b></u>",
				StartDate.HasValue ? string.Format("\nс {0} ", StartDate.Value.ToString("dd.MM.yyyy")) : string.Empty,
				EndDate.HasValue ? string.Format("по {0}", EndDate.Value.ToString("dd.MM.yyyy")) : "\nза всё время"
			);

			var cntTotal = complaintsRepository.GetUnclosedComplaintsCount(InfoProvider.UoW);
			var cntOverdued = complaintsRepository.GetUnclosedComplaintsCount(InfoProvider.UoW, true);
			lblUnclosedCount.Markup = string.Format(
				"<b>Не закрыто <span foreground='{2}'>{0}</span> жалоб,\nиз них просрочено <span foreground='{2}'>{1}</span> шт.</b>",
				cntTotal,
				cntOverdued,
				cntTotal >= 0 ? "red" : "black"
			);

			var levels = LevelConfigFactory
						.FirstLevel<ComplaintGuiltyNode, ComplaintResultNode>(x => x.ComplaintResultNodes)
						.LastLevel(c => c.ComplaintGuiltyNode).EndConfig();
			guilties = new List<ComplaintGuiltyNode>(complaintsRepository.GetGuiltyAndCountForDates(InfoProvider.UoW, StartDate, EndDate));
			yTreeView.YTreeModel = new LevelTreeModel<ComplaintGuiltyNode>(guilties, levels);

			var results = complaintsRepository.GetComplaintsResults(InfoProvider.UoW, StartDate, EndDate);
			yTVComplainsResults.SetItemsSource(results);
		}

		#endregion

	}
}
