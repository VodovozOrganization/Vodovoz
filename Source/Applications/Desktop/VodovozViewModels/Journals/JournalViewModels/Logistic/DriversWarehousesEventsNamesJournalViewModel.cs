using System;
using Autofac;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class DriversWarehousesEventsNamesJournalViewModel :
		EntityJournalViewModelBase<DriverWarehouseEventName, DriverWarehouseEventNameViewModel, DriversWarehousesEventsNamesJournalNode>
	{
		private readonly ILifetimeScope _scope;

		public DriversWarehousesEventsNamesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			IDeleteEntityService deleteEntityService = null)
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
			
			TabName = "Журнал имен событий";
		}

		protected override IQueryOver<DriverWarehouseEventName> ItemsQuery(IUnitOfWork uow)
		{
			DriversWarehousesEventsNamesJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver<DriverWarehouseEventName>()
				.SelectList(list => list
					.Select(en => en.Id).WithAlias(() => resultAlias.Id)
					.Select(en => en.Name).WithAlias(() => resultAlias.EventName)
				);
			
			return query;
		}
	}
}
