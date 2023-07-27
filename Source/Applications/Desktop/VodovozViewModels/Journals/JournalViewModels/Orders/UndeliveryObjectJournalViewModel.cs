using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class UndeliveryObjectJournalViewModel : FilterableSingleEntityJournalViewModelBase<UndeliveryObject, UndeliveryObjectViewModel, UndeliveryObjectJournalNode, UndeliveryObjectJournalFilterViewModel>
	{
		public UndeliveryObjectJournalViewModel(UndeliveryObjectJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Объекты недовозов";

			UpdateOnChanges(
				typeof(UndeliveryObject)
			);
		}

		protected override Func<IUnitOfWork, IQueryOver<UndeliveryObject>> ItemsSourceQueryFunction => (uow) =>
		{
			UndeliveryObject undeliveryObjectAlias = null;
			UndeliveryKind undeliveryKindAlias = null;
			UndeliveryObjectJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => undeliveryObjectAlias);

			var undeliveryKindSubquery = QueryOver.Of(() => undeliveryKindAlias)
				.Where(() => undeliveryKindAlias.UndeliveryObject.Id == undeliveryObjectAlias.Id && !undeliveryKindAlias.IsArchive)
				.Select(CustomProjections.GroupConcat(() => undeliveryKindAlias.Name,
					orderByExpression: () => undeliveryKindAlias.Name, separator: ", "));

			if(!FilterViewModel.IsArchive)
			{
				itemsQuery.Where(x => !x.IsArchive);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => undeliveryObjectAlias.Id,
				() => undeliveryObjectAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => undeliveryObjectAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => undeliveryObjectAlias.Name).WithAlias(() => resultAlias.Name)
					.SelectSubQuery(undeliveryKindSubquery).WithAlias(() => resultAlias.UndeliveryKinds)
					.Select(() => undeliveryObjectAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
				)
				.TransformUsing(Transformers.AliasToBean<UndeliveryObjectJournalNode>());

			return itemsQuery;
		};

		protected override Func<UndeliveryObjectViewModel> CreateDialogFunction => () =>
			new UndeliveryObjectViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<UndeliveryObjectJournalNode, UndeliveryObjectViewModel> OpenDialogFunction =>
			(node) => new UndeliveryObjectViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
