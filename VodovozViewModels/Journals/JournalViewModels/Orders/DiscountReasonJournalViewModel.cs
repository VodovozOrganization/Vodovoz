using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class DiscountReasonJournalViewModel
		: SingleEntityJournalViewModelBase<DiscountReason, DiscountReasonViewModel, DiscountReasonJournalNode>
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly IProductGroupJournalFactory _productGroupJournalFactory;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;

		public DiscountReasonJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDiscountReasonRepository discountReasonRepository,
			IProductGroupJournalFactory productGroupJournalFactory,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false)
			: base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog,	hideJournalForCreateDialog)
		{
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_productGroupJournalFactory = productGroupJournalFactory ?? throw new ArgumentNullException(nameof(productGroupJournalFactory));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));

			TabName = "Журнал оснований для скидки";

			UpdateOnChanges(typeof(DiscountReason));
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<DiscountReason>> ItemsSourceQueryFunction => (uow) =>
		{
			DiscountReason drAlias = null;
			DiscountReasonJournalNode drNodeAlias = null;

			var query = uow.Session.QueryOver(() => drAlias);

			query.Where(GetSearchCriterion(
				() => drAlias.Id,
				() => drAlias.Name));
			
			var result = query.SelectList(list => list
					.Select(dr => dr.Id).WithAlias(() => drNodeAlias.Id)
					.Select(dr => dr.Name).WithAlias(() => drNodeAlias.Name)
					.Select(dr => dr.IsArchive).WithAlias(() => drNodeAlias.IsArchive))
				.OrderBy(dr => dr.IsArchive).Asc
				.OrderBy(dr => dr.Name).Asc
				.TransformUsing(Transformers.AliasToBean<DiscountReasonJournalNode>());
			return result;
		};

		protected override Func<DiscountReasonViewModel> CreateDialogFunction =>
			() => new DiscountReasonViewModel(
				EntityUoWBuilder.ForCreate(),
				QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
				commonServices,
				_discountReasonRepository,
				_productGroupJournalFactory,
				_nomenclatureSelectorFactory);

		protected override Func<DiscountReasonJournalNode, DiscountReasonViewModel> OpenDialogFunction =>
			(node) => new DiscountReasonViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
				commonServices,
				_discountReasonRepository,
				_productGroupJournalFactory,
				_nomenclatureSelectorFactory);
	}
}
