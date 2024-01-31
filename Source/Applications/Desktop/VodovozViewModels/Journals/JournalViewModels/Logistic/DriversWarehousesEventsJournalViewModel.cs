using System;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Logistics.Drivers;
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

			SearchEnabled = false;
			VisibleDeleteAction = false;
			CreateFilter(filterParams);
		}

		protected override IQueryOver<DriverWarehouseEvent> ItemsQuery(IUnitOfWork uow)
		{
			DriverWarehouseEvent eventAlias = null;
			DriversWarehousesEventsJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver(() => eventAlias);

			if(_filterViewModel.EventId.HasValue)
			{
				query.Where(e => e.Id == _filterViewModel.EventId);
			}

			if(!string.IsNullOrWhiteSpace(_filterViewModel.EventName))
			{
				query.Where(Restrictions.Like(
					Projections.Property(() => eventAlias.EventName),
					_filterViewModel.EventName,
					MatchMode.Anywhere));
			}

			if(_filterViewModel.SelectedEventType.HasValue)
			{
				query.Where(e => e.Type == _filterViewModel.SelectedEventType);
			}

			if(_filterViewModel.EventLatitude.HasValue)
			{
				query.Where(e => e.Latitude == _filterViewModel.EventLatitude);
			}

			if(_filterViewModel.EventLongitude.HasValue)
			{
				query.Where(e => e.Longitude == _filterViewModel.EventLongitude);
			}

			query.SelectList(list => list
				.Select(e => e.Id).WithAlias(() => resultAlias.Id)
				.Select(e => e.EventName).WithAlias(() => resultAlias.EventName)
				.Select(e => e.Type).WithAlias(() => resultAlias.Type)
				.Select(e => e.DocumentType).WithAlias(() => resultAlias.DocumentType)
				.Select(e => e.QrPositionOnDocument).WithAlias(() => resultAlias.QrPositionOnDocument)
				.Select(e => e.Latitude).WithAlias(() => resultAlias.Latitude)
				.Select(e => e.Longitude).WithAlias(() => resultAlias.Longitude)
				.Select(e => e.IsArchive).WithAlias(() => resultAlias.IsArchive))
				.TransformUsing(Transformers.AliasToBean<DriversWarehousesEventsJournalNode>());
			
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
