using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public partial class IncludeExcludeLastRouteListFilterFactory : IIncludeExcludeLastRouteListFilterFactory
	{
		private LastRouteListInitIncludeFilter _initIncludeFilter;
		private IInteractiveService _interactiveService;

		public IncludeExcludeLastRouteListFilterFactory(
			IInteractiveService interactiveService)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		public IncludeExludeFiltersViewModel CreateLastReportIncludeExcludeFilter(IUnitOfWork unitOfWork, LastRouteListInitIncludeFilter initIncludeFilter)
		{
			var includeExludeFiltersViewModel = CreateDefaultIncludeExludeFiltersViewModel();
			includeExludeFiltersViewModel.WithExcludes = false;
			_initIncludeFilter = initIncludeFilter;

			AddEmployeeStatusFilter(includeExludeFiltersViewModel);
			AddCarTypeOfUseFilter(includeExludeFiltersViewModel);
			AddCarOwnTypeFilter(includeExludeFiltersViewModel);
			AddVisitingMasterTypeFilter(includeExludeFiltersViewModel);

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

		private void AddVisitingMasterTypeFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<VisitingMasterFilterType>(config =>
			{
				config.RefreshFilteredElements();

				foreach(var element in config.FilteredElements)
				{
					if(element is IncludeExcludeElement<VisitingMasterFilterType, VisitingMasterFilterType> enumElement
						&& _initIncludeFilter != null
						&& _initIncludeFilter.VisitingMaster.Contains(enumElement.Id))
					{
						enumElement.Include = true;
					}
				}
			},
			true
			);
		}

		private IncludeExludeFiltersViewModel CreateDefaultIncludeExludeFiltersViewModel()
		{
			return new IncludeExludeFiltersViewModel(_interactiveService);
		}
	}
}
