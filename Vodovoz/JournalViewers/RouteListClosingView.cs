using System;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repositories.Permissions;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using Vodovoz.Tools.CallTasks;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Tools;

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
				routelistsfilter1.OnlyStatuses = new RouteListStatus[] {RouteListStatus.Delivered, RouteListStatus.OnClosing};
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
				case RouteListStatus.Confirmed:
					TabParent.OpenTab(RouteListCreateDlg.GenerateHashName(node.Id), () => new RouteListCreateDlg(node.Id));
					break;
				case RouteListStatus.EnRoute:
					TabParent.OpenTab(RouteListKeepingDlg.GenerateHashName(node.Id), () => new RouteListKeepingDlg(node.Id));
					break;
				case RouteListStatus.MileageCheck:
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
														  && ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("routelist_unclosing");
			popupMenu.Add(menuItemRouteList);

			return popupMenu;
		}

		private void MenuItemRouteList_Activated(object sender, EventArgs e)
		{
			var callTaskWorker = new CallTaskWorker(
				CallTaskSingletonFactory.GetInstance(),
				new CallTaskRepository(),
				OrderSingletonRepository.GetInstance(),
				EmployeeSingletonRepository.GetInstance(),
				new BaseParametersProvider(),
				ServicesConfig.CommonServices.UserService,
				SingletonErrorReporter.Instance);

			if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("routelist_unclosing")) {
				RouteList rl = UoW.GetById<RouteList>(selectedNode.Id);
				if(rl == null) {
					return;
				}
				rl.ChangeStatusAndCreateTask(RouteListStatus.OnClosing, callTaskWorker);
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

		public override void Destroy()
		{
			if(UoW != null) {
				UoW.Dispose();
			}
			base.Destroy();
		}
	}
}