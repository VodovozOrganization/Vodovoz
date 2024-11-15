using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
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

namespace Vodovoz.ViewModels.Goods.ProductGroups
{
	public class ProductGroupsJournalViewModel : JournalViewModelBase
	{
		private readonly Type _productGroupType = typeof(ProductGroup);
		private readonly Type _nomenclatureType = typeof(Nomenclature);

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

			(Search as SearchViewModel).PropertyChanged += OnSearchPropertyChanged;
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		private void InitializePermissionsMatrix()
		{
			foreach(var domainObject in _domainObjectsTypes)
			{
				_domainObjectsPermissions.Add(domainObject, _currentPermissionService.ValidateEntityPermission(domainObject));
			}
		}

		private void OnSearchPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Search.SearchValues))
			{
				_filter.SearchString = string.Join(" ", Search.SearchValues);
			}
		}

		private IQueryable<ProductGroupsJournalNode> GetChunk(IUnitOfWork unitOfWork, int? parentId) =>
			GetSubGroup(unitOfWork, parentId);

		private IQueryable<ProductGroupsJournalNode> GetSubGroup(IUnitOfWork unitOfWork, int? parentId)
		{
			var searchString = string.IsNullOrWhiteSpace(_filter.SearchString) ? string.Empty : $"%{_filter.SearchString.ToLower()}%";

			if(string.IsNullOrWhiteSpace(searchString) || parentId == null)
			{
				return Enumerable.Empty<ProductGroupsJournalNode>().AsQueryable();
			}

			var groupNodes =
				(from productGroup in unitOfWork.GetAll<ProductGroup>()
				 where
				 (string.IsNullOrWhiteSpace(searchString) && productGroup.Parent.Id == parentId)
					 || productGroup.Name.ToLower().Like(searchString)
					 || productGroup.Id.ToString().Like(searchString)
				 && (!_filter.IsHideArchived || !productGroup.IsArchive)

				 let children = GetSubGroup(unitOfWork, productGroup.Id)

				 orderby productGroup.Name, productGroup.Id
				 select new ProductGroupsJournalNode
				 {
					 Id = productGroup.Id,
					 Name = productGroup.Name,
					 ParentId = productGroup.Parent.Id,
					 IsArchive = productGroup.IsArchive,
					 JournalNodeType = _productGroupType,
					 Children = children.ToList()
				 })
				 .ToList()
				 .Concat(
					(string.IsNullOrWhiteSpace(searchString) || parentId == null)
					? GetNomenclatures(unitOfWork, parentId, searchString).ToList()
					: Enumerable.Empty<ProductGroupsJournalNode>());

			return groupNodes.AsQueryable();
		}

		private IQueryable<ProductGroupsJournalNode> GetNomenclatures(IUnitOfWork unitOfWork, int? parentId, string searchString) =>
			from nomenclature in unitOfWork.GetAll<Nomenclature>()
			where
				((!string.IsNullOrWhiteSpace(searchString) && parentId == null) || nomenclature.ProductGroup.Id == parentId)
				&& (!_filter.IsHideArchived || !nomenclature.IsArchive)
				&& (string.IsNullOrWhiteSpace(searchString) || nomenclature.Name.ToLower().Like(searchString)
					|| nomenclature.Id.ToString().Like(searchString))
			orderby nomenclature.Name, nomenclature.Id
			select new ProductGroupsJournalNode
			{
				Id = nomenclature.Id,
				Name = nomenclature.Name,
				ParentId = nomenclature.ProductGroup.Id,
				IsArchive = nomenclature.IsArchive,
				JournalNodeType = _nomenclatureType
			};
	}
}
