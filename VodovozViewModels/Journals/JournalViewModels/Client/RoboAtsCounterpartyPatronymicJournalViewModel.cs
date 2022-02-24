using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Client
{
	public class RoboAtsCounterpartyPatronymicJournalViewModel : SingleEntityJournalViewModelBase<RoboAtsCounterpartyPatronymic, RoboAtsCounterpartyPatronymicViewModel,
		RoboAtsCounterpartyPatronymicJournalNode>
	{
		public RoboAtsCounterpartyPatronymicJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Отчества контрагентов RoboATS";

			UpdateOnChanges(typeof(RoboAtsCounterpartyPatronymic));
		}

		protected override Func<IUnitOfWork, IQueryOver<RoboAtsCounterpartyPatronymic>> ItemsSourceQueryFunction => (uow) =>
		{
			RoboAtsCounterpartyPatronymic roboAtsCounterpartyPatronymicAlias = null;
			RoboAtsCounterpartyPatronymicJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => roboAtsCounterpartyPatronymicAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => roboAtsCounterpartyPatronymicAlias.Id,
				() => roboAtsCounterpartyPatronymicAlias.Patronymic)
			);

			itemsQuery.SelectList(list => list
					.Select(() => roboAtsCounterpartyPatronymicAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => roboAtsCounterpartyPatronymicAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => roboAtsCounterpartyPatronymicAlias.Accent).WithAlias(() => resultAlias.Accent)
				)
				.OrderBy(() => roboAtsCounterpartyPatronymicAlias.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<RoboAtsCounterpartyPatronymicJournalNode>());

			return itemsQuery;
		};

		protected override Func<RoboAtsCounterpartyPatronymicViewModel> CreateDialogFunction => () =>
			new RoboAtsCounterpartyPatronymicViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<RoboAtsCounterpartyPatronymicJournalNode, RoboAtsCounterpartyPatronymicViewModel> OpenDialogFunction =>
			(node) => new RoboAtsCounterpartyPatronymicViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
