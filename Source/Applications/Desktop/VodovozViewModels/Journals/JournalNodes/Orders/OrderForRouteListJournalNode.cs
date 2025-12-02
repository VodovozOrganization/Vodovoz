using System;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
  public class OrderForRouteListJournalNode<TEntity> : OrderForRouteListJournalNode
		where TEntity : class, IDomainObject
	{
		public OrderForRouteListJournalNode() : base(typeof(TEntity)) { }
	}

	public class OrderForRouteListJournalNode : JournalEntityNodeBase
	{
		public OrderForRouteListJournalNode(Type entityType) : base(entityType)
		{
			if (entityType != typeof(Order))
			{
				StatusEnum = OrderStatus.Closed;
			}

			if(entityType == typeof(Order))
			{
				ViewType = "Заказ";
			}

			if(entityType == typeof(OrderWithoutShipmentForDebt))
			{
				ViewType = "Счет на долг";
			}

			if(entityType == typeof(OrderWithoutShipmentForPayment))
			{
				ViewType = "Счет на постоплату";
			}

			if(entityType == typeof(OrderWithoutShipmentForAdvancePayment))
			{
				ViewType = "Счет на предоплату";
			}
		}

		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";

		public OrderStatus StatusEnum { get; set; }
		
		public string ViewType { get; set; }

		public DateTime CreateDate { get; set; }
		public bool IsSelfDelivery { get; set; }
		public string DeliveryTime { get; set; }
		public TimeSpan? WaitUntilTime { get; set; }
		public decimal BottleAmount { get; set; }
		public decimal SanitisationAmount { get; set; }

		public string Counterparty { get; set; }

		public decimal Sum { get; set; }

		public string DistrictName { get; set; }
		public string City { get; set; }
		public string Street { get; set; }
		public string Building { get; set; }

		public string Address1c { get; set; }

		public string CompilledAddress { get; set; }
		public string Address => IsSelfDelivery ? "Самовывоз" : CompilledAddress;

		public string AuthorLastName { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }

		public string Author => PersonHelper.PersonNameWithInitials(AuthorLastName, AuthorName, AuthorPatronymic);
	}
}
