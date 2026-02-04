using Autofac;
using ClosedXML.Report;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Settings.Car;
using Vodovoz.Settings.Logistics;
using Vodovoz.ViewModels.Widgets.Cars.CarModelSelection;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.AverageFlowDiscrepanciesReport
{
	public class AverageFlowDiscrepanciesReportViewModel : DialogTabViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly ICarSettings _carSettings;
		private readonly ICarEventSettings _carEventSettings;
		private const string _templatePath = @".\Reports\Cars\AverageFlowDiscrepanciesReport.xlsx";
		private CarModelSelectionFilterViewModel _carModelSelectionFilterViewModel;

		public AverageFlowDiscrepanciesReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICommonServices commonServices,
			IFileDialogService fileDialogService,
			ICarSettings carSettings,
			ICarEventSettings carEventSettings)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			Title = "Отчет по расходу топлива";

			var now = DateTime.Now;
			StartDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
			EndDate = StartDate.AddMonths(1).AddDays(-1);
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_carSettings = carSettings ?? throw new ArgumentNullException(nameof(carSettings));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			ConfigureCarModelSelectionFilter();

			CreateReportCommand = new DelegateCommand(CreateReport);
			SaveReportCommand = new DelegateCommand(SaveReport);
			ShowHelpInfoCommand = new DelegateCommand(ShowHelpInfo);
		}

		private void ShowHelpInfo()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Info,
				"Авто без калибровок за выбранный период не отображаются в отчёте \n",
				"Для авто с единственной калибровкой за период невозможно рассчитать достоверные данные."
			);
		}

		private void ConfigureCarModelSelectionFilter()
		{
			_carModelSelectionFilterViewModel = new CarModelSelectionFilterViewModel(UoW, _carSettings)
			{
				SelectedCarTypesOfUse = new List<CarTypeOfUse>(Enum.GetValues(typeof(CarTypeOfUse)).Cast<CarTypeOfUse>())
			};
		}
		private void ExportReport(string path)
		{
			var template = new XLTemplate(_templatePath);
			template.AddVariable(Report);
			template.Generate();
			template.SaveAs(path);
		}

		private void CreateReport()
		{
			Report = new AverageFlowDiscrepanciesReport
			{
				SelectedDiscrepancyPercent = DiscrepancyPercentFilter,
				StartDate = StartDate,
				EndDate = EndDate,
				Rows = GenerateReportRows(),
			};
			
			Report.SelectedCars = _carModelSelectionFilterViewModel.IncludedCarModelIds.Any()
				? string.Join(", ", Report.Rows.Select(x => x.Car).Distinct())
				: "Все";

			OnPropertyChanged(() => ReportRows);
		}

		private List<AverageFlowDiscrepanciesReportRow> GenerateReportRows()
		{
			var includedCarModelIds = _carModelSelectionFilterViewModel?.IncludedCarModelIds;
			var excludedCarModelIds = _carModelSelectionFilterViewModel?.ExcludedCarModelIds;

			var events = (
				from carEvent in UoW.Session.Query<CarEvent>()

				let nextCalibrationDate = (
					from nextCarEvent in UoW.Session.Query<CarEvent>()
					where
						nextCarEvent.Car.Id == carEvent.Car.Id
						&& nextCarEvent.CreateDate >= StartDate
						&& nextCarEvent.CreateDate <= EndDate
						&& nextCarEvent.CreateDate > carEvent.CreateDate
						&& nextCarEvent.CarEventType.Id == _carEventSettings.FuelBalanceCalibrationCarEventTypeId
					orderby nextCarEvent.CreateDate ascending
					select nextCarEvent.CreateDate
				).FirstOrDefault()

				let confirmedDistance =
					(decimal?)(from routeList in UoW.Session.Query<RouteList>()
					where
						routeList.Car.Id == carEvent.Car.Id
						&& routeList.Date >= carEvent.CreateDate.Date
						&& routeList.Date < nextCalibrationDate.Date
					select routeList.ConfirmedDistance
				).Sum() ?? 0

				let recalculatedDistance =
					(decimal?)(from routeList in UoW.Session.Query<RouteList>()
						where
							routeList.Car.Id == carEvent.Car.Id
							&& routeList.Date >= carEvent.CreateDate.Date
							&& routeList.Date < nextCalibrationDate.Date
						select routeList.RecalculatedDistance
					).Sum() ?? 0

				let mileageWriteOffKmSum =
					(decimal?)(from mileageWriteOff in UoW.Session.Query<MileageWriteOff>()
					where
						mileageWriteOff.Car.Id == carEvent.Car.Id
						&& mileageWriteOff.WriteOffDate != null
						&& mileageWriteOff.WriteOffDate >= carEvent.CreateDate.Date
						&& mileageWriteOff.WriteOffDate < nextCalibrationDate.Date
					select mileageWriteOff.DistanceKm
					).Sum() ?? 0

				let carFuelConsumption = (
					from carFuelVersion in UoW.Session.Query<CarFuelVersion>()
					where
					carFuelVersion.CarModel.Id == carEvent.Car.CarModel.Id
					&& carFuelVersion.StartDate <= carEvent.CreateDate.Date
					&& (carFuelVersion.EndDate == null || carFuelVersion.EndDate >= nextCalibrationDate.Date)
					select carFuelVersion.FuelConsumption
				).FirstOrDefault()

				let nextCalibrationFuelOperation = (
					from nextCarEvent in UoW.Session.Query<CarEvent>()
					join nextFuelOperation in UoW.Session.Query<FuelOperation>()
					on nextCarEvent.CalibrationFuelOperation.Id equals nextFuelOperation.Id
					where
					nextCarEvent.Car.Id == carEvent.Car.Id
					&& nextCarEvent.CreateDate == nextCalibrationDate
					select nextFuelOperation.LitersGived - nextFuelOperation.LitersOutlayed
				).Sum()

				let lastFuelCost = (
					from fuelPriceVersion in UoW.Session.Query<FuelPriceVersion>()
					where fuelPriceVersion.FuelType.Id == carEvent.Car.FuelType.Id
					&& fuelPriceVersion.StartDate <= carEvent.CreateDate.Date
					&& (fuelPriceVersion.EndDate == null || fuelPriceVersion.EndDate >= nextCalibrationDate.Date)
					select fuelPriceVersion.FuelPrice
				).FirstOrDefault()

				where
					(!includedCarModelIds.Any() || includedCarModelIds.Contains(carEvent.Car.CarModel.Id))
					&& (!excludedCarModelIds.Any() || !excludedCarModelIds.Contains(carEvent.Car.CarModel.Id))
					&& carEvent.CreateDate >= StartDate
					&& carEvent.CreateDate <= EndDate
					&& carEvent.CarEventType.Id == _carEventSettings.FuelBalanceCalibrationCarEventTypeId

				orderby carEvent.Car ascending, carEvent.CreateDate ascending

				select new AverageFlowDiscrepanciesReportRow
				{
					CalibrationDate = carEvent.CreateDate,
					ActualBalance = carEvent.ActualFuelBalance ?? 0,
					CurrentBalance = carEvent.CurrentFuelBalance ?? 0,
					Car = carEvent.Car.RegistrationNumber,
					ConfirmedDistance = confirmedDistance + mileageWriteOffKmSum,
					RecalculatedDistance = recalculatedDistance,
					Consumption100KmPlan = carFuelConsumption,
					LastFuelCost = lastFuelCost,
					NextCalibrationDate = nextCalibrationDate,
					NextCalibrationFuelOperation = nextCalibrationFuelOperation
				}
			).ToList();

			var eventsGrouped = events.GroupBy(e => e.Car);

			var eventsDict = eventsGrouped.ToDictionary(eg => eg.Key, gdc => gdc.ToList());

			foreach(var row in eventsDict)
			{
				if(row.Value.Count == 1)
				{
					var singleRow = row.Value.Single();
					singleRow = new AverageFlowDiscrepanciesReportRow
					{
						Car = singleRow.Car,
						CalibrationDate= singleRow.CalibrationDate,
						IsSingleCalibrationForPeriod = true
					};

					row.Value[0] = singleRow;

					continue;
				}

				for(int i = 0; i < row.Value.Count - 1; i++)
				{
					row.Value[i].ConsumptionFact =
						(row.Value[i].ConsumptionPlan??0)
						- (row.Value[i].NextCalibrationFuelOperation ?? 0);

					row.Value[i].ActualBalance = row.Value[i + 1].ActualBalance;
				}

				row.Value.Remove(row.Value.Last());
			}

			return eventsDict.SelectMany(ed => ed.Value).ToList();
		}

		public AverageFlowDiscrepanciesReport Report { get; set; } = new AverageFlowDiscrepanciesReport();
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public CarModelSelectionFilterViewModel CarModelSelectionFilterViewModel => _carModelSelectionFilterViewModel;
		public int DiscrepancyPercentFilter { get; set; }
		public DelegateCommand CreateReportCommand { get; set; }
		public DelegateCommand SaveReportCommand { get; set; }
		public DelegateCommand ShowHelpInfoCommand { get; private set; }
		public List<AverageFlowDiscrepanciesReportRow> ReportRows => Report.Rows;

		public void SaveReport()
		{
			var dialogSettings = new DialogSettings()
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = $"{TabName} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx"
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(Report != null && result.Successful)
			{
				ExportReport(result.Path);
			}
		}
	}
}
