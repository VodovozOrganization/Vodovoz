using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
	NominativePlural = "Спец. действия фиксированные цены",
	Nominative = "Спец. действие фиксированныа цена")]
	[HistoryTrace]
	public class PromotionalSetActionFixPrice : PromotionalSetActionBase, IValidatableObject
	{
		Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value, () => Nomenclature); }
		}

		decimal price;
		[Display(Name = "Фиксированная цена")]
		public virtual decimal Price {
			get { return price; }
			set { SetField(ref price, value, () => Price); }
		}

		bool isForZeroDebt;
		[Display(Name = "Только для клиентов без долгов")]
		public virtual bool IsForZeroDebt {
			get { return isForZeroDebt; }
			set { SetField(ref isForZeroDebt, value, () => IsForZeroDebt); }
		}

		public override string Title {
			get {
				var addiotionalTitle = isForZeroDebt ? "(Только для клиентов без долгов)" : "(Для всех клиентов)";
				return $"Фиксированная цена {Price}р. на {Nomenclature.ShortOrFullName} {addiotionalTitle}";
			}
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Nomenclature == null)
				yield return new ValidationResult("Необходимо выбрать номенклатуру");
			if(Price < 0)
				yield return new ValidationResult("Фиксированная цена не может быть отрицательной");
			if(PromotionalSet.ObservablePromotionalSetActions.Cast<PromotionalSetActionFixPrice>().Any(a => a.Nomenclature == Nomenclature))
				yield return new ValidationResult("Фиксированная цена на такую номенклатуру уже создана");
		}

		public override void Activate(Order order)
		{
			if(order == null){
				return;
			}
			IList<NomenclatureFixedPrice> fixedPrices = order.Client.NomenclatureFixedPrices;
			if(order.DeliveryPoint != null){
				fixedPrices = order.DeliveryPoint.NomenclatureFixedPrices;
			}

			var foundFixedPrice = fixedPrices.FirstOrDefault(x => x.Nomenclature.Id == Nomenclature.Id);
			if(foundFixedPrice == null) {
				foundFixedPrice = new NomenclatureFixedPrice();
				fixedPrices.Add(foundFixedPrice);
			}
			
			foundFixedPrice.Nomenclature = Nomenclature;
			foundFixedPrice.Price = Price;
		}

		public override void Deactivate(Order order)
		{
		}

		public override bool IsValidForOrder(Order order, INomenclatureSettings nomenclatureSettings)
		{
			if(!IsForZeroDebt)
				return true;

			IBottlesRepository bottlesRepository = ScopeProvider.Scope.Resolve<IBottlesRepository>();

			BottlesMovementOperation bottlesMovementAlias = null;
			Order orderAlias = null;

			//Долг клиента
			var counterpartyDebtQuery = order.UoW.Session.QueryOver<BottlesMovementOperation>(() => bottlesMovementAlias)
				.Where(() => bottlesMovementAlias.DeliveryPoint == null)
				.Where(() => bottlesMovementAlias.Counterparty.Id == order.Client.Id)
				.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered)}
				)).SingleOrDefault<int>();
			if(counterpartyDebtQuery != 0)
				return false;

			//Долг по точкам доставки
			foreach(var deliveryPoint in order.Client.DeliveryPoints) {
				if(bottlesRepository.GetBottlesDebtAtDeliveryPoint(order.UoW, deliveryPoint) != 0)
				{
					return false;
				}
			}

			//Возврат бутылей и(ничего или возврат залога или неустойка)
			var orders1 = order.UoW.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.Client.Id == order.Client.Id)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.Where(() => orderAlias.BottlesReturn != 0)
				.List<Order>();
			if(orders1.Count == 0)
				return false;

			var orders2 = new List<Order>();

			foreach(var o in orders1) {
				if(o.OrderDepositItems != null && o.OrderItems == null)
					orders2.Add(o);
				if(o.OrderItems.All(i => i.Nomenclature.Id == nomenclatureSettings.ForfeitId))
					orders2.Add(o);
				if(o.OrderItems == null)
					orders2.Add(o);
			}

			if(orders2.Count == 0)
				return false;

			//Ввод остатков
			foreach(var o in orders2) {
				if(o.DeliveryPoint == null)
					continue;
				if(o.DeliveryPoint.HaveResidue.HasValue)
					if(!o.DeliveryPoint.HaveResidue.Value)
						return false;
			}

			return true;
		}
	}
}
