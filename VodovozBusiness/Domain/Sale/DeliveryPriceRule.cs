using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Parameters;

namespace Vodovoz.Domain.Sale
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "правила цен доставки",
			Nominative = "правило цены доставки",
			Prepositional = "правиле цены доставки",
			PrepositionalPlural = "правилах цен доставки"
		)
	]
	[EntityPermission]
	public class DeliveryPriceRule : BusinessObjectBase<DeliveryPriceRule>, IDomainObject, IDeliveryPriceRule, IValidatableObject
	{
		private readonly IParametersProvider _parametersProvider = new ParametersProvider();
		
		#region Свойства

		public virtual int Id { get; set; }

		public virtual string Title => $"{this} или сумма товаров ИМ < {OrderMinSumEShopGoods}р.";
		
		int water19LCount;
		[Display(Name = "Количество 19л бутылей в заказе")]
		public virtual int Water19LCount {
			get => water19LCount;
			set {
				if(SetField(ref water19LCount, value, () => Water19LCount)) {
					OnPropertyChanged(() => Water6LCount);
					OnPropertyChanged(() => Water1500mlCount);
					OnPropertyChanged(() => Water600mlCount);
					OnPropertyChanged(() => Water500mlCount);
				}
			}
		}
		
		decimal orderMinSumEShopGoods;
		[Display(Name = "Минимальная сумма товаров ИМ")]
		public virtual decimal OrderMinSumEShopGoods {
			get => orderMinSumEShopGoods;
			set => SetField(ref orderMinSumEShopGoods, value);
		}

		private int? equalsCount6LFor19L;
		[Display(Name = "Количество 6л бутылей на одну 19л бутыль")]
		public virtual int EqualsCount6LFor19L => 
			equalsCount6LFor19L ?? (equalsCount6LFor19L = int.Parse(_parametersProvider.GetParameterValue("эквивалент_6л_на_1бутыль_19л"))).Value;

		private int? equalsCount1500mlFor19L;
		[Display(Name = "Количество 1,5л бутылей на одну 19л бутыль")]
		public virtual int EqualsCount1500mlFor19L => 
			equalsCount1500mlFor19L ?? (equalsCount1500mlFor19L = int.Parse(_parametersProvider.GetParameterValue("эквивалент_1,5л_на_1бутыль_19л"))).Value;
		
		private int? equalsCount600mlFor19L;
		[Display(Name = "Количество 0,6л бутылей на одну 19л бутыль")]
		public virtual int EqualsCount600mlFor19L => 
			equalsCount600mlFor19L ?? (equalsCount600mlFor19L = int.Parse(_parametersProvider.GetParameterValue("эквивалент_0,6л_на_1бутыль_19л"))).Value;
		
		private int? equalsCount500mlFor19L;
		[Display(Name = "Количество 0,5л бутылей на одну 19л бутыль")]
		public virtual int EqualsCount500mlFor19L => 
			equalsCount500mlFor19L ?? (equalsCount500mlFor19L = int.Parse(_parametersProvider.GetParameterValue("эквивалент_0,5л_на_1бутыль_19л"))).Value;

		[Display(Name = "Количество 6л бутылей в заказе")]
		public virtual string Water6LCount => (water19LCount * EqualsCount6LFor19L).ToString();

		[Display(Name = "Количество 1,5л бутылей в заказе")]
		public virtual string Water1500mlCount => (water19LCount * EqualsCount1500mlFor19L).ToString();
		
		[Display(Name = "Количество 0,6л бутылей в заказе")]
		public virtual string Water600mlCount => (water19LCount * EqualsCount600mlFor19L).ToString();
		
		[Display(Name = "Количество 0,5л бутылей в заказе")]
		public virtual string Water500mlCount => (water19LCount * EqualsCount500mlFor19L).ToString();

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.ServiceContainer.GetService(
				typeof(IDistrictRuleRepository)) is IDistrictRuleRepository districtRuleRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(districtRuleRepository) }");
			}
			
			if(districtRuleRepository.GetAllDeliveryPriceRules(UoW).Where(r => r.Id != Id).Contains(this))
			{
				yield return new ValidationResult("Такое правило уже существует и нельзя его создавать");
			}
		}

		#endregion

		#region переопределённые методы

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if(Water19LCount > 0) {
				sb.Append("Если");
				sb.Append($" 19л б. < {Water19LCount}шт.");
				sb.Append($" или 6л б. < {water19LCount * EqualsCount6LFor19L}шт.");
				sb.Append($" или 1500мл б. < {Water19LCount * EqualsCount1500mlFor19L}шт.");
				sb.Append($" или 600мл б. < {Water19LCount * EqualsCount600mlFor19L}шт.");
				sb.Append($" или 500мл б. < {Water19LCount * EqualsCount500mlFor19L}шт.");
			}

			return sb.ToString().Trim(' ', ',', 'и');
		}

		public override bool Equals(object obj)
		{
			if(obj == null || this.GetType() != obj.GetType())
				return false;

			DeliveryPriceRule rule = (DeliveryPriceRule)obj;
			bool result = this.Water19LCount == rule.Water19LCount
				&& this.OrderMinSumEShopGoods == rule.OrderMinSumEShopGoods;
			return result;
		}

		public static bool operator ==(DeliveryPriceRule x, DeliveryPriceRule y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(DeliveryPriceRule x, DeliveryPriceRule y)
		{
			return !(x == y);
		}

		public override int GetHashCode()
		{
			return 31 * Water19LCount.GetHashCode();
		}

		#endregion
	}
}