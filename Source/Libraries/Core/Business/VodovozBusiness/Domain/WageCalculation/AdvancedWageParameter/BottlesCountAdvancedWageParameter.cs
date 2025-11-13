using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace Vodovoz.Domain.WageCalculation.AdvancedWageParameters
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "Дополнительный параметр расчета зарплаты по кол-ву бутылей в заказе"
	)]
	[HistoryTrace]
	public class BottlesCountAdvancedWageParameter : AdvancedWageParameter
	{
		[IgnoreHistoryTrace]
		public override AdvancedWageParameterType AdvancedWageParameterType { get => AdvancedWageParameterType.BottlesCount; set { } }

		private uint bottlesFrom;
		[Display(Name = "От")]
		public virtual uint BottlesFrom {
			get => bottlesFrom;
			set => SetField(ref bottlesFrom, value);
		}

		private ComparisonSings leftSing;
		[Display(Name = "Левый знак сравнения")]
		public virtual ComparisonSings LeftSing {
			get => leftSing;
			set => SetField(ref leftSing, value);
		}

		private ComparisonSings? rightSing;
		[Display(Name = "Правый знак сравнения")]
		public virtual ComparisonSings? RightSing {
			get => rightSing;
			set 
			{
				if(value == null)
					bottlesTo = null;
				SetField(ref rightSing, value);
			}
		}

		private uint? bottlesTo;
		[Display(Name = "До")]
		public virtual uint? BottlesTo {
			get => bottlesTo;
			set => SetField(ref bottlesTo, value);
		}

		public override string Name => this.ToString();

		public virtual (uint,uint) GetCountRange()
		{
			if(BottlesTo == null || RightSing == null) {
				BottlesTo = null;
				RightSing = null;
			}

			if(LeftSing == ComparisonSings.Equally)
				return (BottlesFrom, BottlesFrom);

			uint? from = null;
			uint? to = null;


			switch(LeftSing) {
				case ComparisonSings.Less:
					from = BottlesFrom + 1;
					break;
				case ComparisonSings.LessOrEqual:
					from = BottlesFrom;
					break;
			}

			if(RightSing == null) 
			{
				switch(LeftSing) 
				{
					case ComparisonSings.More:
						to = BottlesFrom - 1;
						break;
					case ComparisonSings.MoreOrEqual:
						to = BottlesFrom;
						break;
				}
			} else {
				switch(RightSing) {
					case ComparisonSings.Less:
						to = BottlesTo - 1;
						break;
					case ComparisonSings.LessOrEqual:
						to = BottlesTo;
						break;
					case ComparisonSings.More:
					case ComparisonSings.MoreOrEqual:
					case ComparisonSings.Equally:
						throw new ArgumentException(nameof(RightSing));
				}

				if(LeftSing == ComparisonSings.More && to > (BottlesFrom - 1)) {
					to = BottlesFrom - 1;
					from = 0;
				}
				if(LeftSing == ComparisonSings.MoreOrEqual && to > BottlesFrom) {
					to = BottlesFrom;
					from = 0;
				}
			}

			var res = (from ?? 0, to ?? int.MaxValue);

			if(res.Item1 > res.Item2)
				throw new ArgumentException($" Параметр расчета зп: {this} не может быть расчитан");

			return (from ?? 0, to ?? int.MaxValue);
		}

		public override bool HasConflicWith(IAdvancedWageParameter advancedWageParameter)
		{
			if(!(advancedWageParameter is BottlesCountAdvancedWageParameter))
				return true;

			var bottleParam = advancedWageParameter as BottlesCountAdvancedWageParameter;

			(uint, uint) range;
			(uint, uint) anotherParamRange;
			try {
				range = GetCountRange();
				anotherParamRange = bottleParam.GetCountRange();
			}catch(ArgumentException) {
				return true;
			}

			if(anotherParamRange.Item1 >= range.Item1 && anotherParamRange.Item1 <= range.Item2)
				return true;
			if(anotherParamRange.Item2 >= range.Item1 && anotherParamRange.Item2 <= range.Item2)
				return true;
			if(range.Item1 >= anotherParamRange.Item1 && range.Item1 <= anotherParamRange.Item2)
				return true;
			if(range.Item2 >= anotherParamRange.Item1 && range.Item2 <= anotherParamRange.Item2)
				return true;

			return false;
		}

		public override string ToString()
		{
			string param = $"{BottlesFrom} {LeftSing.GetAttribute<DisplayAttribute>()?.Name} кол-во бутылей";
			if(BottlesTo != null && RightSing != null)
				param += $" {RightSing.GetAttribute<DisplayAttribute>()?.Name} {BottlesTo}";
			return param;
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			bool isValid = true;

			if(LeftSing == ComparisonSings.Equally && RightSing != null && BottlesTo != null)
				isValid = false;

			try {
				GetCountRange();
			} catch(ArgumentException) {
				isValid = false;
			}

			if(!isValid)
				yield return new ValidationResult($"Неравенство содержит конфликтующие условия : {this}");

			var result = base.Validate(validationContext);

			foreach(var item in result)
				yield return item;
		}

		public override bool IsValidСonditions(IRouteListItemWageCalculationSource scr)
		{
			(uint, uint) range;

			try {
				range = GetCountRange();
			}
			catch(ArgumentException) {
				return false;
			}
			return scr.FullBottle19LCount >= range.Item1 
				   && scr.FullBottle19LCount <= range.Item2;
		}
	}

	public enum ComparisonSings
	{
		[Display(Name ="=")]
		Equally,
		[Display(Name = "<")]
		Less,
		[Display(Name = "<=")]
		LessOrEqual,
		[Display(Name = ">")]
		More,
		[Display(Name = ">=")]
		MoreOrEqual
	}
}
