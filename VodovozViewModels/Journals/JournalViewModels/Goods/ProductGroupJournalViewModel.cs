using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class ProductGroupJournalViewModel : FilterableSingleEntityJournalViewModelBase<ProductGroup, ProductGroupViewModel, ProductGroupJournalNode, ProductGroupJournalFilterViewModel>
	{
		private readonly IProductGroupJournalFactory _productGroupJournalFactory;
        public ProductGroupJournalViewModel(
	        EntitiesJournalActionsViewModel journalActionsViewModel,
	        ProductGroupJournalFilterViewModel filterViewModel,
	        IUnitOfWorkFactory unitOfWorkFactory,
	        ICommonServices commonServices,
	        IProductGroupJournalFactory productGroupJournalFactory)
            : base(journalActionsViewModel, filterViewModel, unitOfWorkFactory, commonServices)
        {
	        _productGroupJournalFactory = productGroupJournalFactory ?? throw new ArgumentNullException(nameof(productGroupJournalFactory));
			TabName = "Журнал групп продуктов";
            UpdateOnChanges(typeof(ProductGroup));
        }

        protected override Func<IUnitOfWork, IQueryOver<ProductGroup>> ItemsSourceQueryFunction => (uow) =>
        {
            ProductGroup productGroupAlias = null;
            ProductGroupJournalNode resultAlias = null;

            var itemsQuery = uow.Session.QueryOver(() => productGroupAlias);

            if (FilterViewModel.HideArchive)
            {
	            itemsQuery.Where(() => !productGroupAlias.IsArchive);
            }

            itemsQuery.Where(GetSearchCriterion(
                () => productGroupAlias.Name,
                () => productGroupAlias.Id,
                () => productGroupAlias.Name)
            );

            itemsQuery
                .SelectList(list => list
	                .Select(() => productGroupAlias.Id).WithAlias(() => resultAlias.Id)
	                .Select(() => productGroupAlias.Name).WithAlias(() => resultAlias.Name)
	                .Select(() => productGroupAlias.Parent.Id).WithAlias(() => resultAlias.ParentId)
	                .Select(() => productGroupAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
				)
                .TransformUsing(Transformers.AliasToBean<ProductGroupJournalNode>());

            return itemsQuery;
        };

        protected override Func<ProductGroupViewModel> CreateDialogFunction => () =>
	        new ProductGroupViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, CommonServices, _productGroupJournalFactory);

        protected override Func<JournalEntityNodeBase, ProductGroupViewModel> OpenDialogFunction =>
	        (node) => new ProductGroupViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, CommonServices, _productGroupJournalFactory);
	}
}
