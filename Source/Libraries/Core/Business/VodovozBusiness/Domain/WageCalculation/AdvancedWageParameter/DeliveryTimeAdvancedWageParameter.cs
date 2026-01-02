using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace Vodovoz.Domain.WageCalculation.AdvancedWageParameters
{
	[Appellative(
	Gender = GrammaticalGender.Masculine,
	Nominative = "Дополнительный параметр расчета зарплаты по времени доставки"
	)]
	[HistoryTrace]
	public class DeliveryTimeAdvancedWageParameter : AdvancedWageParameter
	{
		public override AdvancedWageParameterType AdvancedWageParameterType { get => AdvancedWageParameterType.DeliveryTime; set { } }

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

		public override string Name => this.ToString();

		public override bool HasConflicWith(IAdvancedWageParameter advancedWageParameter)
		{
			if(!(advancedWageParameter is DeliveryTimeAdvancedWageParameter))
				return true;

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
			if(StartTime == EndTime)
				return StartTime.ToString();

			return $"С {StartTime.ToString()} до {EndTime.ToString()}";
		}

		public override bool IsValidСonditions(IRouteListItemWageCalculationSource scr)
		{
			return scr.DeliverySchedule.Item1 >= StartTime 
				   && scr.DeliverySchedule.Item1<= EndTime;
		}

		public override object Clone()
		{
			var parameter = new DeliveryTimeAdvancedWageParameter
			{
				ForDriverWithForwarder = ForDriverWithForwarder,
				ForDriverWithoutForwarder = ForDriverWithoutForwarder,
				ForForwarder = ForForwarder,
				StartTime = StartTime,
				EndTime = EndTime
			};

			foreach(var child in Children)
			{
				if(child.Clone() is AdvancedWageParameter childParameter)
				{
					childParameter.ParentParameter = parameter;
					parameter.ChildrenParameters.Add(childParameter);
					continue;
				}

				throw new InvalidOperationException("Дочерний узел не является дополнительным параметром расчета зарплаты");
			}

			return parameter;
		}
	}
}
