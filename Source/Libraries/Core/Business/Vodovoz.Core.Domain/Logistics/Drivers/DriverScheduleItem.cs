using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace Vodovoz.Core.Domain.Logistics.Drivers
{
	/// <summary>
	/// День из графика водителя
	/// </summary>
	public class DriverScheduleItem : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DriverScheduleEntity _driverSchedule;
		private DateTime _date;
		private CarEventType _carEventType;
		private int _morningAddresses;
		private int _morningBottles;
		private int _eveningAddresses;
		private int _eveningBottles;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// График водителя
		/// </summary>
		[Display(Name = "График водителя")]
		public virtual DriverScheduleEntity DriverSchedule
		{
			get => _driverSchedule;
			set => SetField(ref _driverSchedule, value);
		}

		/// <summary>
		/// Дата
		/// </summary>
		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		/// <summary>
		/// Вид события ТС
		/// </summary>
		[Display(Name = "Вид события ТС")]
		public virtual CarEventType CarEventType
		{
			get => _carEventType;
			set => SetField(ref _carEventType, value);
		}

		/// <summary>
		/// Фактическое количество адресов утром
		/// </summary>
		[Display(Name = "Фактическое количество адресов утром")]
		public virtual int MorningAddresses
		{
			get => _morningAddresses;
			set => SetField(ref _morningAddresses, value);
		}

		/// <summary>
		/// Фактическое количество бутылей утром
		/// </summary>
		[Display(Name = "Фактическое количество бутылей утром")]
		public virtual int MorningBottles
		{
			get => _morningBottles;
			set => SetField(ref _morningBottles, value);
		}

		/// <summary>
		/// Фактическое количество адресов вечером
		/// </summary>
		[Display(Name = "Фактическое количество адресов вечером")]
		public virtual int EveningAddresses
		{
			get => _eveningAddresses;
			set => SetField(ref _eveningAddresses, value);
		}

		/// <summary>
		/// Фактическое количество бутылей вечером
		/// </summary>
		[Display(Name = "Фактическое количество бутылей вечером")]
		public virtual int EveningBottles
		{
			get => _eveningBottles;
			set => SetField(ref _eveningBottles, value);
		}
	}
}
