using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	public class FastDeliveryMaxDistanceParameterVersion : PropertyChangedBase, IDomainObject
	{
		private DateTime _startDate;
		private DateTime? _endDate;
		private double _value;

		public virtual int Id { get; set; }

		[Display(Name = "Дата начала")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[Display(Name = "Дата окончания")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		[Display(Name = "Значение в километрах")]
		public virtual double Value
		{
			get => _value;
			set => SetField(ref _value, value);
		}
	}
}
