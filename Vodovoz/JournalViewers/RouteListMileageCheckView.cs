using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Services;
using System;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListMileageCheckView : QS.Dialog.Gtk.TdiTabBase
	{
		private readonly ILifetimeScope _autofacScope = MainClass.AppDIContainer.BeginLifetimeScope();

		private IUnitOfWork uow;

		ViewModel.RouteListsVM viewModel;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set	{
				if (uow == value)
					return;
				uow = value;
				routelistsfilter1.UoW = uow;
				viewModel = new ViewModel.RouteListsVM(value);
				viewModel.Filter = routelistsfilter1;
				viewModel.Filter.SetAndRefilterAtOnce(x => x.OnlyStatuses = new RouteListStatus[] {RouteListStatus.MileageCheck, RouteListStatus.Delivered});
				treeRouteLists.RepresentationModel = viewModel;
				treeRouteLists.RepresentationModel.UpdateNodes ();
			}
		}

		public RouteListMileageCheckView()
		{
			this.Build();
			this.TabName = "Контроль за километражем.";
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

			var wageParameterService = new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(new ParametersProvider()));

			var routeListMileageCheckViewModel = new RouteListMileageCheckViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				ServicesConfig.CommonServices,
				_autofacScope.Resolve<ICarJournalFactory>(),
				_autofacScope.Resolve<IEmployeeJournalFactory>(),
				_autofacScope.Resolve<IDeliveryShiftRepository>(),
				_autofacScope.Resolve<IOrderParametersProvider>(),
				_autofacScope.Resolve<IDeliveryRulesParametersProvider>(),
				_autofacScope.Resolve<IGtkTabsOpener>(),
				_autofacScope.Resolve<BaseParametersProvider>(),
				_autofacScope.Resolve<ITrackRepository>(),
				_autofacScope.Resolve<ICallTaskRepository>(),
				_autofacScope.Resolve<IEmployeeRepository>(),
				_autofacScope.Resolve<IOrderRepository>(),
				_autofacScope.Resolve<IErrorReporter>(),
				wageParameterService,
				_autofacScope.Resolve<IRouteListRepository>(),
				_autofacScope.Resolve<IRouteListItemRepository>()
			);

			TabParent.AddSlaveTab(this, routeListMileageCheckViewModel);

		}

		protected void OnRouteListActivated (object o, Gtk.RowActivatedArgs args)
		{
			OnButtonOpenClicked(o, args);
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}

