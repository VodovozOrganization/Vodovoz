using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Commands;
using Vodovoz.ViewModels.Dialogs.Complaints;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Complaints
{
	public class ResponsibleJournalViewModel : SingleEntityJournalViewModelBase<Responsible, ResponsibleViewModel, ResponsibleJournalNode>
	{
		public ResponsibleJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Ответственные";
			UpdateOnChanges(typeof(Responsible));
		}

		protected override Func<IUnitOfWork, IQueryOver<Responsible>> ItemsSourceQueryFunction => (uow) =>
		{
			Responsible responsibleAlias = null;
			ResponsibleJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => responsibleAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => responsibleAlias.Id,
				() => responsibleAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => responsibleAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => responsibleAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => responsibleAlias.IsArchived).WithAlias(() => resultAlias.IsArchived)
				)
				.OrderBy(() => responsibleAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<ResponsibleJournalNode>());

			return itemsQuery;
		};

		protected override Func<ResponsibleViewModel> CreateDialogFunction => () =>
			new ResponsibleViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<ResponsibleJournalNode, ResponsibleViewModel> OpenDialogFunction =>
			(node) => new ResponsibleViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
