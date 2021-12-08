using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels.Journals.JournalNodes.Contacts;
using Vodovoz.ViewModels.ViewModels.Contacts;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Contacts
{
	public class RoboAtsCounterpartyPatronymicJournalViewModel : SingleEntityJournalViewModelBase<RoboAtsCounterpartyPatronymic, RoboatsCounterpartyPatronymicViewModel,
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
				.TransformUsing(Transformers.AliasToBean<RoboAtsCounterpartyPatronymicJournalNode>());

			return itemsQuery;
		};

		protected override Func<RoboatsCounterpartyPatronymicViewModel> CreateDialogFunction => () =>
			new RoboatsCounterpartyPatronymicViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<RoboAtsCounterpartyPatronymicJournalNode, RoboatsCounterpartyPatronymicViewModel> OpenDialogFunction =>
			(node) => new RoboatsCounterpartyPatronymicViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
