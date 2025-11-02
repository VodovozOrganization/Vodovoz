using MoreLinq;
using System;
using System.Linq;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services.Fuel;
using DateTimeHelpers;

namespace Vodovoz.Application.Logistics.Fuel
{
	public class FuelCardVersionService : IFuelCardVersionService
	{
		private readonly Car _car;
		private readonly bool _isLastFuelCardVersionHasEndDate;

		public FuelCardVersionService(Car car)
		{
			_car = car ?? throw new ArgumentNullException(nameof(car));

			_isLastFuelCardVersionHasEndDate = LastFuelCardVersion?.EndDate != null;
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

			if(version.FuelCard == null)
			{
				throw new ArgumentException("Не указана топливная карта в переданной версии");
			}

			var previousVersion = GetPreviousVersionOrNull(version);
			if(previousVersion != null && !_isLastFuelCardVersionHasEndDate)
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

			if(!IsDateTodayOrTomorow(newStartDate))
			{
				return false;
			}

			if(version.Id != 0
				|| version.StartDate == newStartDate
				|| newStartDate >= version.EndDate
				|| version.FuelCard == null)
			{
				return false;
			}

			var previousVersion = GetPreviousVersionOrNull(version);
			return previousVersion == null || newStartDate > previousVersion.StartDate;
		}

		public bool IsValidDateForNewCarVersion(DateTime dateTime, FuelCard fuelCard)
		{
			if(fuelCard == null)
			{
				return false;
			}

			if(!IsDateTodayOrTomorow(dateTime))
			{
				return false;
			}

			var lastVersion = LastFuelCardVersion;

			if(lastVersion == null)
			{
				return true;
			}

			if(lastVersion.FuelCard.Id == fuelCard.Id && lastVersion.EndDate == null)
			{
				return false;
			}

			if(lastVersion.EndDate != null && lastVersion.EndDate.Value > dateTime)
			{
				return false;
			}

			return _car.FuelCardVersions.All(x => x.StartDate < dateTime);
		}

		public bool IsDateTodayOrTomorow(DateTime date) =>
			date >= DateTime.Today
			&& date <= DateTime.Today.AddDays(1).LatestDayTime();

		private void AddNewVersion(FuelCardVersion newVersion, DateTime startDate)
		{
			if(newVersion == null)
			{
				throw new ArgumentNullException(nameof(newVersion));
			}

			if(newVersion.FuelCard == null)
			{
				throw new ArgumentException("Не указана топливная карта для новой версии");
			}

			if(newVersion.Car == null || newVersion.Car.Id != _car.Id)
			{
				newVersion.Car = _car;
			}

			newVersion.StartDate = startDate;

			SetCurrentLatestVersionEndDateIfNeed(startDate);

			_car.ObservableFuelCardVersions.Insert(0, newVersion);
		}

		private void SetCurrentLatestVersionEndDateIfNeed(DateTime newVersionStartDate)
		{
			var currentLatestVersion = _car.FuelCardVersions.MaxBy(x => x.StartDate).FirstOrDefault();

			if(currentLatestVersion == null)
			{
				return;
			}

			if(newVersionStartDate < currentLatestVersion.StartDate.AddDays(1))
			{
				throw new ArgumentException(
					"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
				nameof(newVersionStartDate));
			}

			if(currentLatestVersion.EndDate == null)
			{
				currentLatestVersion.EndDate = newVersionStartDate.AddMilliseconds(-1);
			}
		}

		private FuelCardVersion GetPreviousVersionOrNull(FuelCardVersion currentVersion)
		{
			return _car.FuelCardVersions
				.Where(x => x.StartDate < currentVersion.StartDate)
				.OrderByDescending(x => x.StartDate)
				.FirstOrDefault();
		}

		private FuelCardVersion LastFuelCardVersion =>
			_car.FuelCardVersions?.OrderBy(v => v.StartDate).LastOrDefault();
	}
}
