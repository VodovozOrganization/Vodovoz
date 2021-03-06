﻿using System;
using System.Collections.Generic;
using QS.Project.Journal;
using QS.Utilities.Text;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Journals.JournalNodes
{
	public class OrderForMovDocJournalNode : JournalEntityNodeBase<Order>
	{
		public OrderStatus StatusEnum { get; set; }

		public DateTime Date { get; set; }
		public DateTime CreateDate { get; set; }
		public bool IsSelfDelivery { get; set; }
		public decimal BottleAmount { get; set; }

		public string Counterparty { get; set; }

		public decimal Sum { get; set; }

		public string CompilledAddress { get; set; }
		public string Address => IsSelfDelivery ? "Самовывоз" : CompilledAddress;

		public int? OnlineOrder { get; set; }
		public string OnLineNumber => OnlineOrder?.ToString() ?? string.Empty;

		public int? EShopOrder { get; set; }
		public string EShopNumber => EShopOrder?.ToString() ?? string.Empty;

		public string RowColor {
			get {
				if(StatusEnum == OrderStatus.Canceled || StatusEnum == OrderStatus.DeliveryCanceled)
					return "grey";
				if(StatusEnum == OrderStatus.Closed)
					return "green";
				if(StatusEnum == OrderStatus.NotDelivered)
					return "blue";
				return "black";
			}
		}
	}
}
