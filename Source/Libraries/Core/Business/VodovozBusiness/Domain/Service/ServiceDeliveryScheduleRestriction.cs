using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace VodovozBusiness.Domain.Service
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "ограничение времени сервисного района",
		NominativePlural = "ограничения времени сервисного района")]
	[EntityPermission]
	[HistoryTrace]

	public class ServiceDeliveryScheduleRestriction : PropertyChangedBase, IDomainObject, ICloneable
	{
		private ServiceDistrict _serviceDistrict;
		private WeekDayName _weekDay;
		private DeliverySchedule _deliverySchedule;
		private AcceptBefore _acceptBefore;

		public virtual int Id { get; set; }

		[Display(Name = "Сервисный район")]
		public virtual ServiceDistrict ServiceDistrict
		{
			get => _serviceDistrict;
			set => SetField(ref _serviceDistrict, value);
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
			var newServiceScheduleRestriction = new ServiceDeliveryScheduleRestriction
			{
				AcceptBefore = AcceptBefore,
				DeliverySchedule = DeliverySchedule,
				WeekDay = WeekDay
			};
			return newServiceScheduleRestriction;
		}

		public override string ToString()
		{
			var acceptBeforeStr = AcceptBefore == null ? "" : ", Время приема до: " + AcceptBeforeTitle;
			return $" День недели: {WeekDay.GetEnumTitle()}, График доставки: {DeliverySchedule.Name}{acceptBeforeStr}";
		}
	}
}
