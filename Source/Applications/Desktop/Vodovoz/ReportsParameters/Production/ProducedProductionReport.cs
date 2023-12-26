using Autofac;
using NHibernate.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.CommonEnums;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.Extensions;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.TempAdapters;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.ReportsParameters.Production
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProducedProductionReport : SingleUoWWidgetBase, IParametersWidget
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private readonly INomenclatureJournalFactory _nomenclatureJournalFactory;
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly IGenericRepository<Warehouse> _warehouseRepository;
		private readonly IncludeExludeFiltersViewModel _filterViewModel;

		public ProducedProductionReport(INomenclatureJournalFactory nomenclatureJournalFactory)
		{		
			_nomenclatureRepository = _lifetimeScope.Resolve<IGenericRepository<Nomenclature>>();
			_warehouseRepository = _lifetimeScope.Resolve<IGenericRepository<Warehouse>>();

			_nomenclatureJournalFactory = nomenclatureJournalFactory ?? throw new ArgumentNullException(nameof(nomenclatureJournalFactory));
			Build();
			
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			yenumcomboboxMonths.ItemsEnum = typeof(Month);
			yenumcomboboxMonths.SelectedItem = (Month)(DateTime.Now.AddMonths(-1).Month);
			
            ylistcomboboxYear.ItemsList = Enumerable.Range(DateTime.Now.AddYears(-10).Year, 21).Reverse();
            ylistcomboboxYear.SelectedItem = DateTime.Today.Year;

			yenumcomboboxReportType.ItemsEnum = typeof(ProducedProductionReportMode);
			yenumcomboboxReportType.SelectedItem = ProducedProductionReportMode.Month;

			_filterViewModel = CreateReportIncludeExcludeFilter(UoW);
			var filterView = new IncludeExludeFiltersView(_filterViewModel);
			vboxParameters.Add(filterView);
			filterView.Show();

			buttonCreateReport.Sensitive = true;
			buttonCreateReport.Clicked += OnButtonCreateReportClicked;
		}

		private IncludeExludeFiltersViewModel CreateReportIncludeExcludeFilter(IUnitOfWork unitOfWork)
		{
			var includeExcludeFiltersViewModel = new IncludeExludeFiltersViewModel(ServicesConfig.InteractiveService);

			includeExcludeFiltersViewModel.AddFilter(unitOfWork, _nomenclatureRepository, config =>
			{
				config.Title = "Номенклатура";

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

		void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			OnUpdate (true);
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по произведенной продукции";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{			
			CultureInfo ci = new CultureInfo("ru-RU");
			DateTimeFormatInfo mfi = ci.DateTimeFormat;

            var monthNum = (int)yenumcomboboxMonths.SelectedItem;
			string strMonthName = mfi.GetMonthName(monthNum).ToString();

            var year = (int)ylistcomboboxYear.SelectedItem;
            var reportDate = new DateTime(year, monthNum, 1);

            var monthNumMinus1 = reportDate.AddMonths(-1).Month;
            string strMonthNameMinus1 = mfi.GetMonthName(monthNumMinus1).ToString();
			
			var monthNumMinus2 = reportDate.AddMonths(-2).Month;
			string strMonthNameMinus2 = mfi.GetMonthName(monthNumMinus2).ToString();

            var strYearNameMinus1 = $"{strMonthName} {year - 1}";

            var includedNomenclatureIds = _filterViewModel
				.GetIncludedElements<Nomenclature>()
				.Select(x => (x as IncludeExcludeElement<int, Nomenclature>).Id)
				.ToArray();

			var excludeNomenclatureIds = _filterViewModel
				.GetExcludedElements<Nomenclature>()
				.Select(x => (x as IncludeExcludeElement<int, Nomenclature>).Id)
				.ToArray();

			var includedWarehouseIds = _filterViewModel
				.GetIncludedElements<Warehouse>()
				.Select(x => (x as IncludeExcludeElement<int, Warehouse>).Id)
				.ToArray();

			var excludeWarehouseIds = _filterViewModel
				.GetExcludedElements<Warehouse>()
				.Select(x => (x as IncludeExcludeElement<int, Warehouse>).Id)
				.ToArray();


            Dictionary<string, object> parameters = _filterViewModel.GetReportParametersSet();

            parameters.Add( "month_start", MonthStart(reportDate) );

            parameters.Add("month_end", MonthEnd(reportDate));
            parameters.Add("month_minus_1_start", MonthStart(reportDate.AddMonths(-1)));
            parameters.Add("month_minus_1_end", MonthEnd(reportDate.AddMonths(-1)));
            parameters.Add("month_name", strMonthName);
            parameters.Add("month_name_minus_1", strMonthNameMinus1);            
            parameters.Add("creation_date", DateTime.Now);

            var identifier = string.Empty;

			var reportMode = (ProducedProductionReportMode)yenumcomboboxReportType.SelectedItem;

			if(reportMode == ProducedProductionReportMode.Year)
            {
                identifier = "Production.ProducedProductionYear";
                parameters.Add("year_name_minus_1", strYearNameMinus1);
                parameters.Add("year_minus_1_start", MonthStart(reportDate.AddYears(-1)));
                parameters.Add("year_minus_1_end", MonthEnd(reportDate.AddYears(-1)));
            }
			else
			{
                identifier = "Production.ProducedProduction";
                parameters.Add("month_name_minus_2", strMonthNameMinus2);
                parameters.Add("month_minus_2_start", MonthStart(reportDate.AddMonths(-2)));
                parameters.Add("month_minus_2_end", MonthEnd(reportDate.AddMonths(-2)));
            }

            return new ReportInfo
            {
                Identifier = identifier,
                Parameters = parameters
            };
		}

		void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

		private DateTime MonthStart(DateTime date)
		{
			return new DateTime(date.Year,date.Month, 1);
		}
		private DateTime MonthEnd(DateTime date)
		{
			return new DateTime(date.Year,date.Month, DateTime.DaysInMonth(date.Year,date.Month));
		}

		public override void Destroy()
		{
			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}
			base.Destroy();
		}
	}
}
