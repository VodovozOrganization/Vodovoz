using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Controllers
{
	public class OdometerReadingsController : IOdometerReadingsController
	{
		public OdometerReadingsController(Car car)
		{
			Car = car ?? throw new ArgumentNullException(nameof(car));
		}

		///  <summary>
		///  Добавляет новую строку показаний одометра автомобиля в список показаний одометра автомобиля контроллера.
		///  Если предыдущее показание не имело дату окончания или заканчивалась позже даты начала нового показания,
		///  то в этом показании выставляется дата окончания, равная дате начала нового показания минус 1 миллисекунду
		///  </summary>
		///  <param name="newOdometerReading">Новое показание одометра автомобиля. Свойство StartDate в newOdometerReading игнорируется</param>
		///  <param name="startDate">
		/// 	Дата начала действия нового показания одометра. Должна быть минимум на день позже, чем дата начала действия предыдущего показания.
		/// 	Время должно равняться 00:00:00
		///  </param>
		private void AddNewOdometerReading(OdometerReading newOdometerReading, DateTime startDate)
		{
			if(newOdometerReading == null)
			{
				throw new ArgumentNullException(nameof(newOdometerReading));
			}
			if(startDate.Date != startDate)
			{
				throw new ArgumentException("Время даты начала действия новой версии не равно 00:00:00", nameof(startDate));
			}
			if(newOdometerReading.Car == null || newOdometerReading.Car.Id != Car.Id)
			{
				newOdometerReading.Car = Car;
			}

			if(Car.OdometerReadings.Any())
			{
				var currentLatestVersion = Car.OdometerReadings.MaxBy(x => x.StartDate).First();
				if(startDate < currentLatestVersion.StartDate.AddDays(1))
				{
					throw new ArgumentException(
						"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
						nameof(startDate));
				}
				currentLatestVersion.EndDate = startDate.AddMilliseconds(-1);
			}

			newOdometerReading.StartDate = startDate;
			Car.ObservableOdometerReadings.Insert(0, newOdometerReading);
		}

		private OdometerReading GetPreviousVersionOrNull(OdometerReading currentVersion)
		{
			return Car.OdometerReadings
				.Where(x => x.StartDate < currentVersion.StartDate)
				.OrderByDescending(x => x.StartDate)
				.FirstOrDefault();
		}

		public Car Car { get; }

		/// <summary>
		///  Создаёт и добавляет новое показание одометра автомобиля в список показаний одомиетра.
		/// </summary>
		/// <param name="startDate">Дата начала действия нового показания. Если равно null, берётся текущая дата</param>
		public void CreateAndAddOdometerReading(DateTime? startDate = null)
		{
			if(startDate == null)
			{
				startDate = DateTime.Now;
			}

			var newVersion = new OdometerReading
			{
				Car = Car
			};

			AddNewOdometerReading(newVersion, startDate.Value);
		}

		public void ChangeOdometerReadingStartDate(OdometerReading version, DateTime newStartDate)
		{
			if(version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}
			if(version.Car == null || version.Car.Id != Car.Id)
			{
				throw new ArgumentException("Неверно заполнен автомобиль в переданной версии");
			}

			var previousVersion = GetPreviousVersionOrNull(version);
			if(previousVersion != null)
			{
				var newEndDate = newStartDate.AddMilliseconds(-1);
				previousVersion.EndDate = newEndDate;
			}
			version.StartDate = newStartDate;
		}

		public bool IsValidDateForOdometerReadingStartDateChange(OdometerReading version, DateTime newStartDate)
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

		public bool IsValidDateForNewOdometerReading(DateTime dateTime)
		{
			return Car.OdometerReadings.All(x => x.StartDate < dateTime);
		}
	}
}
