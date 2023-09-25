using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;

namespace Vodovoz.Domain.Goods.PromotionalSets
{
	public abstract class PromotionalSetActionBase : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		PromotionalSet promotionalSet;
		[Display(Name = "Промо-набор")]
		public virtual PromotionalSet PromotionalSet {
			get { return promotionalSet; }
			set { SetField(ref promotionalSet, value, () => PromotionalSet); }
		}

		public abstract void Activate(Order order);
		public abstract void Deactivate(Order order);
		public abstract bool IsValidForOrder(Order order, IStandartNomenclatures standartNomenclatures);

		public abstract string Title { get; }
	}
}
