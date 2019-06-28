using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.Config;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Dialogs.Client;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels
{
	public class ClientCameFromJournalViewModel : SingleEntityJournalViewModelBase<ClientCameFrom, ClientCameFromDlg, ClientCameFromJournalNode, ClientCameFromFilterViewModel>
	{
		public ClientCameFromJournalViewModel(ClientCameFromFilterViewModel filterViewModel, IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(filterViewModel, entityConfigurationProvider, commonServices)
		{
			TabName = "Откуда клиент";
			SetOrder<ClientCameFromJournalNode>(x => x.Name);

			RegisterAliasPropertiesToSearch(
				() => clientCameFromAlias.Name,
				() => clientCameFromAlias.Id
			);
		}

		ClientCameFrom clientCameFromAlias = null;
		ClientCameFromJournalNode resultAlias = null;

		protected override Func<IQueryOver<ClientCameFrom>> ItemsSourceQueryFunction => () => {
			var query = UoW.Session.QueryOver(() => clientCameFromAlias);
			if(!FilterViewModel.RestrictArchive)
				query.Where(() => !clientCameFromAlias.IsArchive);

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

		protected override Func<ClientCameFromDlg> CreateDialogFunction => () => new ClientCameFromDlg();

		protected override Func<ClientCameFromJournalNode, ClientCameFromDlg> OpenDialogFunction => node => new ClientCameFromDlg(node.Id);
	}
}
