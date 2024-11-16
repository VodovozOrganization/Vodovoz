using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate.Linq;
using QS.Deletion;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.DataLoader.Hierarchy;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Goods.ProductGroups
{
	public class ProductGroupsJournalViewModel : JournalViewModelBase
	{
		private readonly Type _productGroupType = typeof(ProductGroup);
		private readonly Type _nomenclatureType = typeof(Nomenclature);
		private IEnumerable<ProductGroupsJournalNode> _groupNodes = new List<ProductGroupsJournalNode>();
		private IEnumerable<ProductGroupsJournalNode> _nomenclatureNodes = new List<ProductGroupsJournalNode>();

		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly HierarchicalChunkLinqLoader<ProductGroup, ProductGroupsJournalNode> _hierarchicalChunkLinqLoader;
		private readonly Type[] _domainObjectsTypes;
		private readonly Dictionary<Type, IPermissionResult> _domainObjectsPermissions;
		private readonly ProductGroupsJournalFilterViewModel _filter;
		private readonly ICommonServices _commonServices;

		public ProductGroupsJournalViewModel(
			ProductGroupsJournalFilterViewModel filter,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICommonServices commonServices,
			Action<ProductGroupsJournalFilterViewModel> filterAction = null)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			if(interactiveService is null)
			{
				throw new ArgumentNullException(nameof(interactiveService));
			}

			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			if(filterAction != null)
			{
				filter.SetAndRefilterAtOnce(filterAction);
			}

			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_currentPermissionService = _commonServices.CurrentPermissionService;

			JournalFilter = _filter;
			_filter.OnFiltered += OnFilterViewModelFiltered;

			Title = "Журнал групп товаров";

			_domainObjectsTypes = new Type[]
			{
				_productGroupType,
				_nomenclatureType
			};

			SearchEnabled = false;

			_hierarchicalChunkLinqLoader = new HierarchicalChunkLinqLoader<ProductGroup, ProductGroupsJournalNode>(UnitOfWorkFactory);

			RecuresiveConfig = _hierarchicalChunkLinqLoader.TreeConfig;

			_hierarchicalChunkLinqLoader.SetRecursiveModel(GetChunk);

			var threadDataLoader = new ThreadDataLoader<ProductGroupsJournalNode>(unitOfWorkFactory);
			threadDataLoader.QueryLoaders.Add(_hierarchicalChunkLinqLoader);

			DataLoader = threadDataLoader;
			DataLoader.DynamicLoadingEnabled = false;

			_domainObjectsPermissions = new Dictionary<Type, IPermissionResult>();
			InitializePermissionsMatrix();

			CreateNodeActions();
			CreatePopupActions();
			UpdateOnChanges(_domainObjectsTypes);
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		private void InitializePermissionsMatrix()
		{
			foreach(var domainObject in _domainObjectsTypes)
			{
				_domainObjectsPermissions.Add(domainObject, _currentPermissionService.ValidateEntityPermission(domainObject));
			}
		}

		private void UpdateGroupAndNomenclatureNodes(IUnitOfWork unitOfWork)
		{
			var searchString = _filter.SqlSearchString;

			_groupNodes =
				(from productGroup in unitOfWork.GetAll<ProductGroup>()
				 where
				 (string.IsNullOrWhiteSpace(searchString)
					 || productGroup.Name.ToLower().Like(searchString)
					 || productGroup.Id.ToString().Like(searchString))
				 && (!_filter.IsHideArchived || !productGroup.IsArchive)
				 orderby productGroup.Id
				 select new ProductGroupsJournalNode
				 {
					 Id = productGroup.Id,
					 Name = productGroup.Name,
					 ParentId = productGroup.Parent.Id,
					 IsArchive = productGroup.IsArchive,
					 JournalNodeType = _productGroupType
				 })
				 .ToList();

			_nomenclatureNodes =
				(from nomenclature in unitOfWork.GetAll<Nomenclature>()
				 where
				 (string.IsNullOrWhiteSpace(searchString)
					 || nomenclature.Name.ToLower().Like(searchString)
					 || nomenclature.Id.ToString().Like(searchString))
				 && (!_filter.IsHideArchived || !nomenclature.IsArchive)
				 orderby nomenclature.Id
				 select new ProductGroupsJournalNode
				 {
					 Id = nomenclature.Id,
					 Name = nomenclature.Name,
					 ParentId = nomenclature.ProductGroup.Id,
					 IsArchive = nomenclature.IsArchive,
					 JournalNodeType = _nomenclatureType
				 })
				.ToList();
		}

		private IQueryable<ProductGroupsJournalNode> GetChunk(IUnitOfWork unitOfWork, int? parentId)
		{
			UpdateGroupAndNomenclatureNodes(unitOfWork);

			return GetSubNodes(parentId);
		}

		private IQueryable<ProductGroupsJournalNode> GetSubNodes(int? parentId)
		{
			if(!_filter.IsSearchStringEmpty && parentId != null)
			{
				return Enumerable.Empty<ProductGroupsJournalNode>().AsQueryable();
			}

			var nodes =
				GetGroups(parentId)
				.Concat(GetNomenclatures(parentId));

			return nodes.AsQueryable();
		}

		private IEnumerable<ProductGroupsJournalNode> GetGroups(int? parentId)
		{
			var groups =
				from productGroup in _groupNodes
				where
					(_filter.IsSearchStringEmpty && productGroup.ParentId == parentId)
					|| !_filter.IsSearchStringEmpty

				let children = GetSubNodes(productGroup.Id)

				orderby productGroup.Id
				select new ProductGroupsJournalNode
				{
					Id = productGroup.Id,
					Name = productGroup.Name,
					ParentId = productGroup.ParentId,
					IsArchive = productGroup.IsArchive,
					JournalNodeType = _productGroupType,
					Children = children.ToList()
				};

			return groups;
		}

		private IEnumerable<ProductGroupsJournalNode> GetNomenclatures(int? parentId)
		{
			if(!_filter.IsSearchStringEmpty && parentId != null)
			{
				return Enumerable.Empty<ProductGroupsJournalNode>();
			}

			var nomenclatures =
				from nomenclature in _nomenclatureNodes
				where
					(_filter.IsSearchStringEmpty && nomenclature.ParentId == parentId)
					|| !_filter.IsSearchStringEmpty
				orderby nomenclature.Id
				select new ProductGroupsJournalNode
				{
					Id = nomenclature.Id,
					Name = nomenclature.Name,
					ParentId = nomenclature.ParentId,
					IsArchive = nomenclature.IsArchive,
					JournalNodeType = _nomenclatureType
				};

			return nomenclatures;
		}

		void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();

			CreateSelectAction();
			CreateAddActions();
			CreateEditAction();
		}

		private void CreateSelectAction()
		{
			var selectAction = new JournalAction("Выбрать",
				(selected) => selected.Any()
					&& selected.Cast<ProductGroupsJournalNode>().All(x => x.JournalNodeType == _productGroupType),
				(selected) => SelectionMode != JournalSelectionMode.None,
				(selected) => OnItemsSelected(selected)
			);

			if(SelectionMode == JournalSelectionMode.Single
				|| SelectionMode == JournalSelectionMode.Multiple)
			{
				RowActivatedAction = selectAction;
			}

			NodeActionsList.Add(selectAction);
		}

		protected void CreateAddActions()
		{
			var createAction = new JournalAction("Добавить",
				(selected) => _domainObjectsPermissions[_productGroupType].CanCreate,
				(selected) => true,
				(selected) =>
				{
					var page = NavigationManager.OpenViewModel<ProductGroupViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
				});

			NodeActionsList.Add(createAction);
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => selected.Length == 1
					&& selected.FirstOrDefault() is ProductGroupsJournalNode node
					&& _domainObjectsPermissions[node.JournalNodeType].CanUpdate
					&& selected.Any(),
				(selected) => true,
				EditNodeAction);

			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}

		private void EditNodeAction(object[] selected)
		{
			if(selected.FirstOrDefault() is ProductGroupsJournalNode node)
			{
				if(node.JournalNodeType == _productGroupType)
				{
					NavigationManager.OpenViewModel<ProductGroupViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
					return;
				}

				if(node.JournalNodeType == _nomenclatureType)
				{
					NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
					return;
				}
			}
		}

		public override void Dispose()
		{
			_filter.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
