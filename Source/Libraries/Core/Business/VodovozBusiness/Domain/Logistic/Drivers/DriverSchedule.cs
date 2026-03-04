using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.Domain.Logistic.Drivers
{
	/// <summary>
	/// График водителей
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Accusative = "график водителей",
		AccusativePlural = "графики водителей",
		Genitive = "графика водителей",
		GenitivePlural = "графиков водителей",
		Nominative = "график водителей",
		NominativePlural = "графики водителей",
		Prepositional = "графике водителей",
		PrepositionalPlural = "графиках водителей")]
	[HistoryTrace]
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
