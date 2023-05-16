using FluentNHibernate.Conventions;
using Gamma.Binding.Core.RecursiveTreeConfig;
using MoreLinq;
using NHibernate.Util;
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

			_hierarchicalChunkLinqLoader = new HierarchicalChunkLinqLoader<FinancialCategoriesGroup, FinancialCategoriesJournalNode>(UnitOfWorkFactory);

			RecuresiveConfig = _hierarchicalChunkLinqLoader.TreeConfig;

			_hierarchicalChunkLinqLoader.SetRecursiveModel(GetChunk);

			var threadDataLoader = new ThreadDataLoader<FinancialCategoriesJournalNode>(unitOfWorkFactory);
			threadDataLoader.DynamicLoadingEnabled = true;
			threadDataLoader.QueryLoaders.Add(_hierarchicalChunkLinqLoader);

			DataLoader = threadDataLoader;

			_domainObjectsPermissions = new Dictionary<Type, (bool CanRead, bool CanCreate, bool CanEdit, bool CanDelete)>();

			InitializePermissionsMatrix();
			CreateNodeActions();
			UpdateOnChanges(_domainObjectsTypes);
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		public override string FooterInfo
		{
			get => DataLoader.TotalCount.HasValue ? $" | Загружено: {DataLoader.TotalCount.Value}" : "";

			set => base.FooterInfo = value;
		}

		private IQueryable<FinancialCategoriesJournalNode> GetChunk(IUnitOfWork unitOfWork, int? parentId) =>
			GetSubGroup(unitOfWork, parentId);

		private IQueryable<FinancialCategoriesJournalNode> GetSubGroup(IUnitOfWork unitOfWork, int? parentId)
		{
			return (from financialCategoriesGroup in unitOfWork.GetAll<FinancialCategoriesGroup>()
					where financialCategoriesGroup.ParentId == parentId
						&& !_filter.ExcludeFinancialGroupsIds.Contains(financialCategoriesGroup.Id)
						&& (_filter.RestrictNodeTypes.IsEmpty() || _filter.RestrictNodeTypes.Contains(_financialCategoriesGroupType))
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
						_filter.RestrictNodeTypes.IsEmpty()
							|| _filter.RestrictNodeTypes.Contains(_financialIncomeCategoryType)
						? GetIncomeCategories(unitOfWork, parentId).ToList() : Enumerable.Empty<FinancialCategoriesJournalNode>())
				   .Concat(
						_filter.RestrictNodeTypes.IsEmpty()
							|| _filter.RestrictNodeTypes.Contains(_financialExpenseCategoryType)
						? GetExpenseCategories(unitOfWork, parentId).ToList() : Enumerable.Empty<FinancialCategoriesJournalNode>())
				   .AsQueryable();
		}

		private IQueryable<FinancialCategoriesJournalNode> GetIncomeCategories(IUnitOfWork unitOfWork, int? parentId) =>
			from incomeCategory in unitOfWork.GetAll<IncomeCategory>()
			where incomeCategory.FinancialCategoryGroupId == parentId
			select new FinancialCategoriesJournalNode
			{
				Id = incomeCategory.Id,
				Name = incomeCategory.Name,
				JournalNodeType = _financialIncomeCategoryType,
				ParentId = parentId,
			};

		private IQueryable<FinancialCategoriesJournalNode> GetExpenseCategories(IUnitOfWork unitOfWork, int? parentId) =>
			from expenseCategory in unitOfWork.GetAll<ExpenseCategory>()
			where expenseCategory.FinancialCategoryGroupId == parentId
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

			var addAction = new JournalAction("Добавить",
					(selected) => selected.Length == 1 && selected.First() is FinancialCategoriesJournalNode node && _domainObjectsPermissions[node.JournalNodeType].CanCreate,
					(selected) => true,
					(selected) =>
					{

						if(selected.First() is FinancialCategoriesJournalNode node)
						{
							if(node.JournalNodeType == _financialCategoriesGroupType)
							{
								NavigationManager.OpenViewModel<FinancialCategoriesGroupViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
								return;
							}

							if(node.JournalNodeType == _financialIncomeCategoryType)
							{
								NavigationManager.OpenViewModel<IncomeCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
								return;
							}

							if(node.JournalNodeType == _financialExpenseCategoryType)
							{
								NavigationManager.OpenViewModel<ExpenseCategoryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
								return;
							}
						}
					});

			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
					(selected) => selected.Length == 1 && selected.First() is FinancialCategoriesJournalNode node && _domainObjectsPermissions[node.JournalNodeType].CanEdit && selected.Any(),
					(selected) => true,
					EditNodeAction);

			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			var deleteAction = new JournalAction("Удалить",
					(selected) => selected.Length == 1 && selected.First() is FinancialCategoriesJournalNode node && _domainObjectsPermissions[node.JournalNodeType].CanDelete && selected.Any(),
					(selected) => true,
					(selected) =>
					{
						if(selected.First() is FinancialCategoriesJournalNode node && _interactiveService.Question(
							$"Вы уверены, что хотите удалить {node.JournalNodeType} - {node.Name}?\n" +
							"Удаление приведет к перемещению всех вложенных элементов на верхний уровень"
							, "Вы уверены?"))
						{
							using(var localUnitOfWork = UnitOfWorkFactory.CreateWithoutRoot(TabName + " -> Действие удаления"))
							{
								localUnitOfWork.TryDelete(UoW.GetById(node.JournalNodeType, node.Id));
								localUnitOfWork.Commit();
							}
						}
					},
					Key.Delete.ToString());
			NodeActionsList.Add(deleteAction);
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
