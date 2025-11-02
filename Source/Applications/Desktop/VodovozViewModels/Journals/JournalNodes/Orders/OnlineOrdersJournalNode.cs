using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Project.Journal;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class OnlineOrdersJournalNode : JournalEntityNodeBase
	{
		public override string Title => string.Empty;
		public string CounterpartyName { get; set; }
		public string EntityTypeString { get; set; }
		public string CompiledAddress { get; set; }
		public DateTime? DeliveryDate { get; set; }
		public DateTime CreationDate { get; set; }
		public bool IsSelfDelivery { get; set; }
		public bool IsFastDelivery { get; set; }
		public string DeliveryTime { get; set; }
		public OnlineOrderStatus? OnlineOrderStatus { get; set; }
		public RequestForCallStatus? RequestForCallStatus { get; set; }
		public int OrderByStatusValue { get; set; }

		public string Status
		{
			get
			{
				if(EntityType == typeof(OnlineOrder))
				{
					return OnlineOrderStatus.Value.GetEnumDisplayName();
				}

				if(EntityType == typeof(RequestForCall))
				{
					return RequestForCallStatus.Value.GetEnumDisplayName();
				}
				
				return string.Empty;
			}
		}

		public string ManagerWorkWith { get; set; }
		public Core.Domain.Clients.Source Source { get; set; }
		public decimal? OnlineOrderSum { get; set; }
		public OnlineOrderPaymentStatus? OnlineOrderPaymentStatus { get; set; }
		public int? OnlinePayment { get; set; }
		public OnlineOrderPaymentType OnlineOrderPaymentType { get; set; }
		public bool IsNeedConfirmationByCall { get; set; }
		public string OrdersIds { get; set; }
	}
}
