using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class UndeliveredOrderJournalNode : JournalEntityNodeBase<UndeliveredOrder>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public int NumberInList { get; set; }
		public string Address { get; set; }
		public string Client { get; set; }
		public string Reason { get; set; }
		public string Guilty { get; set; }
		public string OldDeliverySchedule { get; set; }

		public string ClientAndAddress => String.Format("{0}\n{1}", Client, Address);
		public string DriverName => OldRouteListDriverName ?? "Заказ\nне в МЛ";
		public string OldOrderDeliveryDate => OldOrderDeliveryDateTime.ToString("d MMM");
		public string DispatcherCall => DispatcherCallTime.HasValue ? DispatcherCallTime.Value.ToString("HH:mm") : "Не\nзвонили";
		public string ActionWithInvoice => NewOrderId > 0 ? NewOrderId.ToString() : "Новый заказ\nне создан";
		public string Registrator => PersonHelper.PersonNameWithInitials(RegistratorLastName, RegistratorFirstName, RegistratorMiddleName);
		public string UndeliveryAuthor => PersonHelper.PersonNameWithInitials(AuthorLastName, AuthorFirstName, AuthorMiddleName);
		public string Status => UndeliveryStatus.GetEnumTitle();
		public string FinedPeople => Fined ?? "Не выставлено";
		public string OldOrderStatus => String.Format("{0}\n\t↓\n{1}", StatusOnOldOrderCancel.GetEnumTitle(), OldOrderCurStatus.GetEnumTitle());

		public string OldOrderAuthor =>
			PersonHelper.PersonNameWithInitials(OldOrderAuthorLastName, OldOrderAuthorFirstName, OldOrderAuthorMiddleName);

		public string UndeliveredOrderItems
		{
			get
			{
				if(OldOrder19LBottleQty > 0)
				{
					return $"{OldOrder19LBottleQty:N0}";
				}

				if(OldOrderGoodsToClient != null)
				{
					return "к клиенту:\n" + OldOrderGoodsToClient;
				}

				if(OldOrderGoodsFromClient != null)
				{
					return "от клиента:\n" + OldOrderGoodsFromClient;
				}

				return "Другие\nтовары";
			}
		}

		public string DriversCall
		{
			get
			{
				if(OldRouteListDriverName == null)
				{
					return "Заказ\nне в МЛ";
				}

				string time = DriverCallType != DriverCallType.NoCall ? DriverCallTime.ToString("HH:mm\n") : "";
				return String.Format("{0}{1}", time, DriverCallType.GetEnumTitle());
			}
		}

		public string TransferDateTime =>
			NewOrderId > 0 ? NewOrderDeliveryDate?.ToString("d MMM\n") + NewOrderDeliverySchedule + "\n№" + NewOrderId.ToString() : "Новый заказ\nне создан";

		public DateTime? DispatcherCallTime { get; set; }
		public DateTime DriverCallTime { get; set; }
		public DriverCallType DriverCallType { get; set; }
		public int DriverCallNr { get; set; }
		public string AuthorLastName { get; set; }
		public string AuthorFirstName { get; set; }
		public string AuthorMiddleName { get; set; }
		public string EditorLastName { get; set; }
		public string EditorFirstName { get; set; }
		public string EditorMiddleName { get; set; }
		public string RegistratorLastName { get; set; }
		public string RegistratorFirstName { get; set; }
		public string RegistratorMiddleName { get; set; }
		public UndeliveryStatus UndeliveryStatus { get; set; }
		public GuiltyTypes GuiltySide { get; set; }
		public string GuiltyDepartment { get; set; }
		public string Fined { get; set; }
		public OrderStatus StatusOnOldOrderCancel { get; set; }
		public string InProcessAt { get; set; }

		//старый заказ
		public int OldOrderId { get; set; }
		public DateTime OldOrderDeliveryDateTime { get; set; }
		public string OldOrderAuthorLastName { get; set; }
		public string OldOrderAuthorFirstName { get; set; }
		public string OldOrderAuthorMiddleName { get; set; }
		public decimal OldOrder19LBottleQty { get; set; }
		public string OldOrderGoodsToClient { get; set; }
		public string OldOrderGoodsFromClient { get; set; }
		public string OldRouteListDriverName { get; set; }
		public OrderStatus OldOrderCurStatus { get; set; }

		//новый заказ
		public int NewOrderId { get; set; }
		public DateTime? NewOrderDeliveryDate { get; set; }
		public string NewOrderDeliverySchedule { get; set; }
	}
}

