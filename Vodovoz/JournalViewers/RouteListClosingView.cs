using System;
using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repositories.Permissions;
using QS.Dialog.GtkUI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListClosingView : QS.Dialog.Gtk.TdiTabBase
	{

		private IUnitOfWork uow;

		private ViewModel.RouteListsVMNode selectedNode;

		ViewModel.RouteListsVM viewModel;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if(uow == value)
					return;
				uow = value;
				viewModel = new ViewModel.RouteListsVM(value);
				routelistsfilter1.UoW = uow;
				routelistsfilter1.SetFilterStatus(RouteListStatus.OnClosing);
				viewModel.Filter = routelistsfilter1;
				treeRouteLists.RepresentationModel = viewModel;
			}
		}

		public RouteListClosingView()
		{
			this.Build();
			this.TabName = "Готовые к закрытию";
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			treeRouteLists.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged(object sender, EventArgs args)
		{
			buttonCloseRouteList.Sensitive = treeRouteLists.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonCloseRouteListClicked(object sender, EventArgs e)
		{
			var node = treeRouteLists.GetSelectedNode() as ViewModel.RouteListsVMNode;

			switch(node.StatusEnum) {
				case RouteListStatus.New:
				case RouteListStatus.InLoading:
					TabParent.OpenTab(RouteListCreateDlg.GenerateHashName(node.Id), () => new RouteListCreateDlg(node.Id));
					break;
				case RouteListStatus.EnRoute:
					TabParent.OpenTab(RouteListKeepingDlg.GenerateHashName(node.Id), () => new RouteListKeepingDlg(node.Id));
					break;
				case RouteListStatus.MileageCheck:
					TabParent.OpenTab(RouteListMileageCheckDlg.GenerateHashName(node.Id), () => new RouteListMileageCheckDlg(node.Id));
					break;
				case RouteListStatus.OnClosing:
				case RouteListStatus.Closed:
					if(PermissionRepository.HasAccessToClosingRoutelist()) {
						TabParent.OpenTab(RouteListClosingDlg.GenerateHashName(node.Id), () => new RouteListClosingDlg(node.Id));
					} else {
						MessageDialogHelper.RunWarningDialog("Доступ запрещен");
					}

					break;
				default: throw new NotSupportedException("Тип документа не поддерживается.");
			}
		}

		protected void OnRouteListActivated(object o, Gtk.RowActivatedArgs args)
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

