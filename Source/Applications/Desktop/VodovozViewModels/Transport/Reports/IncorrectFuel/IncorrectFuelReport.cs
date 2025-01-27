using MoreLinq;
using NHibernate.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Fuel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Presentation.ViewModels.Reports;

namespace Vodovoz.ViewModels.Transport.Reports.IncorrectFuel
{
	[Appellative(Nominative = "Отчет по заправкам некорректным типом топлива")]
	public class IncorrectFuelReport : IClosedXmlReport
	{
		private IncorrectFuelReport(
			DateTime startDate,
			DateTime endDate,
			int? carId,
			int? fuelCardid,
			IEnumerable<CarOwnType> carOwnTypes,
			IEnumerable<CarTypeOfUse> carTypeOfUses,
			bool isExcludeOfficeWorkers)
		{
			StartDate = startDate;
			EndDate = endDate;
			CarId = carId;
			FuelCardId = fuelCardid;
			CarOwnTypes = carOwnTypes ?? new List<CarOwnType>();
			CarTypeOfUses = carTypeOfUses ?? new List<CarTypeOfUse>();
			IsExcludeOfficeWorkers = isExcludeOfficeWorkers;
		}

		public IEnumerable<IncorrectFuelReportRow> Rows { get; private set; } = new List<IncorrectFuelReportRow>();

		public string TemplatePath => @".\Reports\Transport\IncorrectFuelReport.xlsx";

		public string Title =>
			$"Отчет по заправкам некорректным типом топлива с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy}";

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public int? CarId { get; }
		public int? FuelCardId { get; }
		public IEnumerable<CarOwnType> CarOwnTypes { get; }
		public IEnumerable<CarTypeOfUse> CarTypeOfUses { get; }
		public bool IsExcludeOfficeWorkers { get; }

		private async Task CreateRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var query =
				from transaction in uow.Session.Query<FuelTransaction>()

				join fc in uow.Session.Query<FuelCard>() on transaction.CardId equals fc.CardId into fuelCards
				from fuelCard in fuelCards.DefaultIfEmpty()

				join fcv in uow.Session.Query<FuelCardVersion>() on fuelCard.Id equals fcv.FuelCard.Id into fuelCardVersions
				from fuelCardVersion in fuelCardVersions.DefaultIfEmpty()

				join c in uow.Session.Query<Car>() on fuelCardVersion.Car.Id equals c.Id into cars
				from car in cars.DefaultIfEmpty()

				join cv in uow.Session.Query<CarVersion>() on car.Id equals cv.Car.Id into carVersions
				from carVersion in carVersions.DefaultIfEmpty()

				join cm in uow.Session.Query<CarModel>() on car.CarModel.Id equals cm.Id into carModels
				from carModel in carModels.DefaultIfEmpty()

				join e in uow.Session.Query<Employee>() on car.Driver.Id equals e.Id into employees
				from driver in employees.DefaultIfEmpty()

				join ft in uow.Session.Query<FuelType>() on car.FuelType.Id equals ft.Id into fuelTypes
				from fuelType in fuelTypes.DefaultIfEmpty()

				join gp in uow.Session.Query<GazpromFuelProduct>() on transaction.ProductId equals gp.GazpromFuelProductId into gps
				from gazpromProduct in gps.DefaultIfEmpty()

				join gpg in uow.Session.Query<GazpromFuelProductsGroup>() on gazpromProduct.GazpromProductsGroupId equals gpg.Id into gpgs
				from gazpromProductGroup in gpgs.DefaultIfEmpty()
				where
					transaction.TransactionDate >= StartDate
					&& transaction.TransactionDate <= EndDate
					&& transaction.TransactionDate >= fuelCardVersion.StartDate
					&& (fuelCardVersion.EndDate == null || transaction.TransactionDate <= fuelCardVersion.EndDate)
					&& transaction.TransactionDate >= carVersion.StartDate
					&& (carVersion.EndDate == null || transaction.TransactionDate <= carVersion.EndDate)
					&& (CarId == null || fuelCardVersion.Car.Id == CarId)
					&& (FuelCardId == null || fuelCard.Id == FuelCardId)
					&& CarOwnTypes.Contains(carVersion.CarOwnType)
					&& CarTypeOfUses.Contains(carModel.CarTypeOfUse)
					&& fuelType.Id != gazpromProductGroup.FuelTypeId
					&& (!IsExcludeOfficeWorkers || driver.Category == null || driver.Category != EmployeeCategory.office)
				select new IncorrectFuelReportRow
				{
					CarRegNumber = car == null ? "" : car.RegistrationNumber,
					CarOwnType = carVersion == null ? (CarOwnType?)default : carVersion.CarOwnType,
					CarTypeOfUse = carModel == null ? (CarTypeOfUse?)default : carModel.CarTypeOfUse,
					CarModel = carModel == null ? "" : carModel.Name,
					DriverCategory = driver == null ? (EmployeeCategory?)default : driver.Category,
					DriverName = driver == null ? default : $"{driver.LastName} {driver.Name} {driver.Patronymic}",
					FuelCardNumber = transaction.CardNumber,
					CarFuelType = fuelType == null ? "" : fuelType.Name,
					TransactionId = transaction.Id,
					TransactionFuelType = gazpromProductGroup.GazpromFuelProductGroupName,
					TransactionFuelId = transaction.ProductId,
					TransactionLitersAmount = transaction.Quantity,
					TransactionDateTime = transaction.TransactionDate
				};

			Rows = await query.ToListAsync(cancellationToken);

			int rowNumber = 1;
			Rows.ForEach(row => row.RowNumber = rowNumber++);
		}

		public async static Task<IncorrectFuelReport> Create(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			int? carId,
			int? fuelCardid,
			IEnumerable<CarOwnType> carOwnTypes,
			IEnumerable<CarTypeOfUse> carTypeOfUses,
			bool isExcludeOfficeWorkers,
			CancellationToken cancellationToken)
		{
			var report = new IncorrectFuelReport(
				startDate,
				endDate,
				carId,
				fuelCardid,
				carOwnTypes,
				carTypeOfUses,
				isExcludeOfficeWorkers);

			await report.CreateRows(uow, cancellationToken);

			return report;
		}
	}
}
