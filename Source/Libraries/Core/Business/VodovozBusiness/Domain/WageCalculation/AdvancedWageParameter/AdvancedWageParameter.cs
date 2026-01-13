using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace Vodovoz.Domain.WageCalculation.AdvancedWageParameters
{
	[Appellative(
	Gender = GrammaticalGender.Masculine,
	NominativePlural = "дополнительные параметры расчёта зарплаты",
	Nominative = "дополнительный параметр расчёта зарплаты"
	)]
	public abstract class AdvancedWageParameter : PropertyChangedBase, IAdvancedWageParameter , IValidatableObject
	{
		public virtual int Id { get; set; }

		private AdvancedWageParameter parentParameter;
		public virtual AdvancedWageParameter ParentParameter {
			get => parentParameter;
			set { 
				SetField(ref parentParameter, value, () => ParentParameter);
				if(value != null)
					wageRate = null;
			}
		}

		private WageRate wageRate;
		[Display(Name = "Тип ставки для расчета зарплаты")]
		public virtual WageRate WageRate { //Устанавливается только у корневого элемента в иерархии дополнительных параметров
			get => wageRate;
			set {
				SetField(ref wageRate, value);
				if(value != null)
					parentParameter = null;
			}
		}

		//Для отображение в иерархическом списке
		public virtual IList<IWageHierarchyNode> Children {
			get => ChildrenParameters.OfType<IWageHierarchyNode>().ToList() ?? new List<IWageHierarchyNode>();
		}

		private IList<AdvancedWageParameter> childrenParameters;
		[Display(Name = "Дополнительные параметры расчета зп")]
		public virtual IList<AdvancedWageParameter> ChildrenParameters {
			get {
				if(childrenParameters == null)
					childrenParameters = new List<AdvancedWageParameter>(); 
				return childrenParameters; 
				}
			set { SetField(ref childrenParameters, value); }
		}

		private decimal forDriverWithForwarder;
		[Display(Name = "Величина ставки при наличии экспедитора")]
		public virtual decimal ForDriverWithForwarder {
			get => forDriverWithForwarder;
			set => SetField(ref forDriverWithForwarder, value);
		}

		private decimal forDriverWithoutForwarder;
		[Display(Name = "Величина ставки при отсутствии экспедитора")]
		public virtual decimal ForDriverWithoutForwarder {
			get => forDriverWithoutForwarder;
			set => SetField(ref forDriverWithoutForwarder, value);
		}

		private decimal forForwarder;
		[Display(Name = "Величина ставки для экспедитора")]
		public virtual decimal ForForwarder {
			get => forForwarder;
			set => SetField(ref forForwarder, value);
		}

		public abstract AdvancedWageParameterType AdvancedWageParameterType { get; set; }

		IAdvancedWageParameter IAdvancedWageParameter.ParentParameter => ParentParameter;

		public virtual IWageHierarchyNode Parent {
			get => WageRate as IWageHierarchyNode ?? ParentParameter as IWageHierarchyNode;
			set {
				if(value is WageRate wageRate)
					WageRate = wageRate;
				if(value is AdvancedWageParameter parameter)
					ParentParameter = parameter;
				if(value == null) {
					ParentParameter = null;
					wageRate = null;
				}
			}
		}

		public abstract string Name { get; }

		public virtual string GetUnitName
		{
			get {
				if(WageRate != null)
					return WageRate.GetUnitName;

				var wageParam = this;
				while(wageParam.ParentParameter != null)
					wageParam = wageParam.ParentParameter;

				return wageParam?.WageRate?.GetUnitName ?? string.Empty;
			}
		}

		public abstract bool HasConflicWith(IAdvancedWageParameter advancedWageParameter);

		public abstract override string ToString();

		public abstract bool IsValidСonditions(IRouteListItemWageCalculationSource scr);

		public virtual RouteListWageNode CalculateWage(IRouteListItemWageCalculationSource src)
		{
			if(!IsValidСonditions(src))
				return null;

			if(ChildrenParameters?.FirstOrDefault() == null)
				return new RouteListWageNode(forDriverWithForwarder, forDriverWithoutForwarder, forForwarder);

			foreach(var param in ChildrenParameters) {
				var wageNode = param.CalculateWage(src);
				if(wageNode != null)
					return wageNode;
			}

			return new RouteListWageNode(forDriverWithForwarder, forDriverWithoutForwarder, forForwarder);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var parameters = Parent.Children?.Where(x => x != this).OfType<AdvancedWageParameter>();

			if(parameters == null)
				yield break;

			foreach(var param in parameters) {
				if(HasConflicWith(param))
					yield return new ValidationResult($"Конфликт {this.ToString()} с {param.ToString()}");
			}
		}

		public abstract object Clone();
	}

	public class RouteListWageNode
	{

		public decimal ForDriverWithForwarder;

		public decimal ForDriverWithoutForwarder;

		public decimal ForForwarder;

		public RouteListWageNode(decimal forDriverWithForwarder, decimal forDriverWithoutForwarder, decimal forForwarder)
		{
			ForDriverWithForwarder = forDriverWithForwarder;
			ForDriverWithoutForwarder = forDriverWithoutForwarder;
			ForForwarder = forForwarder;
		}
	}
}
