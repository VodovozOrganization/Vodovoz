using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.Domain.Logistic.Drivers
{
	public class DriverSchedule : DriverScheduleEntity
	{
		private Employee _driver;

		/// <summary>
		/// Водитель
		/// </summary>
		[Display(Name = "Водитель")]
		public virtual new Employee Driver
		{
			get => _driver;
			set => _driver = value;
		}
	}
}
