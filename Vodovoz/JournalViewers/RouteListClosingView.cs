using System;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using QSTDI;
using Gtk;
using Vodovoz.Repository;
using QSProjectsLib;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListClosingView : TdiTabBase
	{

		private IUnitOfWork uow;

		private ViewModel.RouteListsVMNode selectedNode;

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
				viewModel.Filter = routelistsfilter1;
				viewModel.Filter.UoW = uow;
				viewModel.Filter.SetFilterStatus(RouteListStatus.OnClosing);
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
			TabParent.OpenTab (RouteListClosingDlg.GenerateHashName (node.Id),
			                   () => new RouteListClosingDlg (node.Id));
		}

		protected void OnRouteListActivated (object o, Gtk.RowActivatedArgs args)
		{
			OnButtonCloseRouteListClicked(o, args);
		}

		protected void OnButtonCleanClicked(object sender, EventArgs e)
		{
			entrySearch.Text = String.Empty;
		}

		protected void OnEntrySearchChanged(object sender, EventArgs e)
		{
			viewModel.SearchString = entrySearch.Text;
		}

		private Menu GetPopupMenu(ViewModel.RouteListsVMNode node)
		{
			Menu popupMenu = new Gtk.Menu();
			Gtk.MenuItem menuItemRouteList = new MenuItem("Вернуть в статус \"Сдается\"");
			menuItemRouteList.Activated += MenuItemRouteList_Activated;
			menuItemRouteList.Sensitive = node.StatusEnum == Vodovoz.Domain.Logistic.RouteListStatus.Closed 
														  && QSMain.User.Permissions["routelist_unclosing"];
			popupMenu.Add(menuItemRouteList);

			return popupMenu;
		}

		private void MenuItemRouteList_Activated(object sender, EventArgs e)
		{
			if(QSMain.User.Permissions["routelist_unclosing"]) {
				RouteList rl = UoW.GetById<RouteList>(selectedNode.Id);
				if(rl == null) {
					return;
				}
				rl.ChangeStatus(RouteListStatus.OnClosing);
				UoW.Save(rl);
				UoW.Commit();
			}
		}

		protected void OnTreeRouteListsButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			selectedNode = treeRouteLists.GetSelectedNode() as ViewModel.RouteListsVMNode;
			if(args.Event.Button == 3 && selectedNode != null) {
				var menu = GetPopupMenu(selectedNode);
				if(menu != null) {
					menu.ShowAll();
					menu.Popup();
				}
			}
		}
	}
}

