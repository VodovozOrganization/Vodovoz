using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.DataLoader.Hierarchy;
using QS.Project.Search;
using QS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
		private IEnumerable<ProductGroupsJournalNode> _editableNodes;

		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly HierarchicalChunkLinqLoader<ProductGroup, ProductGroupsJournalNode> _hierarchicalChunkLinqLoader;
		private readonly Type[] _domainObjectsTypes;
		private readonly Dictionary<Type, IPermissionResult> _domainObjectsPermissions;
		private readonly ProductGroupsJournalFilterViewModel _filter;
		private readonly IInteractiveService _interactiveService;
		private readonly INavigationManager _navigationManager;
		private readonly ICommonServices _commonServices;

		public ProductGroupsJournalViewModel(
			ProductGroupsJournalFilterViewModel filter,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICommonServices commonServices,
			Action<ProductGroupsJournalFilterViewModel> filterAction = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			if(filterAction != null)
			{
				filter.SetAndRefilterAtOnce(filterAction);
			}

			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
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

			SelectionMode = JournalSelectionMode.Multiple;

			UpdateOnChanges(_domainObjectsTypes);

			(Search as SearchViewModel).PropertyChanged += OnSearchPropertyChanged;
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		public bool IsGroupSelectionMode { get; set; }

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
			CreateChangeParentGroupAction();
		}

		private void CreateSelectAction()
		{
			var selectAction = new JournalAction("Выбрать",
				(selected) =>
					selected.Any()
					&& selected.Cast<ProductGroupsJournalNode>().All(x => x.JournalNodeType == _productGroupType),
				(selected) => IsGroupSelectionMode,
				(selected) => OnItemsSelected(selected)
			);

			if(IsGroupSelectionMode)
			{
				RowActivatedAction = selectAction;
			}

			NodeActionsList.Add(selectAction);
		}

		protected void CreateAddActions()
		{
			var createAction = new JournalAction("Добавить",
				(selected) => _domainObjectsPermissions[_productGroupType].CanCreate,
				(selected) => !IsGroupSelectionMode,
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
				(selected) => !IsGroupSelectionMode,
				(selected) =>
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
				});

			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}

		private void CreateChangeParentGroupAction()
		{
			var editAction = new JournalAction("Перенос в другую группу",
				(selected) =>
				{
					if(!selected.Any() || !selected.All(n => n is ProductGroupsJournalNode))
					{
						return false;
					}

					var selectedNodes = selected.Cast<ProductGroupsJournalNode>();

					return
					(selectedNodes.All(n => n.JournalNodeType == _productGroupType)
						&& _domainObjectsPermissions[_productGroupType].CanUpdate)
					|| (selected.Cast<ProductGroupsJournalNode>().All(n => n.JournalNodeType == _nomenclatureType)
						&& _domainObjectsPermissions[_nomenclatureType].CanUpdate);
				},
				(selected) => !IsGroupSelectionMode,
				(selected) =>
				{
					_editableNodes = selected.Cast<ProductGroupsJournalNode>();

					var selectGroupPage = _navigationManager.OpenViewModel<ProductGroupsJournalViewModel>(
						this,
						OpenPageOptions.AsSlave,
						viewModel =>
						{
							viewModel.IsGroupSelectionMode = true;
							viewModel.SelectionMode = JournalSelectionMode.Single;
							viewModel.OnSelectResult += OnParentGroupSelected;
						});
				});

			NodeActionsList.Add(editAction);
		}

		private void OnParentGroupSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedParentProductGroup = e.SelectedObjects.FirstOrDefault();

			if(selectedParentProductGroup == null)
			{
				return;
			}

			if(!(selectedParentProductGroup is ProductGroupsJournalNode newParentGroupNode)
				|| newParentGroupNode.JournalNodeType != _productGroupType)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Выбрана не товарная группа!");
				return;
			}

			var newParentGroup = UoW.GetById<ProductGroup>(newParentGroupNode.Id);

			if(_editableNodes is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Не выбраны строки, которым нужно установить новую родительскую группу");
				return;
			}

			if(!_editableNodes.All(n => n.JournalNodeType == _productGroupType)
				&& !_editableNodes.All(n => n.JournalNodeType == _nomenclatureType))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "При редактировании родительской группы должны быть выбраны строки одного типа");
				return;
			}

			var editableNodesIds = _editableNodes.Select(n => n.Id);
			var editableNodesType = _editableNodes.FirstOrDefault().JournalNodeType;

			if(editableNodesType == _productGroupType)
			{
				var productGroups =
					(from productGroup in UoW.Session.Query<ProductGroup>()
					 where
					 editableNodesIds.Contains(productGroup.Id)
					 select productGroup)
					.ToList();

				foreach(var group in productGroups)
				{
					group.Parent = newParentGroup;

					if(ProductGroup.CheckCircle(group, newParentGroup))
					{
						_interactiveService.ShowMessage(ImportanceLevel.Error, "Обнаружена циклическая ссылка. Операция не возможна");
						return;
					}

					UoW.Save(group);
				}

				UoW.Commit();
			}
			else if(editableNodesType == _nomenclatureType)
			{
				var nomenclatures =
					(from nomenclature in UoW.Session.Query<Nomenclature>()
					 where
					 editableNodesIds.Contains(nomenclature.Id)
					 select nomenclature)
					.ToList();

				foreach(var nomenclature in nomenclatures)
				{
					nomenclature.ProductGroup = newParentGroup;
					UoW.Save(nomenclature);
				}

				UoW.Commit();
			}
			else
			{
				throw new InvalidOperationException("Выбран неизвестный тип строки журнала");
			}

			_editableNodes = null;

			Refresh();
		}

		private void OnSearchPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Search.SearchValues))
			{
				_filter.SearchString = string.Join(" ", Search.SearchValues);
			}
		}

		public override void Dispose()
		{
			_filter.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
