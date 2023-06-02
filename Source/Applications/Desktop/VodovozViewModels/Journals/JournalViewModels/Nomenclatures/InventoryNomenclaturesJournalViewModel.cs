using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures
{
	public class InventoryNomenclaturesJournalViewModel : JournalViewModelBase
	{
		private readonly ILifetimeScope _scope;
		private NomenclatureFilterViewModel _filterViewModel;

		public InventoryNomenclaturesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ILifetimeScope scope,
			Action<NomenclatureFilterViewModel> filterParams = null) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			var dataLoader = new ThreadDataLoader<NomenclatureJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			Title = "Журнал номенклатур с инвентарным учетом";
			CreateFilter(filterParams);
			CreateNodeActions();
		}

		private void CreateFilter(Action<NomenclatureFilterViewModel> filterParams)
		{
			Autofac.Core.Parameter[] parameters = {
				new TypedParameter(typeof(Action<NomenclatureFilterViewModel>), filterParams)
			};

			_filterViewModel = _scope.Resolve<NomenclatureFilterViewModel>(parameters);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}
		
		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public IQueryOver<Nomenclature> ItemsQuery(IUnitOfWork uow)
		{
			Nomenclature nomenclatureAlias = null;
			NomenclatureJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => nomenclatureAlias);

			//Хардкодим выборку номенклатур для инвентарного учета
			itemsQuery.Where(() => nomenclatureAlias.HasInventoryAccounting);
			
			if(!_filterViewModel.RestrictArchive)
			{
				itemsQuery.Where(() => !nomenclatureAlias.IsArchive);
			}

			if(_filterViewModel.RestrictedExcludedIds != null && _filterViewModel.RestrictedExcludedIds.Any())
			{
				itemsQuery.WhereRestrictionOn(() => nomenclatureAlias.Id).Not.IsInG(_filterViewModel.RestrictedExcludedIds);
			}

			itemsQuery.Where(
				GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Id,
					() => nomenclatureAlias.OnlineStoreExternalId
				)
			);

			if(!_filterViewModel.RestrictDilers)
			{
				itemsQuery.Where(() => !nomenclatureAlias.IsDiler);
			}

			if(_filterViewModel.RestrictCategory == NomenclatureCategory.water)
			{
				itemsQuery.Where(() => nomenclatureAlias.IsDisposableTare == _filterViewModel.RestrictDisposbleTare);
			}

			if(_filterViewModel.RestrictCategory.HasValue)
			{
				itemsQuery.Where(n => n.Category == _filterViewModel.RestrictCategory.Value);
			}

			if(_filterViewModel.SelectCategory.HasValue
			   && _filterViewModel.SelectSaleCategory.HasValue
			   && Nomenclature.GetCategoriesWithSaleCategory().Contains(_filterViewModel.SelectCategory.Value))
			{
				itemsQuery.Where(n => n.SaleCategory == _filterViewModel.SelectSaleCategory);
			}

			if(_filterViewModel.IsDefectiveBottle)
			{
				itemsQuery.Where(x => x.IsDefectiveBottle);
			}
			
			itemsQuery.Where(() => !nomenclatureAlias.IsSerial)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.Category)
					.Select(() => nomenclatureAlias.OnlineStoreExternalId).WithAlias(() => resultAlias.OnlineStoreExternalId)
					.Select(() => false).WithAlias(() => resultAlias.CalculateQtyOnStock))
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>());

			return itemsQuery;
		}
	}
}
