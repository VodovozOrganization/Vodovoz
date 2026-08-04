using Autofac;
using ClosedXML.Excel;
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
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Отчет");

				worksheet.Cell(1, 5).Value = $"Отчет по расходу топлива за период {Report.StartDate:dd-MM-yyyy} – {Report.EndDate:dd-MM-yyyy}";
				worksheet.Range(1, 5, 1, 15).Merge();
				worksheet.Cell(1, 5).Style.Font.Bold = true;
				worksheet.Cell(1, 5).Style.Font.FontSize = 14;

				worksheet.Cell(2, 2).Value = "Выбранные авто:";
				worksheet.Cell(2, 3).Value = Report.SelectedCars;
				worksheet.Cell(2, 2).Style.Font.Bold = true;

				worksheet.Cell(3, 2).Value = "Расхождения % ≥ :";
				worksheet.Cell(3, 3).Value = Report.SelectedDiscrepancyPercent;
				worksheet.Cell(3, 2).Style.Font.Bold = true;

				var headers = new[]
				{
					"Авто",
					"Тип автомобиля",
					"ФИО последнего водителя",
					"Прикрепление к площадке",
					"Дата калибровки",
					"Дата след.калибровки",
					"Начальный баланс",
					"Актуальный баланс",
					"Сумма км по МЛ на выполнение адресов",
					"Факт по одометру",
					"Полезный пробег, %",
					"Факт расход",
					"План расход",
					"Разница",
					"Факт расход на 100км",
					"План расход на 100км",
					"Разница в %"
				};

				int headerRow = 5;
				for(int i = 0; i < headers.Length; i++)
				{
					var cell = worksheet.Cell(headerRow, i + 1);
					cell.Value = headers[i];
					cell.Style.Font.Bold = true;
					cell.Style.Fill.BackgroundColor = XLColor.LightGray;
					cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
				}

				int dataRow = 6;
				foreach(var item in Report.Rows)
				{
					worksheet.Cell(dataRow, 1).Value = item.Car ?? "";
					worksheet.Cell(dataRow, 2).Value = item.CarTypeOfUseString ?? "";
					worksheet.Cell(dataRow, 3).Value = item.DriverFullName ?? "";
					worksheet.Cell(dataRow, 4).Value = item.GeographicGroups ?? "";
					worksheet.Cell(dataRow, 5).Value = item.CalibrationDate?.ToString("dd.MM.yyyy") ?? "";
					worksheet.Cell(dataRow, 6).Value = item.NextCalibrationDate?.ToString("dd.MM.yyyy") ?? "";
					worksheet.Cell(dataRow, 7).Value = item.CurrentBalance ?? 0;
					worksheet.Cell(dataRow, 8).Value = item.ActualBalance ?? 0;
					worksheet.Cell(dataRow, 9).Value = item.ConfirmedDistance ?? 0;
					worksheet.Cell(dataRow, 10).Value = item.OdometerFact ?? 0;
					worksheet.Cell(dataRow, 11).Value = item.UsefulMileagePercentValue ?? 0;
					worksheet.Cell(dataRow, 12).Value = item.ConsumptionFact ?? 0;
					worksheet.Cell(dataRow, 13).Value = item.ConsumptionPlan ?? 0;
					worksheet.Cell(dataRow, 14).Value = item.DiscrepancyFuel;
					worksheet.Cell(dataRow, 15).Value = item.Consumption100KmFact;
					worksheet.Cell(dataRow, 16).Value = item.Consumption100KmPlan ?? 0;
					worksheet.Cell(dataRow, 17).Value = item.DiscrepancyPercent;

					dataRow++;
				}

				worksheet.Columns().AdjustToContents();

				var dataRange = worksheet.Range(headerRow, 1, dataRow - 1, headers.Length);
				dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
				dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

				var numberColumns = new[] { 7, 8, 9, 10, 12, 13, 14, 15, 16 };
				foreach(var col in numberColumns)
				{
					var range = worksheet.Range(headerRow + 1, col, dataRow - 1, col);
					range.Style.NumberFormat.Format = "#,##0.00";
				}

				var percentColumns = new[] { 11, 17 };
				foreach(var col in percentColumns)
				{
					var range = worksheet.Range(headerRow + 1, col, dataRow - 1, col);
					range.Style.NumberFormat.Format = "#,##0.00\"%\"";
				}

				workbook.SaveAs(path);
			}
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

			var odometerReadings = UoW.Session.Query<OdometerReading>()
				.Where(o => o.StartDate >= StartDate && o.StartDate <= EndDate)
				.OrderBy(o => o.Car.Id)
				.ThenBy(o => o.StartDate)
				.ToList();

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
					CarId = carEvent.Car.Id,
					CalibrationDate = carEvent.CreateDate,
					ActualBalance = carEvent.ActualFuelBalance ?? 0,
					CurrentBalance = carEvent.CurrentFuelBalance ?? 0,
					Car = carEvent.Car.RegistrationNumber,
					ConfirmedDistance = confirmedDistance + mileageWriteOffKmSum,
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
						CarId = singleRow.CarId,
						Car = singleRow.Car,
						CalibrationDate = singleRow.CalibrationDate,
						IsSingleCalibrationForPeriod = true
					};

					row.Value[0] = singleRow;

					continue;
				}

				for(int i = 0; i < row.Value.Count - 1; i++)
				{
					row.Value[i].ConsumptionFact =
						(row.Value[i].ConsumptionPlan ?? 0)
						- (row.Value[i].NextCalibrationFuelOperation ?? 0);

					row.Value[i].ActualBalance = row.Value[i + 1].ActualBalance;
				}

				row.Value.Remove(row.Value.Last());
			}

			var resultRows = eventsDict.SelectMany(ed => ed.Value).ToList();
			FillCarInfo(resultRows);

			FillOdometerReadings(resultRows, odometerReadings);

			return resultRows;
		}

		private void FillOdometerReadings(IEnumerable<AverageFlowDiscrepanciesReportRow> rows, List<OdometerReading> odometerReadings)
		{
			var carIds = rows.Select(x => x.CarId).Distinct().ToArray();

			if(!carIds.Any())
			{
				return;
			}

			var allReadings = UoW.Session.Query<OdometerReading>()
				.Where(o => carIds.Contains(o.Car.Id))
				.OrderBy(o => o.StartDate)
				.ToList();

			var readingsByCar = allReadings
				.GroupBy(o => o.Car.Id)
				.ToDictionary(g => g.Key, g => g.OrderBy(o => o.StartDate).ToList());

			foreach(var row in rows)
			{
				if(!readingsByCar.TryGetValue(row.CarId, out var readings))
				{
					continue;
				}

				DateTime periodEnd = row.NextCalibrationDate ?? EndDate;

				var lastReading = readings
					.LastOrDefault(o => o.StartDate <= periodEnd);

				if(lastReading != null)
				{
					var previousReading = readings
						.LastOrDefault(o => o.StartDate < lastReading.StartDate);

					if(previousReading != null)
					{
						row.OdometerFact = lastReading.Odometer - previousReading.Odometer;
					}
					else
					{
						row.OdometerFact = lastReading.Odometer;
					}
				}
			}
		}

		private void FillCarInfo(IEnumerable<AverageFlowDiscrepanciesReportRow> rows)
		{
			var carIds = rows.Select(x => x.CarId).Distinct().ToArray();

			if(!carIds.Any())
			{
				return;
			}

			var cars = UoW.Session.Query<Car>()
				.Where(c => carIds.Contains(c.Id))
				.ToDictionary(c => c.Id);

			foreach(var row in rows)
			{
				if(!cars.TryGetValue(row.CarId, out var car))
				{
					continue;
				}

				row.CarTypeOfUse = car.CarModel?.CarTypeOfUse;
				row.DriverFullName = car.Driver?.FullName ?? string.Empty;
				row.GeographicGroups = string.Join(", ", car.GeographicGroups.Select(g => g.Name));
			}
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
