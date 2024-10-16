using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace Vodovoz.Core.Domain.Logistics
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Журнал МЛ",
		Nominative = "маршрутный лист")]
	[HistoryTrace]
	[EntityPermission]
	public class RouteListEntity : PropertyChangedBase, IDomainObject, IBusinessObject
	{
		private int _id;
		private DateTime _version;
		private DateTime _date;
		private CarEntity _car;
		private EmployeeEntity _driver;

		public RouteListEntity()
		{
			_date = DateTime.Today;
		}

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		public virtual IUnitOfWork UoW { set; get; }

		[Display(Name = "Дата")]
		[HistoryDateOnly]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}

		[Display(Name = "Машина")]
		public virtual CarEntity Car
		{
			get => _car;
			//Нельзя устанавливать, см. логику в RouteList.cs
			protected set => SetField(ref _car, value);
		}

		[Display(Name = "Водитель")]
		public virtual EmployeeEntity Driver
		{
			get => _driver;
			//Нельзя устанавливать, см. логику в RouteList.cs
			protected set => SetField(ref _driver, value);
		}
	}
}
