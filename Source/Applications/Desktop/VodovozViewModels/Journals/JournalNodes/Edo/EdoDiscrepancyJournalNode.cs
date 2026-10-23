using Gamma.Utilities;
using QS.Project.Journal;
using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Edo
{
	public class EdoDiscrepancyJournalNode : JournalNodeBase
	{
		private DateTime _deliveryDate;
		private OrderStatus _orderStatus;
		private EdoTaskStatus _edoTaskStatus;
		private EdoDocumentStatus _updInErpStatus;
		private EdoDocFlowStatus _updInTaxcomStatus;


		public int OrderId { get; set; }

		public string DeliveryDateTitle { get; set; }
		public DateTime DeliveryDate
		{
			get => _deliveryDate;
			set
			{
				_deliveryDate = value;
				DeliveryDateTitle = value.ToString("dd.MM.yyyy HH:mm");
			}
		}

		public string OrderStatusTitle { get; set; }
		public OrderStatus OrderStatus
		{
			get => _orderStatus;
			set
			{
				_orderStatus = value;
				OrderStatusTitle = value.GetEnumTitle();
			}
		}

		public string DocumentNumber { get; set; }
		public int TaskId { get; set; }
		public int DocumentId { get; set; }

		public string EdoTaskStatusTitle { get; set; }
		public EdoTaskStatus EdoTaskStatus
		{
			get => _edoTaskStatus;
			set
			{
				_edoTaskStatus = value;
				EdoTaskStatusTitle = value.GetEnumTitle();
			}
		}

		public string UpdInErpStatusTitle { get; set; }
		public EdoDocumentStatus UpdInErpStatus
		{
			get => _updInErpStatus;
			set
			{
				_updInErpStatus = value;
				UpdInErpStatusTitle = value.GetEnumTitle();
			}
		}

		public string UpdInTaxcomStatusTitle { get; set; }
		public EdoDocFlowStatus UpdInTaxcomStatus
		{
			get => _updInTaxcomStatus;
			set
			{
				_updInTaxcomStatus = value;
				UpdInTaxcomStatusTitle = value.GetEnumTitle();
			}
		}
	}
}
