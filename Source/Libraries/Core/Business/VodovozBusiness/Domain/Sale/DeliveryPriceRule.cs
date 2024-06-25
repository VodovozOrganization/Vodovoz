using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Vodovoz.EntityRepositories.Sale;

namespace Vodovoz.Domain.Sale
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "правила цен доставки",
		Nominative = "правило цены доставки",
		Prepositional = "правиле цены доставки",
		PrepositionalPlural = "правилах цен доставки")]
	[EntityPermission]
	public class DeliveryPriceRule : BusinessObjectBase<DeliveryPriceRule>, IDomainObject, IDeliveryPriceRule, IValidatableObject
	{
		private string _ruleName;
		private int _water19LCount;
		private int _water6LCount;
		private int _water1500mlCount;
		private int _water600mlCount;
		private int _water500mlCount;
		private decimal _orderMinSumEShopGoods;

		#region Свойства

		public virtual int Id { get; set; }

		public virtual string Title => $"{this}";

		[Display(Name = "Количество 19л бутылей в заказе")]
		public virtual int Water19LCount
		{
			get => _water19LCount;
			set => SetField(ref _water19LCount, value);
		}

		[Display(Name = "Минимальная сумма товаров ИМ")]
		public virtual decimal OrderMinSumEShopGoods
		{
			get => _orderMinSumEShopGoods;
			set => SetField(ref _orderMinSumEShopGoods, value);
		}

		[Display(Name = "Количество 6л бутылей в заказе")]
		public virtual int Water6LCount
		{
			get => _water6LCount;
			set => SetField(ref _water6LCount, value);
		}

		[Display(Name = "Количество 1,5л бутылей в заказе")]
		public virtual int Water1500mlCount
		{
			get => _water1500mlCount;
			set => SetField(ref _water1500mlCount, value);
		}

		[Display(Name = "Количество 0,6л бутылей в заказе")]
		public virtual int Water600mlCount
		{
			get => _water600mlCount;
			set => SetField(ref _water600mlCount, value);
		}

		[Display(Name = "Количество 0,5л бутылей в заказе")]
		public virtual int Water500mlCount
		{
			get => _water500mlCount;
			set => SetField(ref _water500mlCount, value);
		}

		[Display(Name = "Название или краткое описание правила")]
		public virtual string RuleName
		{
			get => _ruleName;
			set => SetField(ref _ruleName, value);
		}

		#endregion Свойства

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.GetService(
				typeof(IDistrictRuleRepository)) is IDistrictRuleRepository districtRuleRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(districtRuleRepository)}");
			}

			if(districtRuleRepository.GetAllDeliveryPriceRules(UoW).Where(r => r.Id != Id).Contains(this))
			{
				yield return new ValidationResult("Такое правило уже существует и нельзя его создавать");
			}
		}

		#endregion IValidatableObject implementation

		/// <summary>
		/// Строки литража, участвует в <see cref="GetVolumeValue(string)"/>
		/// 600мл отключено
		/// </summary>
		public static string[] Volumes = { "19л", "6л", "1,5л", /*"0,6л",*/ "0,5л" };

		/// <summary>
		/// Возвращает значение по строке обозначающей литраж <see cref="Volumes"/>
		/// </summary>
		/// <param name="volume"></param>
		/// <returns></returns>
		public virtual string GetVolumeValue(string volume)
		{
			switch(volume)
			{
				case "19л": return Water19LCount.ToString();
				case "6л": return Water6LCount.ToString();
				case "1,5л": return Water1500mlCount.ToString();
				case "0,6л": return Water600mlCount.ToString();
				case "0,5л": return _water500mlCount.ToString();
				default: return "";
			}
		}

		#region Переопределённые методы

		public override string ToString()
		{
			var sb = new StringBuilder();

			if(Water19LCount > 0
				|| Water6LCount > 0
				|| Water1500mlCount > 0
				|| Water600mlCount > 0
				|| Water500mlCount > 0)
			{
				sb.Append("Если");
				sb.Append($" 19л б. < {Water19LCount}шт.");
				sb.Append($" или 6л б. < {_water6LCount}шт.");
				sb.Append($" или 1500мл б. < {_water1500mlCount}шт.");
				sb.Append($" или 500мл б. < {Water500mlCount}шт.");
			}

			return sb.ToString().Trim(' ', ',', 'и');
		}

		public override bool Equals(object obj)
		{
			if(obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			var rule = (DeliveryPriceRule)obj;

			bool result = Water19LCount == rule.Water19LCount
				&& Water6LCount == rule.Water6LCount
				&& Water1500mlCount == rule.Water1500mlCount
				&& Water600mlCount == rule.Water600mlCount
				&& Water500mlCount == rule.Water500mlCount
				&& OrderMinSumEShopGoods == rule.OrderMinSumEShopGoods;

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
			return 31 * Water19LCount.GetHashCode()
				+ 31 * Water6LCount.GetHashCode()
				+ 31 * Water1500mlCount.GetHashCode()
				+ 31 * Water600mlCount.GetHashCode()
				+ 31 * _water500mlCount.GetHashCode();
		}

		#endregion Переопределённые методы
	}
}
