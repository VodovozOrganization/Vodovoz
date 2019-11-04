using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.WageCalculation.AdvancedWageParameter
{
	[Appellative(
	Gender = GrammaticalGender.Masculine,
	NominativePlural = "Расчет зарплаты по времени доставки"
	)]
	[HistoryTrace]
	public class DeliveryTimeAdvancedWageParameter : AdvancedWageParameter
	{
		private TimeSpan startTime;
		[Display(Name = "Время доставки от")]
		public virtual TimeSpan StartTime {
			get => startTime;
			set => SetField(ref startTime, value);
		}

		private TimeSpan endTime;
		[Display(Name = "Время доставки до")]
		public virtual TimeSpan EndTime {
			get => endTime;
			set => SetField(ref endTime, value);
		}

		public override AdvancedWageParameterType AdvancedWageParameterType => throw new NotImplementedException();

		public override bool HasConflicWith(IAdvancedWageParameter advancedWageParameter)
		{
			if(!(advancedWageParameter is DeliveryTimeAdvancedWageParameter))
				throw new ArgumentException();

			var wageParam = advancedWageParameter as DeliveryTimeAdvancedWageParameter;

			if(wageParam.StartTime >= StartTime && wageParam.StartTime <= EndTime)
				return true;

			if(wageParam.EndTime >= StartTime && wageParam.EndTime <= EndTime)
				return true;

			if(StartTime >= wageParam.StartTime && StartTime <= wageParam.EndTime)
				return true;

			if(EndTime >= wageParam.StartTime && EndTime <= wageParam.EndTime)
				return true;

			return false;
		}

		public override string ToString()
		{
			return $"С {StartTime.ToString()} до {EndTime.ToString()}";
		}
	}
}
