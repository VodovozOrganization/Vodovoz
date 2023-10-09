using Autofac;
using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate.Linq;
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
using Vodovoz.Domain.Cash;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.Journals.JournalViewModels.Organizations
{
	public class SubdivisionsJournalViewModel
		: JournalViewModelBase
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly ISalesPlanJournalFactory _salesPlanJournalFactory;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;
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
			ISalesPlanJournalFactory salesPlanJournalFactory,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			Action<SubdivisionFilterViewModel> filterConfig = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_currentPermissionService = currentPermissionService
				?? throw new ArgumentNullException(nameof(currentPermissionService));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_salesPlanJournalFactory = salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;

			TabName = $"Журнал {typeof(Subdivision).GetClassUserFriendlyName().GenitivePlural}";

			SearchEnabled = false;

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

			if(filterConfig != null)
			{
				_filterViewModel.SetAndRefilterAtOnce(filterConfig);
			}
		}


		public IRecursiveConfig RecuresiveConfig { get; }

		public override string FooterInfo
		{
			get => DataLoader.TotalCount.HasValue ? $" | Загружено: {DataLoader.TotalCount.Value}" : "";

			set => base.FooterInfo = value;
		}

		private void InitializePermissionsMatrix()
		{
			_domainObjectsPermissions.Add(typeof(Subdivision),  _currentPermissionService.ValidateEntityPermission(typeof(Subdivision)));
		}

		private void OnSearchPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Search.SearchValues))
			{
				_filterViewModel.SearchString = string.Join(" ", Search.SearchValues);
			}
		}

		private IQueryable<SubdivisionJournalNode> GetChunk(IUnitOfWork unitOfWork, int? parentId) =>
			GetSubGroup(unitOfWork, parentId);

		private IQueryable<SubdivisionJournalNode> GetSubGroup(IUnitOfWork unitOfWork, int? parentId)
		{
			var searchString = string.IsNullOrWhiteSpace(_filterViewModel.SearchString) ? string.Empty : $"%{_filterViewModel.SearchString.ToLower()}%";

			return (string.IsNullOrWhiteSpace(searchString) || parentId == null)
			? (from subdivision in unitOfWork.Session.Query<Subdivision>()
			   where ((string.IsNullOrWhiteSpace(searchString)
				   && subdivision.ParentSubdivision.Id == parentId)
					   || subdivision.Name.ToLower().Like(searchString)
					   || subdivision.ShortName.ToLower().Like(searchString)
					   || subdivision.Id.ToString().Like(searchString))
				   && (!_filterViewModel.ShowArchieved || !subdivision.IsArchive)
				   && !_filterViewModel.ExcludedSubdivisionsIds.Contains(subdivision.Id)
				   && (_filterViewModel.SubdivisionType == null
					|| subdivision.SubdivisionType == _filterViewModel.SubdivisionType)
					&& (!_filterViewModel.OnlyCashSubdivisions
						|| subdivision.DocumentTypes.Any(x => x.Type == nameof(Income))
						|| subdivision.DocumentTypes.Any(x => x.Type == nameof(Expense))
						|| subdivision.DocumentTypes.Any(x => x.Type == nameof(AdvanceReport)))
			   let children = GetSubGroup(unitOfWork, subdivision.Id)
			   //let chiefFIO = subdivision.Chief == null ? "" : $"{subdivision.Chief.LastName.Take(1)}. {subdivision.Chief.Name.Take(1)}. {subdivision.Chief.Patronymic}"
			   orderby subdivision.Name
			   select new SubdivisionJournalNode
			   {
					Id = subdivision.Id,
					Name = subdivision.Name,
					//ChiefName = chiefFIO,
					ParentId = subdivision.ParentSubdivision.Id,
					//Children = children.ToList()
			   })
				: Enumerable.Empty<SubdivisionJournalNode>().AsQueryable();
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
	}
}
