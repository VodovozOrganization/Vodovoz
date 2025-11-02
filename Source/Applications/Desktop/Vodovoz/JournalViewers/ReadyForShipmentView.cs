using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class ReadyForShipmentView : QS.Dialog.Gtk.TdiTabBase
	{
		private IUnitOfWork uow;

		ReadyForShipmentVM viewModel;

		public IUnitOfWork UoW {
			get => uow;
			set {
				if(uow == value)
					return;
				uow = value;
				viewModel = new ReadyForShipmentVM(value);
				readyforshipmentfilter1.ParentTab = this;
				readyforshipmentfilter1.UoW = value;
				viewModel.Filter = readyforshipmentfilter1;
				tableReadyForShipment.RepresentationModel = viewModel;
				tableReadyForShipment.RepresentationModel.UpdateNodes();
			}
		}

		public ReadyForShipmentView()
		{
			this.Build();
			this.TabName = "Готовые к погрузке";
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			tableReadyForShipment.Selection.Changed += OnSelectionChanged;
		}

		RepresentationSelectResult[] lastMenuSelected;

		void OnSelectionChanged(object sender, EventArgs e)
		{
			buttonOpen.Sensitive = tableReadyForShipment.Selection.CountSelectedRows() > 0;

			if(tableReadyForShipment.GetSelectedObject() is ReadyForShipmentVMNode selectedNode)
				lastMenuSelected = new[] { new RepresentationSelectResult(selectedNode.Id, selectedNode) };
		}

		protected void OnButtonOpenClicked(object sender, EventArgs e)
		{
			var node = tableReadyForShipment.GetSelectedNode() as ReadyForShipmentVMNode;
			var dlg = new CarLoadDocumentDlg(node.Id, viewModel.Filter.Warehouse?.Id);
			TabParent.AddTab(dlg, this);
		}

		protected void OnTableReadyForShipmentRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonOpen.Click();
		}

		protected void OnSearchentity2TextChanged(object sender, EventArgs e)
		{
			tableReadyForShipment.SearchHighlightText = searchentity2.Text;
			tableReadyForShipment.RepresentationModel.SearchString = searchentity2.Text;
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			tableReadyForShipment.RepresentationModel.UpdateNodes();
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}

