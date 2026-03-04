using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Gamma.Utilities;
using Type = System.Type;

namespace Vodovoz.JournalNodes
{
	public class OrderJournalNode<TEntity> : OrderJournalNode
		where TEntity : class, IDomainObject
	{
		public OrderJournalNode() : base(typeof(TEntity)) { }
	}

	public class OrderJournalNode : JournalEntityNodeBase
	{
		public OrderJournalNode(Type entityType) : base(entityType)
		{
			if (entityType != typeof(Order))
				StatusEnum = OrderStatus.Closed;

			if(entityType == typeof(Order))
				ViewType = "Заказ";

			if(entityType == typeof(OrderWithoutShipmentForDebt))
				ViewType = "Счет на долг";

			if(entityType == typeof(OrderWithoutShipmentForPayment))
				ViewType = "Счет на постоплату";

			if(entityType == typeof(OrderWithoutShipmentForAdvancePayment))
				ViewType = "Счет на предоплату";
		}

		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";

		public OrderStatus StatusEnum { get; set; }
		
		public OrderPaymentStatus OrderPaymentStatus { get; set; }

		public string ViewType { get; set; }

		public DateTime? Date { get; set; }
		public DateTime CreateDate { get; set; }
		public bool IsSelfDelivery { get; set; }
		public string DeliveryTime { get; set; }
		public TimeSpan? WaitUntilTime { get; set; }		
		public decimal BottleAmount { get; set; }
		public decimal SanitisationAmount { get; set; }

		public string Counterparty { get; set; }
		public string Inn { get; set; }

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

		public string LastEditorLastName { get; set; }
		public string LastEditorName { get; set; }
		public string LastEditorPatronymic { get; set; }

		public string Author => PersonHelper.PersonNameWithInitials(AuthorLastName, AuthorName, AuthorPatronymic);

		public string LastEditor => PersonHelper.PersonNameWithInitials(LastEditorLastName, LastEditorName, LastEditorPatronymic);

		public DateTime LastEditedTime { get; set; }

		public int DriverCallId { get; set; }

		public int? OnlineOrder { get; set; }
		public string OnLineNumber => OnlineOrder?.ToString() ?? string.Empty;

		public int? EShopOrder { get; set; }
		public string EShopNumber => EShopOrder?.ToString() ?? string.Empty;

		public bool Sensitive { get; set; }

		public EdoDocFlowStatus? EdoDocFlowStatus { get; set; }
		public EdoDocumentStatus? NewEdoDocFlowStatus { get; set; }
		public string EdoDocFlowStatusString =>
			EdoDocFlowStatus is null
			? NewEdoDocFlowStatus is null
				? string.Empty
				: NewEdoDocFlowStatus.GetEnumTitle()
			: EdoDocFlowStatus.GetEnumTitle();
		
		public string UpdDocumentName { get; set; }
	}
}
