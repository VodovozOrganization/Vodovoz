using System;
using System.Linq;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Representations;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DebtorsView : QS.Dialog.Gtk.TdiTabBase
	{
		BottleDebtorsVM bottleDebtorVM;

		public DebtorsView()
		{
			this.Build();
			this.TabName = "Журнал задолжености по бутылям";
			ConfigureWidget();
		}

		void ConfigureWidget()
		{
			bottledebtorsfilter.UoW = UnitOfWorkFactory.CreateWithoutRoot();
			bottleDebtorVM = new BottleDebtorsVM(bottledebtorsfilter);
			treeviewDebtors.RepresentationModel = bottleDebtorVM;
			treeviewDebtors.Selection.Mode = SelectionMode.Multiple;
			treeviewDebtors.RepresentationModel.UpdateNodes();
		}

		protected void OnButtonCreateTaskClicked(object sender, EventArgs e)
		{
			bottleDebtorVM.CreateTask(treeviewDebtors.GetSelectedObjects().OfType<BottleDebtorsVMNode>().ToArray());
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


		protected void OnTreeviewDebtorsButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != 3)
				return;
				
			var selectedObj = treeviewDebtors.GetSelectedObjects()?[0];
			var selectedNodeId = (selectedObj as BottleDebtorsVMNode)?.AddressId;
			if(selectedNodeId == null)
				return;

			RepresentationSelectResult[] representation = { new RepresentationSelectResult(selectedNodeId.Value, selectedObj) };
			var popup = bottleDebtorVM.GetPopupMenu(representation);
			popup.ShowAll();
			popup.Popup();
		}

		protected void OnButtonOpenReportClicked(object sender, EventArgs e)
		{
			var selected = treeviewDebtors.GetSelectedObjects().FirstOrDefault();

			if(selected is BottleDebtorsVMNode selectedNode) 
				bottleDebtorVM.OpenReport(selectedNode.ClientId, selectedNode.AddressId);
			else
				MessageDialogHelper.RunInfoDialog("Необходимо выбрать точку доставки");
		}
	}
}

