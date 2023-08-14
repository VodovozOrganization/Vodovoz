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
	public class UndeliveryKindJournalViewModel : FilterableSingleEntityJournalViewModelBase<UndeliveryKind, UndeliveryKindViewModel, UndeliveryKindJournalNode, UndeliveryKindJournalFilterViewModel>
	{
		public UndeliveryKindJournalViewModel(
			UndeliveryKindJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Виды недовозов";

			UpdateOnChanges(
				typeof(UndeliveryKind),
				typeof(UndeliveryObject)
				);
		}

		protected override Func<IUnitOfWork, IQueryOver<UndeliveryKind>> ItemsSourceQueryFunction => (uow) =>
		{
			UndeliveryKind undeliveryKindAlias = null;
			UndeliveryObject undeliveryObjectAlias = null;
			UndeliveryKindJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => undeliveryKindAlias)
				.Left.JoinAlias(x => x.UndeliveryObject, () => undeliveryObjectAlias);

			if(FilterViewModel.UndeliveryObject != null)
			{
				itemsQuery.Where(x => x.UndeliveryObject.Id == FilterViewModel.UndeliveryObject.Id);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => undeliveryKindAlias.Id,
				() => undeliveryKindAlias.Name,
				() => undeliveryObjectAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.SelectGroup(() => undeliveryKindAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => undeliveryKindAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => undeliveryKindAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(() => undeliveryObjectAlias.Name).WithAlias(() => resultAlias.UndeliveryObject)
				)
				.TransformUsing(Transformers.AliasToBean<UndeliveryKindJournalNode>());

			return itemsQuery;
		};

		protected override void CreateNodeActions()
		{
			CreateDefaultAddActions();
			CreateDefaultEditAction();
			CreateDefaultSelectAction();
		}

		protected override Func<UndeliveryKindViewModel> CreateDialogFunction => () =>
			new UndeliveryKindViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<UndeliveryKindJournalNode, UndeliveryKindViewModel> OpenDialogFunction =>
			(node) => new UndeliveryKindViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
