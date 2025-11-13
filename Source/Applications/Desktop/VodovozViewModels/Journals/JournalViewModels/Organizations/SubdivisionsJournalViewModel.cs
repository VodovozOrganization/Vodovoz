using Autofac;
using FluentNHibernate.Conventions;
using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate.Linq;
using QS.Deletion;
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
using Vodovoz.Domain.Cash;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.Journals.JournalViewModels.Organizations
{
	public class SubdivisionsJournalViewModel
		: JournalViewModelBase
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly SubdivisionFilterViewModel _filterViewModel;
		private readonly ILifetimeScope _scope;
		private HierarchicalChunkLinqLoader<Subdivision, SubdivisionJournalNode> _hierarchicalChunkLinqLoader;
		private Dictionary<Type, IPermissionResult> _domainObjectsPermissions = new Dictionary<Type, IPermissionResult>();

		public SubdivisionsJournalViewModel(
			SubdivisionFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ILifetimeScope scope,
			ICurrentPermissionService currentPermissionService,
			IEmployeeJournalFactory employeeJournalFactory,
			Action<SubdivisionFilterViewModel> filterConfig = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_currentPermissionService = currentPermissionService
				?? throw new ArgumentNullException(nameof(currentPermissionService));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			if(filterConfig != null)
			{
				_filterViewModel.SetAndRefilterAtOnce(filterConfig);
			}

			JournalFilter = _filterViewModel;

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;

			TabName = $"Журнал {typeof(Subdivision).GetClassUserFriendlyName().GenitivePlural}";

			SearchEnabled = false;

			ExpandAfterReloading = true;

			UseSlider = true;

			_hierarchicalChunkLinqLoader = new HierarchicalChunkLinqLoader<Subdivision, SubdivisionJournalNode>(UnitOfWorkFactory);

			RecuresiveConfig = _hierarchicalChunkLinqLoader.TreeConfig;

			_hierarchicalChunkLinqLoader.SetRecursiveModel(GetChunk);

			var threadDataLoader = new ThreadDataLoader<SubdivisionJournalNode>(unitOfWorkFactory);

			threadDataLoader.QueryLoaders.Add(_hierarchicalChunkLinqLoader);
			DataLoader = threadDataLoader;
			DataLoader.DynamicLoadingEnabled = false;

			InitializePermissionsMatrix();

			CreateNodeActions();
			CreatePopupActions();

			UpdateOnChanges(typeof(Subdivision));

			(Search as SearchViewModel).PropertyChanged += OnSearchPropertyChanged;
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		public override string FooterInfo
		{
			get => DataLoader.TotalCount.HasValue ? $" | Загружено: {DataLoader.TotalCount.Value}" : "";

			set => base.FooterInfo = value;
		}

		private void InitializePermissionsMatrix()
		{
			_domainObjectsPermissions.Add(typeof(Subdivision), _currentPermissionService.ValidateEntityPermission(typeof(Subdivision)));
		}

		private void OnSearchPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Search.SearchValues))
			{
				_filterViewModel.SearchString = string.Join(" ", Search.SearchValues);
			}
		}

		private IQueryable<SubdivisionJournalNode> GetChunk(IUnitOfWork unitOfWork, int? parentId) =>
			_filterViewModel.RestrictParentId is null
			? GetSubGroup(unitOfWork, parentId)
			: GetSubGroup(unitOfWork, _filterViewModel.RestrictParentId);

		private IQueryable<SubdivisionJournalNode> GetSubGroup(IUnitOfWork unitOfWork, int? parentId)
		{
			var searchString = string.IsNullOrWhiteSpace(_filterViewModel.SearchString) ? string.Empty : $"%{_filterViewModel.SearchString.ToLower()}%";

			return ((string.IsNullOrWhiteSpace(searchString) && _filterViewModel.IncludedSubdivisionsIds.Length == 0) || parentId == null)
			? (from subdivision in unitOfWork.Session.Query<Subdivision>()
			   where (
						(
							string.IsNullOrWhiteSpace(searchString)
							&& (
									subdivision.ParentSubdivision.Id == parentId
									|| (
											parentId == null
											&& subdivision.SubdivisionType == _filterViewModel.SubdivisionType
											&& subdivision.ParentSubdivision.SubdivisionType != _filterViewModel.SubdivisionType
										)
								)
							&& _filterViewModel.IncludedSubdivisionsIds.Length == 0
						)
						|| ((string.IsNullOrWhiteSpace(searchString)
								|| (subdivision.Name.ToLower().Like(searchString)
								|| subdivision.ShortName.ToLower().Like(searchString)
								|| subdivision.Id.ToString().Like(searchString)))
							&& (_filterViewModel.IncludedSubdivisionsIds.Length == 0 || _filterViewModel.IncludedSubdivisionsIds.Contains(subdivision.Id))
							&& (!string.IsNullOrWhiteSpace(searchString) || _filterViewModel.IncludedSubdivisionsIds.Length > 0)))
				   && (_filterViewModel.ShowArchieved || !subdivision.IsArchive)
				   && !_filterViewModel.ExcludedSubdivisionsIds.Contains(subdivision.Id)
				   && (_filterViewModel.SubdivisionType == null
					|| subdivision.SubdivisionType == _filterViewModel.SubdivisionType)
					&& (!_filterViewModel.OnlyCashSubdivisions
						|| subdivision.DocumentTypes.Any(x => x.Type == nameof(Income))
						|| subdivision.DocumentTypes.Any(x => x.Type == nameof(Expense))
						|| subdivision.DocumentTypes.Any(x => x.Type == nameof(AdvanceReport)))
			   let children = GetSubGroup(unitOfWork, subdivision.Id)
			   let chiefFIO = subdivision.Chief == null ? "" : $"{subdivision.Chief.LastName} {subdivision.Chief.Name.Substring(0, 1)}. {subdivision.Chief.Patronymic.Substring(0, 1)}."
			   orderby subdivision.Name
			   select new SubdivisionJournalNode
			   {
				   Id = subdivision.Id,
				   Name = subdivision.Name,
				   ChiefName = chiefFIO,
				   ParentId = subdivision.ParentSubdivision.Id,
				   Children = children.ToList(),
				   IsArchive = subdivision.IsArchive
			   })
				: Enumerable.Empty<SubdivisionJournalNode>().AsQueryable();
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
				(selected) => selected.Any(),
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
					&& selected.FirstOrDefault() is SubdivisionJournalNode node
					&& _domainObjectsPermissions[typeof(Subdivision)].CanDelete
					&& selected.Any(),
				(selected) => true,
				(selected) =>
				{
					if(selected.FirstOrDefault() is SubdivisionJournalNode node
						&& _domainObjectsPermissions[typeof(Subdivision)].CanDelete)
					{
						DeleteHelper.DeleteEntity(typeof(Subdivision), node.Id);
					}
				},
				Key.Delete.ToString());
			NodeActionsList.Add(deleteAction);
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => selected.Length == 1
					&& selected.FirstOrDefault() is SubdivisionJournalNode node
					&& _domainObjectsPermissions[typeof(Subdivision)].CanUpdate
					&& selected.Any(),
				(selected) => true,
				(selected) =>
				{
					if(selected.FirstOrDefault() is SubdivisionJournalNode node)
					{
						var page = NavigationManager.OpenViewModel<SubdivisionViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
					}
				});

			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}

		protected void CreateAddActions()
		{
			var createAction = new JournalAction("Добавить",
				(selected) => _domainObjectsPermissions[typeof(Subdivision)].CanCreate,
				(selected) => true,
				(selected) =>
				{
					var page = NavigationManager.OpenViewModel<SubdivisionViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
				});

			NodeActionsList.Add(createAction);
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
