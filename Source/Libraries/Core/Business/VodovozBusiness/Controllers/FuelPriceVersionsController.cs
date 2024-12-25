using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Controllers
{
	public class FuelPriceVersionsController : IFuelPriceVersionsController
	{
		private FuelType _fuelType;

		private bool _isNeedSetFuelType =>
			_fuelType is null;

		private string _fuelTypeNotSetErrorMessage =>
			"Значение типа топлива не установлено";

		public void SetFuelType(FuelType fuelType)
		{
			_fuelType = fuelType ?? throw new ArgumentNullException(nameof(fuelType));
		}

		public void ChangeVersionStartDate(FuelPriceVersion version, DateTime newStartDate)
		{
			if(_isNeedSetFuelType)
			{
				throw new InvalidOperationException(_fuelTypeNotSetErrorMessage);
			}

			if(version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}
			if(version.FuelType == null || version.FuelType.Id != _fuelType.Id)
			{
				throw new ArgumentException("Неверно заполнен вид топлива в переданной версии");
			}

			var previousVersion = GetPreviousVersionOrNull(version);
			if(previousVersion != null)
			{
				var newEndDate = newStartDate.AddMilliseconds(-1);
				previousVersion.EndDate = newEndDate;
			}
			version.StartDate = newStartDate;
		}

		public void CreateAndAddVersion(decimal fuelPrice, DateTime? startDate)
		{
			if(_isNeedSetFuelType)
			{
				throw new InvalidOperationException(_fuelTypeNotSetErrorMessage);
			}

			if(startDate == null)
			{
				startDate = DateTime.Now;
			}

			var newVersion = new FuelPriceVersion
			{
				FuelType = _fuelType,
				FuelPrice = fuelPrice
			};

			AddNewVersion(newVersion, startDate.Value);
		}

		private void AddNewVersion(FuelPriceVersion newCarFuelVersion, DateTime startDate)
		{
			if(newCarFuelVersion == null)
			{
				throw new ArgumentNullException(nameof(newCarFuelVersion));
			}
			if(newCarFuelVersion.FuelType == null || newCarFuelVersion.FuelType.Id != _fuelType.Id)
			{
				newCarFuelVersion.FuelType = _fuelType;
			}
			newCarFuelVersion.StartDate = startDate;

			if(_fuelType.FuelPriceVersions.Any())
			{
				var currentLatestVersion = _fuelType.FuelPriceVersions.MaxBy(x => x.StartDate).First();
				if(startDate < currentLatestVersion.StartDate.AddDays(1))
				{
					throw new ArgumentException(
						"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
						nameof(startDate));
				}
				currentLatestVersion.EndDate = startDate.AddMilliseconds(-1);
			}

			_fuelType.ObservableFuelPriceVersions.Insert(0, newCarFuelVersion);
		}

		public bool IsValidDateForVersionStartDateChange(FuelPriceVersion version, DateTime newStartDate)
		{
			if(_isNeedSetFuelType)
			{
				throw new InvalidOperationException(_fuelTypeNotSetErrorMessage);
			}

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
			if(_isNeedSetFuelType)
			{
				throw new InvalidOperationException(_fuelTypeNotSetErrorMessage);
			}

			return _fuelType.FuelPriceVersions.All(x => x.StartDate < dateTime);
		}

		private FuelPriceVersion GetPreviousVersionOrNull(FuelPriceVersion currentVersion)
		{
			return _fuelType.FuelPriceVersions
				.Where(x => x.StartDate < currentVersion.StartDate)
				.OrderByDescending(x => x.StartDate)
				.FirstOrDefault();
		}
	}
}
