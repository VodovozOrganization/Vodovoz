using System;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListMileageCheckView : TdiTabBase
	{
		private IUnitOfWork uow;

		ViewModel.RouteListsVM viewModel;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if (uow == value)
					return;
				uow = value;
				routelistsfilter1.UoW = uow;
				viewModel = new ViewModel.RouteListsVM (value);
				viewModel.Filter = routelistsfilter1;
				viewModel.Filter.RestrictAtOnce(x => x.RestrictStatus = RouteListStatus.MileageCheck);
				treeRouteLists.RepresentationModel = viewModel;
				treeRouteLists.RepresentationModel.UpdateNodes ();
			}
		}

		public RouteListMileageCheckView()
		{
			this.Build();
			this.TabName = "Контроль за километражом.";
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			treeRouteLists.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged(object sender, EventArgs args)
		{
			buttonOpen.Sensitive = treeRouteLists.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			var node = treeRouteLists.GetSelectedNode () as ViewModel.RouteListsVMNode;
			var dlg = new RouteListMileageCheckDlg (node.Id);
			TabParent.AddTab (dlg, this);
		}

		protected void OnRouteListActivated (object o, Gtk.RowActivatedArgs args)
		{
			OnButtonOpenClicked(o, args);
		}		
	}
}

