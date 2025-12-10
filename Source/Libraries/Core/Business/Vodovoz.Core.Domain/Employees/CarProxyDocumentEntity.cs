using QS.Print;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace Vodovoz.Core.Domain.Employees
{
	public class CarProxyDocumentEntity : ProxyDocumentEntity
	{
		private DateTime _date;
		private EmployeeEntity _driver;
		private CarEntity _car;
		private DateTime _expirationDate;

		/// <summary>
		/// Заголовок
		/// </summary>
		[Display(Name = "Заголовок")]
		public virtual string Title => $"Доверенность на ТС № {0}";

		public override ProxyDocumentType Type => ProxyDocumentType.CarProxy;

		public override PrinterType PrintType => PrinterType.ODT;

		/// <summary>
		/// Дата доверенности
		/// </summary>
		[Display(Name = "Дата доверенности")]
		public override DateTime Date
		{
			get => _date;
			set
			{
				SetField(ref _date, value);
				ExpirationDate = _date.AddYears(1);
			}
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
		/// Автомобиль
		/// </summary>
		[Display(Name = "Автомобиль")]
		public virtual CarEntity Car
		{
			get =>_car; 
			set => SetField(ref _car, value); 
		}

		public override DateTime ExpirationDate
		{
			get => _expirationDate;
			set => SetField(ref _expirationDate, value);
		}
	}
}
