using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "ограничение времени доставки района",
		NominativePlural = "ограничения времени доставки района")]
	[EntityPermission]
	public class DeliveryScheduleRestriction : PropertyChangedBase, IDomainObject, ICloneable
	{
		private District _district;
		private WeekDayName _weekDay;
		private DeliverySchedule _deliverySchedule;
		private AcceptBefore _acceptBefore;

		public virtual int Id { get; set; }

		[Display(Name = "Район")]
		public virtual District District
		{
			get => _district;
			set => SetField(ref _district, value);
		}

		[Display(Name = "День недели")]
		public virtual WeekDayName WeekDay
		{
			get => _weekDay;
			set => SetField(ref _weekDay, value);
		}

		[Display(Name = "График доставки")]
		public virtual DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set => SetField(ref _deliverySchedule, value);
		}

		[Display(Name = "Прием до")]
		public virtual AcceptBefore AcceptBefore
		{
			get => _acceptBefore;
			set => SetField(ref _acceptBefore, value);
		}

		public virtual string AcceptBeforeTitle => AcceptBefore?.Name ?? "";

		public virtual object Clone()
		{
			var newDeliveryScheduleRestriction = new DeliveryScheduleRestriction
			{
				AcceptBefore = AcceptBefore,
				DeliverySchedule = DeliverySchedule,
				WeekDay = WeekDay
			};
			return newDeliveryScheduleRestriction;
		}

		public override string ToString()
		{
			var acceptBeforeStr = AcceptBefore == null ? "" : ", Время приема до: " + AcceptBeforeTitle;
			return $" День недели: {WeekDay.GetEnumTitle()}, График доставки: {DeliverySchedule.Name}{acceptBeforeStr}";
		}
	}
}
