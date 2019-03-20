using System;
using System.Linq;
using Gtk;
using QS.DomainModel.UoW;
using Vodovoz.Representations;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DebtorsView : QS.Dialog.Gtk.TdiTabBase
	{
		BottleDebtorsVM viewModel;

		public DebtorsView()
		{
			this.Build();
			this.TabName = "Журнал задолжености по бутылям";
			ConfigureWidget();
		}

		void ConfigureWidget()
		{
			bottledebtorsfilter.UoW = UnitOfWorkFactory.CreateWithoutRoot();
			viewModel = new BottleDebtorsVM(bottledebtorsfilter);
			treeviewDebtors.RepresentationModel = viewModel;
			treeviewDebtors.Selection.Mode = SelectionMode.Multiple;
			treeviewDebtors.RepresentationModel.UpdateNodes();
		}

		protected void OnButtonCreateTaskClicked(object sender, EventArgs e)
		{
			viewModel.CreateTask(treeviewDebtors.GetSelectedObjects().OfType<BottleDebtorsVMNode>().ToArray());
		}

		protected void OnSearchentity1TextChanged(object sender, EventArgs e)
		{
			treeviewDebtors.SearchHighlightText = searchentity1.Text;
			treeviewDebtors.RepresentationModel.SearchString = searchentity1.Text;
		}

		protected void OnButtonFilterClicked(object sender, EventArgs e)
		{
			bottledebtorsfilter.Visible = !bottledebtorsfilter.Visible;
		}
	}
}

