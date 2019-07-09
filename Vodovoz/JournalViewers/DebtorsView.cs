using System;
using System.Linq;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
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
			var taskCount = bottleDebtorVM.CreateTask(treeviewDebtors.GetSelectedObjects().OfType<BottleDebtorsVMNode>().ToArray());
			MessageDialogHelper.RunInfoDialog($"Создано задач: {taskCount}");
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

			var selectedObjects = treeviewDebtors.GetSelectedObjects();
			var selectedObj = selectedObjects?[0];
			var selectedNodeId = (selectedObj as BottleDebtorsVMNode)?.AddressId;
			if(selectedNodeId == null)
				return;

			Menu popup = new Menu();
			var popupItems = bottleDebtorVM.PopupItems;
			foreach(var popupItem in popupItems) {
				var menuItem = new MenuItem(popupItem.Title) {
					Sensitive = popupItem.SensitivityFunc.Invoke(selectedObjects)
				};
				menuItem.Activated += (sender, e) => { popupItem.ExecuteAction.Invoke(selectedObjects); };
				popup.Add(menuItem);
			}

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

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			bottleDebtorVM?.UpdateNodes();
		}

		public override void Destroy()
		{
			bottledebtorsfilter.UoW?.Dispose();
			base.Destroy();
		}
	}
}

