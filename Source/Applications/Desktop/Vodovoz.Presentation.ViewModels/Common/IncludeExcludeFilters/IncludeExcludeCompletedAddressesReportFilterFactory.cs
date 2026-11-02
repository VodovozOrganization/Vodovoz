using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public class IncludeExcludeCompletedAddressesReportFilterFactory : IIncludeExcludeCompletedAddressesReportFilterFactory
	{
		public const string DriversFilterName = "CompletedAddressesReportDrivers";
		public const string DistrictsFilterName = "CompletedAddressesReportDistricts";

		private readonly IInteractiveService _interactiveService;

		public IncludeExcludeCompletedAddressesReportFilterFactory(IInteractiveService interactiveService)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		/// <inheritdoc/>
		public IncludeExludeFiltersViewModel CreateCompletedAddressesReportIncludeExcludeFilter(
			Func<IEnumerable<(int Id, string Title)>> getDrivers,
			Func<IEnumerable<(int? Id, string Title)>> getDistricts)
		{
			if(getDrivers == null)
			{
				throw new ArgumentNullException(nameof(getDrivers));
			}

			if(getDistricts == null)
			{
				throw new ArgumentNullException(nameof(getDistricts));
			}

			var filtersViewModel = new IncludeExludeFiltersViewModel(_interactiveService);

			filtersViewModel.AddFilter("Водители", new Dictionary<string, string>(), config =>
			{
				config.DefaultName = DriversFilterName;
				config.GenitivePluralTitle = "Водителей";
				config.RefreshFunc = filter => FillElements<int, Employee>(filter, filtersViewModel, getDrivers(), splitSearchByWords: true);
			});

			filtersViewModel.AddFilter("Районы", new Dictionary<string, string>(), config =>
			{
				config.DefaultName = DistrictsFilterName;
				config.GenitivePluralTitle = "Районов";
				config.RefreshFunc = filter => FillElements<int?, District>(filter, filtersViewModel, getDistricts(), splitSearchByWords: false);
			});

			return filtersViewModel;
		}

		private static void FillElements<TId, TEntity>(
			IncludeExcludeBoolParamsFilter filter,
			IncludeExludeFiltersViewModel filtersViewModel,
			IEnumerable<(TId Id, string Title)> items,
			bool splitSearchByWords)
		{
			var search = filtersViewModel.CurrentSearchString ?? string.Empty;

			var words = splitSearchByWords
				? search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				: string.IsNullOrWhiteSpace(search) ? new string[0] : new[] { search.Trim() };

			filter.FilteredElements.Clear();

			foreach(var item in items)
			{
				if(words.All(word => item.Title.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0))
				{
					filter.FilteredElements.Add(new IncludeExcludeElement<TId, TEntity> { Id = item.Id, Title = item.Title });
				}
			}
		}
	}
}
