using FluentNHibernate.Conventions;
using Gamma.Binding.Core.RecursiveTreeConfig;
using MoreLinq;
using MoreLinq.Extensions;
using NHibernate.Linq;
using NHibernate.Util;
using QS.Deletion;
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
using System.Windows.Input;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public class FinancialCategoriesGroupsJournalViewModel : JournalViewModelBase
	{
		private readonly Type _financialCategoriesGroupType = typeof(FinancialCategoriesGroup);
		private readonly Type _financialIncomeCategoryType = typeof(FinancialIncomeCategory);
		private readonly Type _financialExpenseCategoryType = typeof(FinancialExpenseCategory);
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly FinancialCategoriesJournalFilterViewModel _filter;
		private readonly HierarchicalChunkLinqLoader<FinancialCategoriesGroup, FinancialCategoriesJournalNode> _hierarchicalChunkLinqLoader;
		private readonly Type[] _domainObjectsTypes;
		private readonly Dictionary<Type, IPermissionResult> _domainObjectsPermissions;
		private readonly bool _hasAccessToHiddenFinancialCategories;

		public FinancialCategoriesGroupsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			INavigationManager navigation,
			ICommonServices commonServices,
			FinancialCategoriesJournalFilterViewModel filter,
			Action<FinancialCategoriesJournalFilterViewModel> filterAction = null)
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

			if(commonServices is null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			if(filterAction != null)
			{
				filter.SetAndRefilterAtOnce(filterAction);
			}

			filter.JournalViewModel = this;
			JournalFilter = filter;

			_currentPermissionService = currentPermissionService
				?? throw new ArgumentNullException(nameof(currentPermissionService));
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
			_filter.OnFiltered += OnFilterViewModelFiltered;

			Title = "Журнал финансовых статей";

			_domainObjectsTypes = new Type[]
			{
				_financialCategoriesGroupType,
				_financialIncomeCategoryType,
				_financialExpenseCategoryType
			};

			SearchEnabled = false;

			_hierarchicalChunkLinqLoader = new HierarchicalChunkLinqLoader<FinancialCategoriesGroup, FinancialCategoriesJournalNode>(UnitOfWorkFactory);

			RecuresiveConfig = _hierarchicalChunkLinqLoader.TreeConfig;

			_hierarchicalChunkLinqLoader.SetRecursiveModel(GetChunk);

			var threadDataLoader = new ThreadDataLoader<FinancialCategoriesJournalNode>(unitOfWorkFactory);
			threadDataLoader.QueryLoaders.Add(_hierarchicalChunkLinqLoader);

			DataLoader = threadDataLoader;
			DataLoader.DynamicLoadingEnabled = false;

			_domainObjectsPermissions = new Dictionary<Type, IPermissionResult>();
			_hasAccessToHiddenFinancialCategories = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.FinancialCategory.HasAccessToHiddenFinancialCategories);

			InitializePermissionsMatrix();
			CreateNodeActions();
			CreatePopupActions();
			UpdateOnChanges(_domainObjectsTypes);

			(Search as SearchViewModel).PropertyChanged += OnSearchPropertyChanged;
		}

		private void OnSearchPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Search.SearchValues))
			{
				_filter.SearchString = string.Join(" ", Search.SearchValues);
			}
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		public override string FooterInfo
		{
			get => DataLoader.TotalCount.HasValue ? $" | Загружено: {DataLoader.TotalCount.Value}" : "";

			set => base.FooterInfo = value;
		}

		private IQueryable<FinancialCategoriesJournalNode> GetChunk(IUnitOfWork unitOfWork, int? parentId) =>
			GetSubGroup(unitOfWork, (_filter.ParentFinancialGroup?.Id != null) ? _filter.ParentFinancialGroup?.Id : parentId);

		private IQueryable<FinancialCategoriesJournalNode> GetSubGroup(IUnitOfWork unitOfWork, int? parentId)
		{
			var searchString = string.IsNullOrWhiteSpace(_filter.SearchString) ? string.Empty : $"%{_filter.SearchString.ToLower()}%";

			var subdivisionId = _filter.Subdivision?.Id ?? -1;

			return (string.IsNullOrWhiteSpace(searchString) || parentId == null)
				? PostFiltering(
					(from financialCategoriesGroup in unitOfWork.GetAll<FinancialCategoriesGroup>()
					 where ((string.IsNullOrWhiteSpace(searchString) && financialCategoriesGroup.ParentId == parentId)
							 || financialCategoriesGroup.Title.ToLower().Like(searchString)
							 || financialCategoriesGroup.Id.ToString().Like(searchString))
						 && !_filter.ExcludeFinancialGroupsIds.Contains(financialCategoriesGroup.Id)
						 && (_filter.RestrictNodeTypes.IsEmpty() || _filter.RestrictNodeTypes.Contains(_financialCategoriesGroupType))
						 && (_filter.ShowArchive || !financialCategoriesGroup.IsArchive)
						 && (!financialCategoriesGroup.IsHiddenFromPublicAccess || _hasAccessToHiddenFinancialCategories == financialCategoriesGroup.IsHiddenFromPublicAccess)
						 && (_filter.RestrictFinancialSubtype == null || _filter.RestrictFinancialSubtype == financialCategoriesGroup.FinancialSubtype)
						 && (!_filter.RestrictNodeSelectTypes.Any() || string.IsNullOrWhiteSpace(searchString) || _filter.RestrictNodeSelectTypes.Contains(_financialCategoriesGroupType))
					 let children = GetSubGroup(unitOfWork, financialCategoriesGroup.Id)
					 orderby financialCategoriesGroup.Numbering, financialCategoriesGroup.Title
					 select new FinancialCategoriesJournalNode
					 {
						 Id = financialCategoriesGroup.Id,
						 ParentId = parentId,
						 Numbering = financialCategoriesGroup.Numbering,
						 Name = financialCategoriesGroup.Title,
						 JournalNodeType = _financialCategoriesGroupType,
						 FinancialSubType = financialCategoriesGroup.FinancialSubtype,
						 Children = children.ToList()
					 })
					.ToList()
					.Concat(
						(_filter.RestrictNodeTypes.IsEmpty()
							|| _filter.RestrictNodeTypes.Contains(_financialIncomeCategoryType))
						&& (string.IsNullOrWhiteSpace(searchString) || parentId == null)
						? GetIncomeCategories(unitOfWork, parentId, searchString, subdivisionId).ToList() : Enumerable.Empty<FinancialCategoriesJournalNode>())
					.Concat(
						(_filter.RestrictNodeTypes.IsEmpty()
							|| _filter.RestrictNodeTypes.Contains(_financialExpenseCategoryType))
						&& (string.IsNullOrWhiteSpace(searchString) || parentId == null)
						? GetExpenseCategories(unitOfWork, parentId, searchString, subdivisionId).ToList() : Enumerable.Empty<FinancialCategoriesJournalNode>())
					.ToList())
				: Enumerable.Empty<FinancialCategoriesJournalNode>().AsQueryable();
		}

		private IQueryable<FinancialCategoriesJournalNode> GetIncomeCategories(IUnitOfWork unitOfWork, int? parentId, string searchString, int subdivisionId) =>
			from incomeCategory in unitOfWork.GetAll<FinancialIncomeCategory>()
			where ((!string.IsNullOrWhiteSpace(searchString) && parentId == null) || incomeCategory.ParentId == parentId)
				&& (_filter.ShowArchive || !incomeCategory.IsArchive)
				&& (!incomeCategory.IsHiddenFromPublicAccess || _hasAccessToHiddenFinancialCategories == incomeCategory.IsHiddenFromPublicAccess)
				&& (string.IsNullOrWhiteSpace(searchString) || incomeCategory.Title.ToLower().Like(searchString)
					|| incomeCategory.Id.ToString().Like(searchString))
				&& (_filter.TargetDocument == null || _filter.TargetDocument == incomeCategory.TargetDocument)
				&& (_filter.Subdivision == null || incomeCategory.SubdivisionId == subdivisionId)
				&& (_filter.RestrictFinancialSubtype == null || _filter.RestrictFinancialSubtype == incomeCategory.FinancialSubtype)
				&& (!_filter.RestrictNodeSelectTypes.Any() || _filter.RestrictNodeSelectTypes.Contains(_financialIncomeCategoryType))
			orderby incomeCategory.Numbering, incomeCategory.Title
			select new FinancialCategoriesJournalNode
			{
				Id = incomeCategory.Id,
				Numbering = incomeCategory.Numbering,
				Name = incomeCategory.Title,
				JournalNodeType = _financialIncomeCategoryType,
				FinancialSubType = incomeCategory.FinancialSubtype,
				ParentId = parentId,
			};

		private IQueryable<FinancialCategoriesJournalNode> GetExpenseCategories(IUnitOfWork unitOfWork, int? parentId, string searchString, int subdivisionId) =>
			from expenseCategory in unitOfWork.GetAll<FinancialExpenseCategory>()
			where ((!string.IsNullOrWhiteSpace(searchString) && parentId == null) || expenseCategory.ParentId == parentId)
				&& (!_filter.IncludeExpenseCategoryIds.Any() || _filter.IncludeExpenseCategoryIds.Contains(expenseCategory.Id))
				&& (_filter.ShowArchive || !expenseCategory.IsArchive)
				&& (!expenseCategory.IsHiddenFromPublicAccess || _hasAccessToHiddenFinancialCategories == expenseCategory.IsHiddenFromPublicAccess)
				&& (string.IsNullOrWhiteSpace(searchString) || expenseCategory.Title.ToLower().Like(searchString)
					|| expenseCategory.Id.ToString().Like(searchString))
				&& (_filter.TargetDocument == null || _filter.TargetDocument == expenseCategory.TargetDocument)
				&& (_filter.Subdivision == null || expenseCategory.SubdivisionId == subdivisionId)
				&& (_filter.RestrictFinancialSubtype == null || _filter.RestrictFinancialSubtype == expenseCategory.FinancialSubtype)
				&& (!_filter.RestrictNodeSelectTypes.Any() || _filter.RestrictNodeSelectTypes.Contains(_financialExpenseCategoryType))
			orderby expenseCategory.Numbering, expenseCategory.Title
			select new FinancialCategoriesJournalNode
			{
				Id = expenseCategory.Id,
				Numbering = expenseCategory.Numbering,
				Name = expenseCategory.Title,
				JournalNodeType = _financialExpenseCategoryType,
				FinancialSubType = expenseCategory.FinancialSubtype,
				ParentId = parentId,
			};

		public IQueryable<FinancialCategoriesJournalNode> PostFiltering(IList<FinancialCategoriesJournalNode> financialCategoriesJournalNodes)
		{
			if(_filter.HideEmptyGroups)
			{
				for(int i = financialCategoriesJournalNodes.Count - 1; i >= 0; i--)
				{
					if(financialCategoriesJournalNodes[i].JournalNodeType == _financialCategoriesGroupType
						&& financialCategoriesJournalNodes[i].Children.Count == 0)
					{
						financialCategoriesJournalNodes.RemoveAt(i);
					}
				}
			}

			return financialCategoriesJournalNodes.AsQueryable();
		}

		private void InitializePermissionsMatrix()
		{
			foreach(var domainObject in _domainObjectsTypes)
			{
				_domainObjectsPermissions.Add(domainObject, _currentPermissionService.ValidateEntityPermission(domainObject));
			}
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
			CreateDeleteAction();
		}

		private void CreateSelectAction()
		{
			var selectAction = new JournalAction("Выбрать",
				(selected) => selected.Any()
					&& (_filter.RestrictNodeSelectTypes.IsEmpty()
						|| selected.Cast<FinancialCategoriesJournalNode>()
							.All(x => _filter.RestrictNodeSelectTypes.Contains(x.JournalNodeType))),
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

		private void CreateDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				(selected) => selected.Length == 1
					&& selected.FirstOrDefault() is FinancialCategoriesJournalNode node
					&& _domainObjectsPermissions[node.JournalNodeType].CanDelete
					&& selected.Any(),
				(selected) => true,
				(selected) =>
				{
					if(selected.FirstOrDefault() is FinancialCategoriesJournalNode node
						&& _domainObjectsPermissions[node.JournalNodeType].CanDelete)
					{
						DeleteHelper.DeleteEntity(node.JournalNodeType, node.Id);
					}
				},
				Key.Delete.ToString());
			NodeActionsList.Add(deleteAction);
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => selected.Length == 1
					&& selected.FirstOrDefault() is FinancialCategoriesJournalNode node
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

		protected void CreateAddActions()
		{
			var addParentNodeAction = new JournalAction("Добавить", (selected) => true, (selected) => true, (selected) => { });
			foreach(var objectType in _domainObjectsTypes)
			{
				var childNodeAction = new JournalAction(objectType.GetClassUserFriendlyName().Accusative.CapitalizeSentence(),
					(selected) => _domainObjectsPermissions[objectType].CanCreate
						&& selected.Length == 1
						&& selected.FirstOrDefault() is FinancialCategoriesJournalNode node
						&& node.JournalNodeType == _financialCategoriesGroupType
						&& ((node.FinancialSubType == FinancialSubType.Income && objectType == _financialIncomeCategoryType)
							|| (node.FinancialSubType == FinancialSubType.Expense && objectType == _financialExpenseCategoryType)
							|| objectType == _financialCategoriesGroupType),
					(selected) => _domainObjectsPermissions[objectType].CanCreate
						&& ((selected.FirstOrDefault() is FinancialCategoriesJournalNode node
							&& ((objectType == _financialIncomeCategoryType && node.FinancialSubType == FinancialSubType.Income)
							|| (objectType == _financialExpenseCategoryType && node.FinancialSubType == FinancialSubType.Expense)
							|| objectType == _financialCategoriesGroupType))
							|| selected.IsEmpty()),
					(selected) =>
					{
						if(selected.FirstOrDefault() is FinancialCategoriesJournalNode node)
						{
							if(objectType == _financialCategoriesGroupType)
							{
								var page = NavigationManager.OpenViewModel<FinancialCategoriesGroupViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
								page.ViewModel.Entity.ParentId = node.Id;
								page.ViewModel.Entity.FinancialSubtype = node.FinancialSubType;
								return;
							}

							if(objectType == _financialIncomeCategoryType)
							{
								var page = NavigationManager.OpenViewModel<FinancialIncomeCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
								page.ViewModel.Entity.ParentId = node.Id;
								return;
							}

							if(objectType == _financialExpenseCategoryType)
							{
								var page = NavigationManager.OpenViewModel<FinancialExpenseCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
								page.ViewModel.Entity.ParentId = node.Id;
								return;
							}
						}
					}
				);
				addParentNodeAction.ChildActionsList.Add(childNodeAction);
			}

			NodeActionsList.Add(addParentNodeAction);
		}

		private void EditNodeAction(object[] selected)
		{
			if(selected.FirstOrDefault() is FinancialCategoriesJournalNode node)
			{
				if(node.JournalNodeType == _financialCategoriesGroupType)
				{
					NavigationManager.OpenViewModel<FinancialCategoriesGroupViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
					return;
				}

				if(node.JournalNodeType == _financialIncomeCategoryType)
				{
					NavigationManager.OpenViewModel<FinancialIncomeCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
					return;
				}

				if(node.JournalNodeType == _financialExpenseCategoryType)
				{
					NavigationManager.OpenViewModel<FinancialExpenseCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
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
