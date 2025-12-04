using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Client
{
	public class EdoOperatorsJournalViewModel : SingleEntityJournalViewModelBase<EdoOperator, EdoOperatorViewModel, EdoOpeartorJournalNode>
	{
		public EdoOperatorsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал операторов ЭДО";

			UpdateOnChanges(typeof(EdoOperator));
		}

		protected override Func<IUnitOfWork, IQueryOver<EdoOperator>> ItemsSourceQueryFunction => (uow) =>
		{
			EdoOperator edoOpeartorAlias = null;
			EdoOpeartorJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => edoOpeartorAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => edoOpeartorAlias.Id,
				() => edoOpeartorAlias.Name,
				() => edoOpeartorAlias.BrandName,
				() => edoOpeartorAlias.Code)
			);

			itemsQuery.SelectList(list => list
					.Select(() => edoOpeartorAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => edoOpeartorAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => edoOpeartorAlias.BrandName).WithAlias(() => resultAlias.BrandName)
					.Select(() => edoOpeartorAlias.Code).WithAlias(() => resultAlias.Code)
				)
				.TransformUsing(Transformers.AliasToBean<EdoOpeartorJournalNode>());

			return itemsQuery;
		};

		protected override Func<EdoOperatorViewModel> CreateDialogFunction => () =>
			new EdoOperatorViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);
		
		protected override Func<EdoOpeartorJournalNode, EdoOperatorViewModel> OpenDialogFunction =>
			(node) => new EdoOperatorViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
