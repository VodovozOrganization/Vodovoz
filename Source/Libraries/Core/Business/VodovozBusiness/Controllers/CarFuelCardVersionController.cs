using MoreLinq;
using System;
using System.Linq;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Controllers
{
	public class CarFuelCardVersionController : ICarFuelCardVersionController
	{
		private readonly Car _car;

		public CarFuelCardVersionController(Car car)
		{
			_car = car ?? throw new System.ArgumentNullException(nameof(car));
		}

		public void ChangeVersionStartDate(FuelCardVersion version, DateTime newStartDate)
		{
			if(version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}

			if(version.Car == null || version.Car.Id != _car.Id)
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

		public void CreateAndAddVersion(FuelCard fuelCard, DateTime? startDate)
		{
			if(startDate == null)
			{
				startDate = DateTime.Now;
			}

			var newVersion = new FuelCardVersion
			{
				Car = _car,
				FuelCard = fuelCard
			};

			AddNewVersion(newVersion, startDate.Value);
		}

		public bool IsValidDateForVersionStartDateChange(FuelCardVersion version, DateTime newStartDate)
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
			return _car.FuelCardVersions.All(x => x.StartDate < dateTime);
		}

		private void AddNewVersion(FuelCardVersion newVersion, DateTime startDate)
		{
			if(newVersion == null)
			{
				throw new ArgumentNullException(nameof(newVersion));
			}
			if(newVersion.Car == null || newVersion.Car.Id != _car.Id)
			{
				newVersion.Car = _car;
			}
			newVersion.StartDate = startDate;

			if(_car.FuelCardVersions.Any())
			{
				var currentLatestVersion = _car.FuelCardVersions.MaxBy(x => x.StartDate).First();
				if(startDate < currentLatestVersion.StartDate.AddDays(1))
				{
					throw new ArgumentException(
						"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
						nameof(startDate));
				}
				currentLatestVersion.EndDate = startDate.AddMilliseconds(-1);
			}

			_car.ObservableFuelCardVersions.Insert(0, newVersion);
		}

		private FuelCardVersion GetPreviousVersionOrNull(FuelCardVersion currentVersion)
		{
			return _car.FuelCardVersions
				.Where(x => x.StartDate < currentVersion.StartDate)
				.OrderByDescending(x => x.StartDate)
				.FirstOrDefault();
		}
	}
}
