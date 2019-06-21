using System;
using QS.Project.Journal;
using QS.Utilities.Text;
using Vodovoz.Domain.Orders;

namespace Vodovoz.JournalNodes
{
	public class OrderJournalNode : JournalEntityNodeBase<Order>
	{
		public OrderStatus StatusEnum { get; set; }

		public DateTime Date { get; set; }
		public string DeliveryTime { get; set; }
		public int BottleAmount { get; set; }
		public int SanitisationAmount { get; set; }

		public string Counterparty { get; set; }

		public decimal Sum { get; set; }

		public string DistrictName { get; set; }
		public string City { get; set; }
		public string Street { get; set; }
		public string Building { get; set; }

		public string Address1c { get; set; }

		public string Address => String.Format("{0}, {1} д.{2}", City, Street, Building);

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

		public decimal? Latitude { get; set; }
		public decimal? Longitude { get; set; }

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
