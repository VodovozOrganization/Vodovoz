using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
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
	public class UndeliveryDetalizationJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<UndeliveryDetalization, UndeliveryDetalizationViewModel, UndeliveryDetalizationJournalNode, UndeliveryDetalizationJournalFilterViewModel>
	{
		public UndeliveryDetalizationJournalViewModel(
			UndeliveryDetalizationJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices
		)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{

			TabName = "Детализации видов недовоза";

			UpdateOnChanges(
				typeof(UndeliveryKind),
				typeof(UndeliveryObject),
				typeof(UndeliveryDetalization));
		}

		protected override Func<IUnitOfWork, IQueryOver<UndeliveryDetalization>> ItemsSourceQueryFunction => (uow) =>
		{
			UndeliveryDetalization undeliveryDetalizationAlias = null;
			UndeliveryKind undeliveryKindAlias = null;
			UndeliveryObject undeliveryObjectAlias = null;
			UndeliveryDetalizationJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => undeliveryDetalizationAlias)
				.Left.JoinAlias(x => x.UndeliveryKind, () => undeliveryKindAlias)
				.Left.JoinAlias(() => undeliveryKindAlias.UndeliveryObject, () => undeliveryObjectAlias);

			if(FilterViewModel?.UndeliveryObject != null)
			{
				var undeliveryIbjectId = FilterViewModel.UndeliveryObject.Id;
				itemsQuery.Where(() => undeliveryObjectAlias.Id == undeliveryIbjectId);
			}

			if(FilterViewModel?.UndeliveryKind != null)
			{
				var undeliveryKindId = FilterViewModel.UndeliveryKind.Id;
				itemsQuery.Where(() => undeliveryKindAlias.Id == undeliveryKindId);
			}

			if(FilterViewModel?.HideArchive ?? false)
			{
				itemsQuery.Where(() => undeliveryDetalizationAlias.IsArchive == false);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => undeliveryDetalizationAlias.Id,
				() => undeliveryDetalizationAlias.Name,
				() => undeliveryKindAlias.Name,
				() => undeliveryObjectAlias.Name));

			itemsQuery.OrderBy(x => x.IsArchive).Asc
				.ThenBy(x => x.Id).Asc
				.SelectList(list =>
					list.SelectGroup(() => undeliveryDetalizationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => undeliveryDetalizationAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => undeliveryDetalizationAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
						.Select(() => undeliveryObjectAlias.Name).WithAlias(() => resultAlias.UndeliveryObject)
						.Select(() => undeliveryKindAlias.Name).WithAlias(() => resultAlias.UndeliveryKind))
				.TransformUsing(Transformers.AliasToBean<UndeliveryDetalizationJournalNode>());

			return itemsQuery;
		};

		protected override Func<UndeliveryDetalizationViewModel> CreateDialogFunction => () =>
			new UndeliveryDetalizationViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<UndeliveryDetalizationJournalNode, UndeliveryDetalizationViewModel> OpenDialogFunction =>
			node => new UndeliveryDetalizationViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
