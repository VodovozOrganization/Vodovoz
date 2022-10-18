using System;
using System.Linq;
using MoreLinq;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Controllers
{
	public class CarFuelVersionsController : ICarFuelVersionsController
	{
		private CarModel _carModel;
		public CarFuelVersionsController(CarModel carModel)
		{
			_carModel = carModel ?? throw new ArgumentNullException(nameof(carModel));
		}

		public void ChangeVersionStartDate(CarFuelVersion version, DateTime newStartDate)
		{
			if(version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}
			if(version.CarModel == null || version.CarModel.Id != _carModel.Id)
			{
				throw new ArgumentException("Неверно заполнен авто в переданной версии");
			}

			var previousVersion = GetPreviousVersionOrNull(version);
			if(previousVersion != null)
			{
				var newEndDate = newStartDate.AddMilliseconds(-1);
				previousVersion.EndDate = newEndDate;
			}
			version.StartDate = newStartDate;
		}

		public void CreateAndAddVersion(double fuelConsumption, DateTime? startDate)
		{
			if(startDate == null)
			{
				startDate = DateTime.Now;
			}

			var newVersion = new CarFuelVersion
			{
				CarModel = _carModel,
				FuelConsumption = fuelConsumption
			};

			AddNewVersion(newVersion, startDate.Value);
		}
		
		public bool IsValidDateForVersionStartDateChange(CarFuelVersion version, DateTime newStartDate)
		{
			if(version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}
			if(version.StartDate == newStartDate)
			{
				return false;
			}
			if(newStartDate >= version.EndDate)
			{
				return false;
			}
			var previousVersion = GetPreviousVersionOrNull(version);
			return previousVersion == null || newStartDate > previousVersion.StartDate;
		}

		public bool IsValidDateForNewCarVersion(DateTime dateTime)
		{
			return _carModel.CarFuelVersions.All(x => x.StartDate < dateTime);
		}

		private void AddNewVersion(CarFuelVersion newCarFuelVersion, DateTime startDate)
		{
			if(newCarFuelVersion == null)
			{
				throw new ArgumentNullException(nameof(newCarFuelVersion));
			}
			if(newCarFuelVersion.CarModel == null || newCarFuelVersion.CarModel.Id != _carModel.Id)
			{
				newCarFuelVersion.CarModel = _carModel;
			}
			newCarFuelVersion.StartDate = startDate;

			if(_carModel.CarFuelVersions.Any())
			{
				var currentLatestVersion = _carModel.CarFuelVersions.MaxBy(x => x.StartDate);
				if(startDate < currentLatestVersion.StartDate.AddDays(1))
				{
					throw new ArgumentException(
						"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
						nameof(startDate));
				}
				currentLatestVersion.EndDate = startDate.AddMilliseconds(-1);
			}

			_carModel.ObservableCarFuelVersions.Insert(0, newCarFuelVersion);
		}

		private CarFuelVersion GetPreviousVersionOrNull(CarFuelVersion currentVersion)
		{
			return _carModel.CarFuelVersions
				.Where(x => x.StartDate < currentVersion.StartDate)
				.OrderByDescending(x => x.StartDate)
				.FirstOrDefault();
		}
	}
}
