using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Domain.Logistics.Drivers
{
	public class DriverScheduleEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private EmployeeEntity _driver;
		private int _morningAddressesPotential;
		private int _morningBottlesPotential;
		private int _eveningAddressesPotential;
		private int _eveningBottlesPotential;
		private DateTime _lastChangeTime;
		private IList<DriverScheduleItem> _days;
		private string _comment;

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
		/// Водитель
		/// </summary>
		[Display(Name = "Водитель")]
		public virtual EmployeeEntity Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		/// <summary>
		/// Потенциал адресов утром
		/// </summary>
		[Display(Name = "Потенциал адресов утром")]
		public virtual int MorningAddressesPotential
		{
			get => _morningAddressesPotential;
			set => SetField(ref _morningAddressesPotential, value);
		}

		/// <summary>
		/// Потенциал бутылей утром
		/// </summary>
		[Display(Name = "Потенциал бутылей утром")]
		public virtual int MorningBottlesPotential
		{
			get => _morningBottlesPotential;
			set => SetField(ref _morningBottlesPotential, value);
		}

		/// <summary>
		/// Потенциал адресов вечером
		/// </summary>
		[Display(Name = "Потенциал адресов вечером")]
		public virtual int EveningAddressesPotential
		{
			get => _eveningAddressesPotential;
			set => SetField(ref _eveningAddressesPotential, value);
		}

		/// <summary>
		/// Потенциал бутылей вечером
		/// </summary>
		[Display(Name = "Потенциал бутылей вечером")]
		public virtual int EveningBottlesPotential
		{
			get => _eveningBottlesPotential;
			set => SetField(ref _eveningBottlesPotential, value);
		}

		/// <summary>
		/// Последнее время изменения
		/// </summary>
		[Display(Name = "Последнее время изменения")]
		public virtual DateTime LastChangeTime
		{
			get => _lastChangeTime;
			set => SetField(ref _lastChangeTime, value);
		}

		/// <summary>
		/// Дни расписания
		/// </summary>
		[Display(Name = "Дни расписания")]
		public virtual IList<DriverScheduleItem> Days
		{
			get => _days;
			set => SetField(ref _days, value);
		}

		/// <summary>
		/// Комментарий
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}
	}
}
