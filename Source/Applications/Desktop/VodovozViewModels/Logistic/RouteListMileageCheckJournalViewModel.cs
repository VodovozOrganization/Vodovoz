using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Services;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListMileageCheckJournalViewModel : RouteListJournalViewModel
	{
		public RouteListMileageCheckJournalViewModel(RouteListJournalFilterViewModel filterViewModel,
			IRouteListRepository routeListRepository,
			ISubdivisionRepository subdivisionRepository,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ICallTaskWorker callTaskWorker,
			IWarehouseRepository warehouseRepository,
			IEmployeeRepository employeeRepository,
			IGtkTabsOpener gtkTabsOpener,
			IStockRepository stockRepository,
			IReportPrinter reportPrinter,
			ITerminalNomenclatureProvider terminalNomenclatureProvider,
			ICommonServices commonServices,
			IRouteListProfitabilitySettings routeListProfitabilitySettings,
			IWarehousePermissionService warehousePermissionService,
			IRouteListDailyNumberProvider routeListDailyNumberProvider,
			IUserSettings userSettings,
			IStoreDocumentHelper storeDocumentHelper,
			IRouteListService routeListService)
			: base(
				  filterViewModel,
				  routeListRepository,
				  subdivisionRepository,
				  unitOfWorkFactory,
				  navigationManager,
				  callTaskWorker,
				  warehouseRepository,
				  employeeRepository,
				  gtkTabsOpener,
				  stockRepository,
				  reportPrinter,
				  terminalNomenclatureProvider,
				  commonServices,
				  routeListProfitabilitySettings,
				  warehousePermissionService,
				  routeListDailyNumberProvider,
				  userSettings,
				  storeDocumentHelper,
				  routeListService)
		{
			TabName = "Контроль за километражем.";

			FilterViewModel.SetAndRefilterAtOnce(x => x.DisplayableStatuses =
				new RouteListStatus[]
				{
					RouteListStatus.MileageCheck,
					RouteListStatus.Delivered
				});
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateEditAction();
		}

		protected override void CreatePopupActions()
		{
		}

		private void CreateEditAction()
		{
			var action = new JournalAction(
				"Открыть",
				selectedItems => true,
				selectedItems => true,
				selectedItems =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						NavigationManager.OpenViewModel<RouteListMileageCheckViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(selectedNode.Id));
					}
				}
			);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = action;
			}

			NodeActionsList.Add(action);
		}
	}
}
