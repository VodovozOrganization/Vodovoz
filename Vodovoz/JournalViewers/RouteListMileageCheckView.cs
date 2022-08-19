using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Services;
using System;
using QS.Navigation;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
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

			MainClass.MainWin.NavigationManager.OpenViewModel<RouteListMileageCheckViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(node.Id), OpenPageOptions.AsSlave);
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

