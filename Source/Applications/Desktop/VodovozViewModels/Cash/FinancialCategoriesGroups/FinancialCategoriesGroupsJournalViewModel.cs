using FluentNHibernate.Conventions;
using Gamma.Binding.Core.RecursiveTreeConfig;
using MoreLinq;
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
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Vodovoz.Domain.Cash;
using Vodovoz.Tools;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public class FinancialCategoriesGroupsJournalViewModel : JournalViewModelBase
	{
		private readonly Type _financialCategoriesGroupType = typeof(FinancialCategoriesGroup);
		private readonly Type _financialIncomeCategoryType = typeof(IncomeCategory);
		private readonly Type _financialExpenseCategoryType = typeof(ExpenseCategory);
		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly FinancialCategoriesJournalFilterViewModel _filter;
		private readonly HierarchicalChunkLinqLoader<FinancialCategoriesGroup, FinancialCategoriesJournalNode> _hierarchicalChunkLinqLoader;
		private readonly Type[] _domainObjectsTypes;
		private readonly Dictionary<Type, (bool CanRead, bool CanCreate, bool CanEdit, bool CanDelete)> _domainObjectsPermissions;

		public FinancialCategoriesGroupsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			INavigationManager navigation,
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

			if(filterAction != null)
			{
				filter.SetAndRefilterAtOnce(filterAction);
			}

			filter.JournalViewModel = this;
			JournalFilter = filter;

			_interactiveService = interactiveService;
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

			_domainObjectsPermissions = new Dictionary<Type, (bool CanRead, bool CanCreate, bool CanEdit, bool CanDelete)>();

			InitializePermissionsMatrix();
			CreateNodeActions();
			CreatePopupActions();
			UpdateOnChanges(_domainObjectsTypes);
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		public override string FooterInfo
		{
			get => DataLoader.TotalCount.HasValue ? $" | Загружено: {DataLoader.TotalCount.Value}" : "";

			set => base.FooterInfo = value;
		}

		private IQueryable<FinancialCategoriesJournalNode> GetChunk(IUnitOfWork unitOfWork, int? parentId) =>
			GetSubGroup(unitOfWork, (_filter.ParentFinancialGroup?.Id == null) ? parentId : _filter.ParentFinancialGroup?.Id);

		private IQueryable<FinancialCategoriesJournalNode> GetSubGroup(IUnitOfWork unitOfWork, int? parentId)
		{
			var showFinancialGroups = _filter.SelectableObjectTypes.First(x => x.Type == _financialCategoriesGroupType).Selected;
			var showIncomeCategories = _filter.SelectableObjectTypes.First(x => x.Type == _financialIncomeCategoryType).Selected;
			var showExpenseCategories =  _filter.SelectableObjectTypes.First(x => x.Type == _financialExpenseCategoryType).Selected;

			var titlePart = string.IsNullOrWhiteSpace(_filter.TitlePart) ? string.Empty : $"%{_filter.TitlePart.ToLower()}%";
			var idPart = string.IsNullOrWhiteSpace(_filter.IdPart) ? string.Empty : $"%{_filter.IdPart}%";

			var subdivisionId = _filter.Subdivision?.Id ?? -1;
			var parentFinancialGroupId = _filter.ParentFinancialGroup?.Id ?? -1;

			return (from financialCategoriesGroup in unitOfWork.GetAll<FinancialCategoriesGroup>()
					where financialCategoriesGroup.ParentId == parentId
						&& !_filter.ExcludeFinancialGroupsIds.Contains(financialCategoriesGroup.Id)
						&& (_filter.RestrictNodeTypes.IsEmpty() || _filter.RestrictNodeTypes.Contains(_financialCategoriesGroupType))
						&& (_filter.ShowArchive || !financialCategoriesGroup.IsArchive)
						&& showFinancialGroups
					let children = GetSubGroup(unitOfWork, financialCategoriesGroup.Id)
					select new FinancialCategoriesJournalNode
					{
						Id = financialCategoriesGroup.Id,
						ParentId = parentId,
						Name = financialCategoriesGroup.Title,
						JournalNodeType = _financialCategoriesGroupType,
						Children = children.ToList()
					}).ToList()
				   .Concat(
						(_filter.RestrictNodeTypes.IsEmpty()
							|| _filter.RestrictNodeTypes.Contains(_financialIncomeCategoryType))
						&& showIncomeCategories
						? GetIncomeCategories(unitOfWork, parentId, titlePart, idPart, subdivisionId, parentFinancialGroupId).ToList() : Enumerable.Empty<FinancialCategoriesJournalNode>())
				   .Concat(
						(_filter.RestrictNodeTypes.IsEmpty()
							|| _filter.RestrictNodeTypes.Contains(_financialExpenseCategoryType))
						&& showExpenseCategories
						? GetExpenseCategories(unitOfWork, parentId, titlePart, idPart, subdivisionId, parentFinancialGroupId).ToList() : Enumerable.Empty<FinancialCategoriesJournalNode>())
				   .AsQueryable();
		}

		private IQueryable<FinancialCategoriesJournalNode> GetIncomeCategories(IUnitOfWork unitOfWork, int? parentId, string titlePart, string idPart, int subdivisionId, int parentFinancialGroupId) =>
			from incomeCategory in unitOfWork.GetAll<IncomeCategory>()
			where incomeCategory.FinancialCategoryGroupId == parentId
				&& (_filter.ShowArchive || !incomeCategory.IsArchive)
				&& (string.IsNullOrWhiteSpace(_filter.TitlePart) || incomeCategory.Name.ToLower().Like(titlePart))
				&& (string.IsNullOrWhiteSpace(_filter.IdPart) || incomeCategory.Id.ToString().Like(idPart))
				&& _filter.ExpenseDocumentType == null
				&& (_filter.IncomeDocumentType == null || _filter.IncomeDocumentType == incomeCategory.IncomeDocumentType)
				&& (_filter.Subdivision == null || incomeCategory.Subdivision.Id == subdivisionId)
				&& (_filter.ParentFinancialGroup == null || incomeCategory.FinancialCategoryGroupId == parentFinancialGroupId)
			select new FinancialCategoriesJournalNode
			{
				Id = incomeCategory.Id,
				Name = incomeCategory.Name,
				JournalNodeType = _financialIncomeCategoryType,
				ParentId = parentId,
			};

		private IQueryable<FinancialCategoriesJournalNode> GetExpenseCategories(IUnitOfWork unitOfWork, int? parentId, string titlePart, string idPart, int subdivisionId, int parentFinancialGroupId) =>
			from expenseCategory in unitOfWork.GetAll<ExpenseCategory>()
			where expenseCategory.FinancialCategoryGroupId == parentId
				&& (_filter.ShowArchive || !expenseCategory.IsArchive)
				&& (string.IsNullOrWhiteSpace(_filter.TitlePart) || expenseCategory.Name.ToLower().Like(titlePart))
				&& (string.IsNullOrWhiteSpace(_filter.IdPart) || expenseCategory.Id.ToString().Like(idPart))
				&& _filter.IncomeDocumentType == null
				&& (_filter.ExpenseDocumentType == null || _filter.ExpenseDocumentType == expenseCategory.ExpenseDocumentType)
				&& (_filter.Subdivision == null || expenseCategory.Subdivision.Id == subdivisionId)
				&& (_filter.ParentFinancialGroup == null || expenseCategory.FinancialCategoryGroupId == parentFinancialGroupId)
			select new FinancialCategoriesJournalNode
			{
				Id = expenseCategory.Id,
				Name = expenseCategory.Name,
				JournalNodeType = _financialExpenseCategoryType,
				ParentId = parentId,
			};

		private void InitializePermissionsMatrix()
		{
			foreach(var domainObject in _domainObjectsTypes)
			{
				bool canRead = _currentPermissionService == null || _currentPermissionService.ValidateEntityPermission(domainObject).CanRead;
				bool canCreate = _currentPermissionService == null || _currentPermissionService.ValidateEntityPermission(domainObject).CanCreate;
				bool canEdit = _currentPermissionService == null || _currentPermissionService.ValidateEntityPermission(domainObject).CanUpdate;
				bool canDelete = _currentPermissionService == null || _currentPermissionService.ValidateEntityPermission(domainObject).CanDelete;
				_domainObjectsPermissions.Add(domainObject, (canRead, canCreate, canEdit, canDelete));
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
					&& selected.First() is FinancialCategoriesJournalNode node
					&& _domainObjectsPermissions[node.JournalNodeType].CanDelete
					&& selected.Any(),
				(selected) => true,
				(selected) =>
				{
					if(selected.First() is FinancialCategoriesJournalNode node
						&& ((node.JournalNodeType == typeof(FinancialCategoriesGroup)
							&& _interactiveService.Question(
								$"Вы уверены, что хотите удалить {node.JournalNodeType.GetClassUserFriendlyName().Accusative} - {node.Name}?\n" +
								"Удаление приведет к перемещению всех вложенных элементов на верхний уровень",
								"Вы уверены?"))
							|| _interactiveService.Question(
								$"Вы уверены, что хотите удалить {node.JournalNodeType.GetClassUserFriendlyName().Accusative} - {node.Name}?",
								"Вы уверены?")))
					{
						if(_domainObjectsPermissions[node.JournalNodeType].CanDelete)
						{
							DeleteHelper.DeleteEntity(node.JournalNodeType, node.Id);
						}
					}
				},
				Key.Delete.ToString());
			NodeActionsList.Add(deleteAction);
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => selected.Length == 1
					&& selected.First() is FinancialCategoriesJournalNode node
					&& _domainObjectsPermissions[node.JournalNodeType].CanEdit
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
					(selected) => _domainObjectsPermissions[objectType].CanCreate,
					(selected) => _domainObjectsPermissions[objectType].CanCreate,
					(selected) => {
						if(objectType == _financialCategoriesGroupType)
						{
							NavigationManager.OpenViewModel<FinancialCategoriesGroupViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
							return;
						}

						if(objectType == _financialIncomeCategoryType)
						{
							NavigationManager.OpenViewModel<IncomeCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
							return;
						}

						if(objectType == _financialExpenseCategoryType)
						{
							NavigationManager.OpenViewModel<ExpenseCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
							return;
						}
					}
				);
				addParentNodeAction.ChildActionsList.Add(childNodeAction);
			}

			NodeActionsList.Add(addParentNodeAction);
		}

		private void EditNodeAction(object[] selected)
		{
			if(selected.First() is FinancialCategoriesJournalNode node)
			{
				if(node.JournalNodeType == _financialCategoriesGroupType)
				{
					NavigationManager.OpenViewModel<FinancialCategoriesGroupViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
					return;
				}

				if(node.JournalNodeType == _financialIncomeCategoryType)
				{
					NavigationManager.OpenViewModel<IncomeCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
					return;
				}

				if(node.JournalNodeType == _financialExpenseCategoryType)
				{
					NavigationManager.OpenViewModel<ExpenseCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
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
