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
	public class RoboAtsCounterpartyNameJournalViewModel : SingleEntityJournalViewModelBase<RoboAtsCounterpartyName, RoboAtsCounterpartyNameViewModel,
		RoboAtsCounterpartyNameJournalNode>
	{
		public RoboAtsCounterpartyNameJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Имена контрагентов RoboATS";

			UpdateOnChanges(typeof(RoboAtsCounterpartyName));
		}

		protected override Func<IUnitOfWork, IQueryOver<RoboAtsCounterpartyName>> ItemsSourceQueryFunction => (uow) =>
		{
			RoboAtsCounterpartyName roboAtsCounterpartyNameAlias = null;
			RoboAtsCounterpartyNameJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => roboAtsCounterpartyNameAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => roboAtsCounterpartyNameAlias.Id,
				() => roboAtsCounterpartyNameAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => roboAtsCounterpartyNameAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => roboAtsCounterpartyNameAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => roboAtsCounterpartyNameAlias.Accent).WithAlias(() => resultAlias.Accent)
				)
				.TransformUsing(Transformers.AliasToBean<RoboAtsCounterpartyNameJournalNode>());

			return itemsQuery;
		};

		protected override Func<RoboAtsCounterpartyNameViewModel> CreateDialogFunction => () =>
			new RoboAtsCounterpartyNameViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<RoboAtsCounterpartyNameJournalNode, RoboAtsCounterpartyNameViewModel> OpenDialogFunction =>
			(node) => new RoboAtsCounterpartyNameViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
