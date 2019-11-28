using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;

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
			set => SetField(ref rightSing, value);
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
			if(LeftSing == ComparisonSings.Equally && RightSing == null)
				return ((uint)BottlesFrom, (uint)BottlesFrom);

			uint? from = null;
			uint? to = null;


			switch(LeftSing) {
				case ComparisonSings.Less:
					from = (uint)BottlesFrom + 1;
					break;
				case ComparisonSings.LessOrEqual:
					from = (uint)BottlesFrom;
					break;
			}

			if(RightSing == null) 
			{
				switch(LeftSing) 
				{
					case ComparisonSings.More:
						to = (uint)BottlesFrom - 1;
						break;
					case ComparisonSings.MoreOrEqual:
						to = (uint)BottlesFrom;
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
						throw new ArgumentOutOfRangeException(nameof(RightSing));
				}
			}

			return (from ?? 0, to ?? int.MaxValue);
		}

		public override bool HasConflicWith(IAdvancedWageParameter advancedWageParameter)
		{
			if(!(advancedWageParameter is BottlesCountAdvancedWageParameter))
				return true;

			var bottleParam = advancedWageParameter as BottlesCountAdvancedWageParameter;

			(uint, uint) range = GetCountRange();
			(uint, uint) anotherParamRange = bottleParam.GetCountRange();

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
		[Display(Name = ">=)")]
		MoreOrEqual
	}


	public class ComparisonSingStringType : NHibernate.Type.EnumStringType
	{
		public ComparisonSingStringType() : base(typeof(ComparisonSings))
		{
		}
	}
}
