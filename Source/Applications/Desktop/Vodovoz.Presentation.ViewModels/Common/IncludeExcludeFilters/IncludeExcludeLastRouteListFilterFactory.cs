using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public partial class IncludeExcludeLastRouteListFilterFactory : IIncludeExcludeLastRouteListFilterFactory
	{
		private LastRouteListInitIncludeFilter _initIncludeFilter;
		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<Subdivision> _subdivisionRepository;

		public IncludeExcludeLastRouteListFilterFactory(
			IInteractiveService interactiveService, IGenericRepository<Subdivision> subdivisionRepository)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
		}

		public IncludeExludeFiltersViewModel CreateLastReportIncludeExcludeFilter(IUnitOfWork unitOfWork, LastRouteListInitIncludeFilter initIncludeFilter)
		{
			var includeExludeFiltersViewModel = CreateDefaultIncludeExludeFiltersViewModel();
			includeExludeFiltersViewModel.WithExcludes = false;
			_initIncludeFilter = initIncludeFilter;

			AddEmployeeStatusFilter(includeExludeFiltersViewModel);
			AddCarTypeOfUseFilter(includeExludeFiltersViewModel);
			AddCarOwnTypeFilter(includeExludeFiltersViewModel);
			AddEmployeeCategoryFilter(includeExludeFiltersViewModel);
			AddSubdivisionFilter(includeExludeFiltersViewModel, unitOfWork);

			return includeExludeFiltersViewModel;
		}

		private void AddEmployeeStatusFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<EmployeeStatus>(config =>
			{
				config.RefreshFilteredElements();

				foreach(var element in config.FilteredElements)
				{
					if(element is IncludeExcludeElement<EmployeeStatus, EmployeeStatus> enumElement
						&& _initIncludeFilter != null
						&& _initIncludeFilter.EmployeeStatusesForInclude.Contains(enumElement.Id))
					{
						enumElement.Include = true;
					}
				}
			});
		}

		private void AddCarTypeOfUseFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<CarTypeOfUse>(config =>
			{
				config.RefreshFilteredElements();

				foreach(var element in config.FilteredElements)
				{
					if(element is IncludeExcludeElement<CarTypeOfUse, CarTypeOfUse> enumElement
						&& _initIncludeFilter != null
						&& _initIncludeFilter.CarTypesOfUseForInclude.Contains(enumElement.Id))
					{
						enumElement.Include = true;
					}
				}
			});
		}

		private void AddCarOwnTypeFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<CarOwnType>(config =>
			{
				config.RefreshFilteredElements();

				foreach(var element in config.FilteredElements)
				{
					if(element is IncludeExcludeElement<CarOwnType, CarOwnType> enumElement
						&& _initIncludeFilter != null
						&& _initIncludeFilter.CarOwnTypesForInclude.Contains(enumElement.Id))
					{
						enumElement.Include = true;
					}
				}
			});
		}

		private void AddEmployeeCategoryFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<EmployeeCategoryFilterType>(config =>
			{
				config.RefreshFilteredElements();

				foreach(var element in config.FilteredElements)
				{
					if(element is IncludeExcludeElement<EmployeeCategoryFilterType, EmployeeCategoryFilterType> enumElement
						&& _initIncludeFilter != null
						&& _initIncludeFilter.EmployeeCategoryFilterTypeInclude.Contains(enumElement.Id))
					{
						enumElement.Include = true;
					}
				}
			});
		}

		private void AddSubdivisionFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel, IUnitOfWork unitOfWork)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _subdivisionRepository);
		}
		
		private IncludeExludeFiltersViewModel CreateDefaultIncludeExludeFiltersViewModel()
		{
			return new IncludeExludeFiltersViewModel(_interactiveService);
		}
	}
}
