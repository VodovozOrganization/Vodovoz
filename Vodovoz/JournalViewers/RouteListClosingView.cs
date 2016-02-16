using System;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using QSTDI;
using System.Linq;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListClosingView : TdiTabBase
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
				viewModel = new ViewModel.RouteListsVM (value);
				viewModel.Filter = new RouteListsFilter(uow);
				viewModel.Filter.RestrictStatus = RouteListStatus.ReadyToReport;
				treeRouteLists.RepresentationModel = viewModel;
				treeRouteLists.RepresentationModel.UpdateNodes ();
			}
		}
				
		public RouteListClosingView()
		{
			this.Build();
			this.TabName = "Готовые к закрытию";
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			treeRouteLists.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged(object sender, EventArgs args)
		{
			buttonCloseRouteList.Sensitive = treeRouteLists.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonCloseRouteListClicked (object sender, EventArgs e)
		{
			var node = treeRouteLists.GetSelectedNode () as ViewModel.RouteListsVMNode;
			var closing = uow.Session.QueryOver<RouteListClosing>()
				.Where(item => item.RouteList.Id == node.Id)
				.Take(1).List().FirstOrDefault();
			var dlg = closing != null ? new RouteListClosingDlg(closing) : new RouteListClosingDlg(node.Id);
			TabParent.AddTab (dlg, this);
		}

		protected void OnRouteListActivated (object o, Gtk.RowActivatedArgs args)
		{
			OnButtonCloseRouteListClicked(o, args);
		}
	}
}

