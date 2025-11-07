using NHibernate.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.CommonEnums;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Extensions;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.ViewModels.ReportsParameters.Production
{
	public partial class ProducedProductionReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly IGenericRepository<Warehouse> _warehouseRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;

		public ProducedProductionReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IGenericRepository<Warehouse> warehouseRepository,
			IInteractiveService interactiveService,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Отчет по произведенной продукции";
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			_unitOfWork = _uowFactory.CreateWithoutRoot(Title);
			
			FilterViewModel = CreateReportIncludeExcludeFilter(_unitOfWork);
			
			GenerateReportCommand = new DelegateCommand(LoadReport);

			Years = Enumerable.Range(DateTime.Now.AddYears(-10).Year, 21).Reverse();
			SelectedYear = DateTime.Today.Year;

			Months = typeof(Month);
			SelectedMonth = (Month)(DateTime.Now.AddMonths(-1).Month);

			ReportModes = typeof(ProducedProductionReportMode);
			SelectedReportMode = ProducedProductionReportMode.Month;
		}

		private IncludeExludeFiltersViewModel CreateReportIncludeExcludeFilter(IUnitOfWork unitOfWork)
		{
			var includeExcludeFiltersViewModel = new IncludeExludeFiltersViewModel(_interactiveService);

			includeExcludeFiltersViewModel.AddFilter(unitOfWork, _nomenclatureRepository, config =>
			{
				config.Title = "Номенклатура";
				config.GenitivePluralTitle = "Номенклатур";

				config.RefreshFunc = (IncludeExcludeEntityFilter<Nomenclature> filter) =>
				{
					Expression<Func<Nomenclature, bool>> specificationExpression = null;

					Expression<Func<Nomenclature, bool>> searchInFullNameSpec = nomenclature =>
						(string.IsNullOrWhiteSpace(includeExcludeFiltersViewModel.CurrentSearchString)
						|| nomenclature.Name.ToLower().Like($"%{includeExcludeFiltersViewModel.CurrentSearchString.ToLower()}%"))
						&& nomenclature.Category == NomenclatureCategory.water
						&& (includeExcludeFiltersViewModel.ShowArchived || !nomenclature.IsArchive);

					specificationExpression = specificationExpression.CombineWith(searchInFullNameSpec);

					var elementsToAdd = _nomenclatureRepository.Get(
							unitOfWork,
							specificationExpression,
							limit: IncludeExludeFiltersViewModel.DefaultLimit)
						.Select(x => new IncludeExcludeElement<int, Nomenclature>
						{
							Id = x.Id,
							Title = x.Name
						});

					filter.FilteredElements.Clear();

					foreach(var element in elementsToAdd)
					{
						filter.FilteredElements.Add(element);
					}
				};
			});

			includeExcludeFiltersViewModel.AddFilter(unitOfWork, _warehouseRepository, config =>
			{
				config.Title = "Производство";
				config.GenitivePluralTitle = "Производств";

				config.RefreshFunc = (IncludeExcludeEntityFilter<Warehouse> filter) =>
				{
					Expression<Func<Warehouse, bool>> specificationExpression = null;

					Expression<Func<Warehouse, bool>> searchInFullNameSpec = warehouse =>
						(string.IsNullOrWhiteSpace(includeExcludeFiltersViewModel.CurrentSearchString)
						|| warehouse.Name.ToLower().Like($"%{includeExcludeFiltersViewModel.CurrentSearchString.ToLower()}%"))
						&& warehouse.TypeOfUse == WarehouseUsing.Production
						&& (includeExcludeFiltersViewModel.ShowArchived || !warehouse.IsArchive);

					specificationExpression = specificationExpression.CombineWith(searchInFullNameSpec);

					var elementsToAdd = _warehouseRepository.Get(
							unitOfWork,
							specificationExpression,
							limit: IncludeExludeFiltersViewModel.DefaultLimit)
						.Select(x => new IncludeExcludeElement<int, Warehouse>
						{
							Id = x.Id,
							Title = x.Name
						});

					filter.FilteredElements.Clear();

					foreach(var element in elementsToAdd)
					{
						filter.FilteredElements.Add(element);
					}
				};
			});

			return includeExcludeFiltersViewModel;
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var includedNomenclatureIds = FilterViewModel
					.GetIncludedElements<Nomenclature>()
					.Select(x => (x as IncludeExcludeElement<int, Nomenclature>).Id)
					.ToArray();

				var excludeNomenclatureIds = FilterViewModel
					.GetExcludedElements<Nomenclature>()
					.Select(x => (x as IncludeExcludeElement<int, Nomenclature>).Id)
					.ToArray();

				var includedWarehouseIds = FilterViewModel
					.GetIncludedElements<Warehouse>()
					.Select(x => (x as IncludeExcludeElement<int, Warehouse>).Id)
					.ToArray();

				var excludeWarehouseIds = FilterViewModel
					.GetExcludedElements<Warehouse>()
					.Select(x => (x as IncludeExcludeElement<int, Warehouse>).Id)
					.ToArray();

				CultureInfo ci = new CultureInfo("ru-RU");
				DateTimeFormatInfo mfi = ci.DateTimeFormat;

				string strMonthName = mfi.GetMonthName((int)SelectedMonth).ToString();

				var reportDate = new DateTime(SelectedYear, (int)SelectedMonth, 1);

				var monthNumMinus1 = reportDate.AddMonths(-1).Month;
				string strMonthNameMinus1 = mfi.GetMonthName(monthNumMinus1).ToString();

				var parameters = FilterViewModel.GetReportParametersSet(out var sb);

				parameters.Add("month_start", MonthStart(reportDate));
				parameters.Add("month_end", MonthEnd(reportDate));
				parameters.Add("month_minus_1_start", MonthStart(reportDate.AddMonths(-1)));
				parameters.Add("month_minus_1_end", MonthEnd(reportDate.AddMonths(-1)));
				parameters.Add("month_name", strMonthName);
				parameters.Add("month_name_minus_1", strMonthNameMinus1);
				parameters.Add("creation_date", DateTime.Now);

				if(SelectedReportMode == ProducedProductionReportMode.Year)
				{
					Identifier = "Production.ProducedProductionYear";

					var strYearNameMinus1 = $"{strMonthName} {SelectedYear - 1}";
					parameters.Add("year_name_minus_1", strYearNameMinus1);
					parameters.Add("year_minus_1_start", MonthStart(reportDate.AddYears(-1)));
					parameters.Add("year_minus_1_end", MonthEnd(reportDate.AddYears(-1)));
				}
				else
				{
					Identifier = "Production.ProducedProduction";

					var monthNumMinus2 = reportDate.AddMonths(-2).Month;
					string strMonthNameMinus2 = mfi.GetMonthName(monthNumMinus2).ToString();
					parameters.Add("month_name_minus_2", strMonthNameMinus2);
					parameters.Add("month_minus_2_start", MonthStart(reportDate.AddMonths(-2)));
					parameters.Add("month_minus_2_end", MonthEnd(reportDate.AddMonths(-2)));
				}

				return parameters;
			}
		}

		public IncludeExludeFiltersViewModel FilterViewModel { get; }

		private DateTime MonthStart(DateTime date)
		{
			return new DateTime(date.Year, date.Month, 1);
		}

		private DateTime MonthEnd(DateTime date)
		{
			return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
		}

		public DelegateCommand GenerateReportCommand { get; }
		public IEnumerable<int> Years { get; }
		public int SelectedYear { get; set; }
		public Type Months { get;}
		public Month SelectedMonth { get; set; }
		public Type ReportModes { get; set; }
		public ProducedProductionReportMode SelectedReportMode { get; set; }

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
