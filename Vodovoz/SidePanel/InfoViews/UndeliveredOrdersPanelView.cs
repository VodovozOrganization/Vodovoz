using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.GtkWidgets;
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
			yTreeView.ColumnsConfig = ColumnsConfigFactory.Create<UndeliveredOrderCountNode>()
				.AddColumn("Виновный")
					.AddTextRenderer(n => n.GuiltySide)
					.WrapWidth(150).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddTextRenderer(n => n.CountStr)
					.WrapWidth(50).WrapMode(Pango.WrapMode.WordChar)
				.Finish();
		}

		DateTime StartDate { get; set; }
		DateTime EndDate { get; set; }
		List<UndeliveredOrderCountNode> guilties = new List<UndeliveredOrderCountNode>();

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

			guilties = UndeliveredOrdersRepository.GetListOfUndeliveriesCountForDates(InfoProvider.UoW, StartDate, EndDate).ToList();

			if(guilties.Any(g => g.Type == Domain.Orders.GuiltyTypes.Department)) {
				var guiltiesDpts = UndeliveredOrdersRepository.GetListOfUndeliveriesCountOnDptForDates(InfoProvider.UoW, StartDate, EndDate)
				                                              .ToList<UndeliveredOrderCountNode>();
				var parent = guilties.FirstOrDefault(g => g.Type == Domain.Orders.GuiltyTypes.Department);
				parent.Children = guiltiesDpts;
				guiltiesDpts.ForEach(d => d.Parent = parent);
				yTreeView.YTreeModel = new RecursiveTreeModel<UndeliveredOrderCountNode>(guilties, x => x.Parent, x => x.Children);
				yTreeView.ExpandAll();
			} else
				yTreeView.SetItemsSource<UndeliveredOrderCountNode>(guilties);
		}

		#endregion
	}
}
