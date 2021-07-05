using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Dialogs.Client;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels
{
	public class ClientCameFromJournalViewModel : FilterableSingleEntityJournalViewModelBase<ClientCameFrom, ClientCameFromViewModel, ClientCameFromJournalNode, ClientCameFromFilterViewModel>
	{
		public ClientCameFromJournalViewModel(
			EntitiesJournalActionsViewModel journalActionsViewModel,
			ClientCameFromFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(journalActionsViewModel, filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Откуда клиент";
			SetOrder(x => x.Name);
			UpdateOnChanges(typeof(ClientCameFrom));
		}

		protected override Func<IUnitOfWork, IQueryOver<ClientCameFrom>> ItemsSourceQueryFunction => (uow) => {
			ClientCameFrom clientCameFromAlias = null;
			ClientCameFromJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => clientCameFromAlias);
			if(!FilterViewModel.RestrictArchive)
				query.Where(() => !clientCameFromAlias.IsArchive);

			query.Where(GetSearchCriterion(
				() => clientCameFromAlias.Name,
				() => clientCameFromAlias.Id
			));

			var resultQuery = query
				.SelectList(list => list
				   .Select(x => x.Id).WithAlias(() => resultAlias.Id)
				   .Select(x => x.Name).WithAlias(() => resultAlias.Name)
				   .Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<ClientCameFromJournalNode>());

			return resultQuery;
		};

		protected override Func<ClientCameFromViewModel> CreateDialogFunction => () => new ClientCameFromViewModel (
			EntityUoWBuilder.ForCreate(),
		   	UnitOfWorkFactory,
			CommonServices
		);

		protected override Func<JournalEntityNodeBase, ClientCameFromViewModel> OpenDialogFunction => node => new ClientCameFromViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
		    UnitOfWorkFactory,
			CommonServices
		);
	}
}
