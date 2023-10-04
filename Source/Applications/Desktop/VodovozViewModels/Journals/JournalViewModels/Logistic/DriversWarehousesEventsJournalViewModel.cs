using System;
using Autofac;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class DriversWarehousesEventsJournalViewModel :
		EntityJournalViewModelBase<DriverWarehouseEvent, DriverWarehouseEventViewModel, DriversWarehousesEventsJournalNode>
	{
		private readonly ILifetimeScope _scope;
		private DriversWarehousesEventsJournalFilterViewModel _filterViewModel;

		public DriversWarehousesEventsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			IDeleteEntityService deleteEntityService = null,
			Action<DriversWarehousesEventsJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager, deleteEntityService,
				commonServices.CurrentPermissionService)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			
			TabName = "Журнал событий";

			CreateFilter(filterParams);
		}

		protected override IQueryOver<DriverWarehouseEvent> ItemsQuery(IUnitOfWork uow)
		{
			DriverWarehouseEventName eventNameAlias = null;
			DriversWarehousesEventsJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver<DriverWarehouseEvent>()
				.JoinAlias(e => e.EventName, () => eventNameAlias)
				.SelectList(list => list
					.Select(e => e.Id).WithAlias(() => resultAlias.Id)
					.Select(() => eventNameAlias.Name).WithAlias(() => resultAlias.EventName)
					.Select(e => e.Type).WithAlias(() => resultAlias.Type)
				);
			
			return query;
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<DriverWarehouseEventViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
		}

		protected override void EditEntityDialog(DriversWarehousesEventsJournalNode node)
		{
			NavigationManager.OpenViewModel<DriverWarehouseEventViewModel, IEntityUoWBuilder>(
				this, EntityUoWBuilder.ForOpen(DomainHelper.GetId(node)));
		}

		private void CreateFilter(Action<DriversWarehousesEventsJournalFilterViewModel> filterParams)
		{
			Autofac.Core.Parameter[] parameters = {
				new TypedParameter(typeof(DialogViewModelBase), this),
				new TypedParameter(typeof(Action<DriversWarehousesEventsJournalFilterViewModel>), filterParams)
			};

			_filterViewModel = _scope.Resolve<DriversWarehousesEventsJournalFilterViewModel>(parameters);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
	}
}
