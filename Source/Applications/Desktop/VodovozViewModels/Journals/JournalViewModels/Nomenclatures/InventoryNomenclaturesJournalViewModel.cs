using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures
{
	public class InventoryNomenclaturesJournalViewModel : JournalViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly ILifetimeScope _scope;
		private NomenclatureFilterViewModel _filterViewModel;

		public InventoryNomenclaturesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ILifetimeScope scope,
			Action<NomenclatureFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, commonServices?.InteractiveService, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			var dataLoader = new ThreadDataLoader<NomenclatureJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			Title = "Журнал номенклатур с инвентарным учетом";
			CreateFilter(filterParams);
			CreateNodeActions();
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();
			CreateDefaultEditAction();
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
		
		private void CreateDefaultEditAction()
		{
			var permissionResult = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Nomenclature));
			var editAction = new JournalAction("Изменить",
				selected =>
				{
					var selectedNodes = selected.OfType<NomenclatureJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					
					return permissionResult.CanRead;
				},
				selected => true,
				selected =>
				{
					var selectedNodes = selected.OfType<NomenclatureJournalNode>();
					
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}

					var selectedNode = selectedNodes.First();
					NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForOpen(selectedNode.Id));
				}
			);
			if(SelectionMode == JournalSelectionMode.None) {
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}
	}
}
