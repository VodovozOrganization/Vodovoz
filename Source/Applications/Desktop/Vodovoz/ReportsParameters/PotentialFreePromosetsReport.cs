using Gamma.ColumnConfig;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ReportsParameters
{
	[ToolboxItem(true)]
	public partial class PotentialFreePromosetsReport : SingleUoWWidgetBase, IParametersWidget
	{
		IEnumerable<PromosetReportNode> _promotionalSets;

		public PotentialFreePromosetsReport()
		{
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

			buttonCreateReport.Clicked += (sender, e) => OnUpdate(false);

			ytreeview1.ColumnsConfig = FluentColumnsConfig<PromosetReportNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(x => x.Active)
				.AddColumn("Промонабор").AddTextRenderer(x => x.Name)
				.Finish();

			_promotionalSets = (from ps in UoW.GetAll<PromotionalSet>()
								   select new PromosetReportNode
								   {
									   Id = ps.Id,
									   Name = ps.Name,
									   Active = ps.PromotionalSetForNewClients,
								   })
								   .ToList();

			ytreeview1.ItemsDataSource = _promotionalSets;
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по потенциальным халявщикам";

		private int[] GetSelectedPromotionalSets()
		{
			if(_promotionalSets.Any(x => x.Active))
			{
				return _promotionalSets.Where(x => x.Active).Select(x => x.Id).ToArray();
			}

			//если ни один промосет не выбран, необходимо выбрать все
			return _promotionalSets.Select(x => x.Id).ToArray();
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			parameters.Add("start_date", dateperiodpicker.StartDate);
			parameters.Add("end_date", dateperiodpicker.EndDate);
			parameters.Add("promosets", GetSelectedPromotionalSets());

			return new ReportInfo
			{
				Identifier = "Client.PotentialFreePromosets",
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		void OnUpdate(bool hide = false)
		{
			if(dateperiodpicker.StartDateOrNull == null || dateperiodpicker.EndDateOrNull == null)
			{
				MessageDialogHelper.RunWarningDialog("Необходимо ввести полный период");

				return;
			}

			MakeQueries();

			//LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private void MakeQueries()
		{
			var selectedPromosets = GetSelectedPromotionalSets();
			var notDeliveredOrderStatuses = new List<OrderStatus> { OrderStatus.Canceled, OrderStatus.NotDelivered, OrderStatus.DeliveryCanceled };

			//Все строки номеров телефонов в карочках точек доставки,
			//на которые оформлялись доставленные заказы с промонабором за период
			var deliveryPointPhoneDigitNumbersHavingPromotionalSetsForPeriod =
				(from order in UoW.Session.Query<Order>()
				 join orderItem in UoW.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				 join phone in UoW.Session.Query<Phone>() on order.DeliveryPoint.Id equals phone.DeliveryPoint.Id
				 where
				 order.CreateDate >= dateperiodpicker.StartDate.Date
				 && order.CreateDate < dateperiodpicker.EndDate.Date.AddDays(1)
				 && !phone.IsArchive
				 && selectedPromosets.Contains(orderItem.PromoSet.Id)
				 && !notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 && phone.DigitsNumber != null
				 select phone.DigitsNumber).Distinct().ToList();

			//Все строки номеров телефонов в карточках контаргентов,
			//которым оформлялись доставленные заказы с промонабором за период
			var clientPhoneDigitNumbersHavingPromotionalSetsForPeriod =
				(from order in UoW.Session.Query<Order>()
				 join orderItem in UoW.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				 join phone in UoW.Session.Query<Phone>() on order.Client.Id equals phone.Counterparty.Id
				 where
				 order.CreateDate >= dateperiodpicker.StartDate.Date
				 && order.CreateDate < dateperiodpicker.EndDate.Date.AddDays(1)
				 && !phone.IsArchive
				 && selectedPromosets.Contains(orderItem.PromoSet.Id)
				 && !notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 && phone.DigitsNumber != null
				 select phone.DigitsNumber).Distinct().ToList();

			//Все строки номеров телефонов в карточке клиента и карточке точек доставки,
			//на которые оформлялись доставленные заказы с промонабором за период
			var allDigitNumbersHavingPromotionalSetsForPeriod =
				deliveryPointPhoneDigitNumbersHavingPromotionalSetsForPeriod
				.Union(clientPhoneDigitNumbersHavingPromotionalSetsForPeriod)
				.Distinct()
				.ToList();

			var ordersWithPhonesHavingPromotionalSetsByDeliveryPointPhoneDigitNumbers =
				(from order in UoW.Session.Query<Order>()
				 join orderItem in UoW.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				 join phone in UoW.Session.Query<Phone>() on order.DeliveryPoint.Id equals phone.DeliveryPoint.Id
				 join dp in UoW.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals dp.Id into deliveryPoints
				 from deliveryPoint in deliveryPoints.DefaultIfEmpty()
				 join cl in UoW.Session.Query<Counterparty>() on order.Client.Id equals cl.Id into clients
				 from client in clients.DefaultIfEmpty()
				 join dpc in UoW.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
				 from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
				 where
				 orderItem.PromoSet.Id != null
				 && !phone.IsArchive
				 && !notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 && allDigitNumbersHavingPromotionalSetsForPeriod.Contains(phone.DigitsNumber)
				 select new OrderWithPhoneDataNode
				 {
					 OrderId = order.Id,
					 ClientId = order.Client.Id,
					 DeliveryPointId = order.DeliveryPoint.Id,
					 OrderCreateDate = order.CreateDate,
					 OrderDeliveryDate = order.DeliveryDate,
					 AuthorId = order.Author.Id,
					 PhoneNumber = phone.Number,
					 PhoneDigitNumber = phone.DigitsNumber,
					 ClientName = client.FullName,
					 DeliveryPointAddress = deliveryPoint.ShortAddress,
					 DeliveryPointCategory = deliveryPointCategory.Name
				 }).ToList();

			var ordersWithPhonesHavingPromotionalSetsByClientPhoneDigitNumbers =
				(from order in UoW.Session.Query<Order>()
				 join orderItem in UoW.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				 join phone in UoW.Session.Query<Phone>() on order.Client.Id equals phone.Counterparty.Id
				 join dp in UoW.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals dp.Id into deliveryPoints
				 from deliveryPoint in deliveryPoints.DefaultIfEmpty()
				 join cl in UoW.Session.Query<Counterparty>() on order.Client.Id equals cl.Id into clients
				 from client in clients.DefaultIfEmpty()
				 join dpc in UoW.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
				 from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
				 where
				 orderItem.PromoSet.Id != null
				 && !phone.IsArchive
				 && !notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 && allDigitNumbersHavingPromotionalSetsForPeriod.Contains(phone.DigitsNumber)
				 select new OrderWithPhoneDataNode
				 {
					 OrderId = order.Id,
					 ClientId = order.Client.Id,
					 DeliveryPointId = order.DeliveryPoint.Id,
					 OrderCreateDate = order.CreateDate,
					 OrderDeliveryDate = order.DeliveryDate,
					 AuthorId = order.Author.Id,
					 PhoneNumber = phone.Number,
					 PhoneDigitNumber = phone.DigitsNumber,
					 ClientName = client.FullName,
					 DeliveryPointAddress = deliveryPoint.ShortAddress,
					 DeliveryPointCategory = deliveryPointCategory.Name
				 }).ToList();

			var ordersWithPhonesHavingPromotionalSetsByAllDigitNumbers = new List<OrderWithPhoneDataNode>();
			ordersWithPhonesHavingPromotionalSetsByAllDigitNumbers.AddRange(ordersWithPhonesHavingPromotionalSetsByDeliveryPointPhoneDigitNumbers);
			ordersWithPhonesHavingPromotionalSetsByAllDigitNumbers.AddRange(ordersWithPhonesHavingPromotionalSetsByClientPhoneDigitNumbers);

			//var ordersHavignPromosetsAndSameDiditPhoneNumber =
			//	from order in UoW.Session.Query<Order>()
			//	join cp1 in UoW.Session.Query<Phone>()
			//	on new { ClientId = order.Client.Id, IsPhoneNotArchived = true } equals new { ClientId = cp1.Counterparty.Id, IsPhoneNotArchived = cp1.IsArchive }
			//	into clientPhones
			//	from clientPhone in clientPhones.DefaultIfEmpty()
			//	join cp2 in UoW.Session.Query<Phone>()
			//	on new { DeliveryPointId = order.DeliveryPoint.Id, IsPhoneNotArchived = true } equals new { DeliveryPointId = cp2.DeliveryPoint.Id, IsPhoneNotArchived = cp2.IsArchive }
			//	into deliveryPointPhones
			//	from deliveryPointPhone in deliveryPointPhones.DefaultIfEmpty()
			//	select order;
		}
	}

	public class OrderWithPhoneDataNode
	{
		public int OrderId { get; set; }
		public int ClientId { get; set; }
		public int DeliveryPointId { get; set; }
		public DateTime? OrderCreateDate { get; set; }
		public DateTime? OrderDeliveryDate { get; set; }
		public int AuthorId { get; set; }
		public string PhoneNumber { get; set; }
		public string PhoneDigitNumber { get; set; }
		public string ClientName { get; set; }
		public string DeliveryPointAddress { get; set; }
		public string DeliveryPointCategory { get; set; }
	}

	public class PromosetReportNode : PropertyChangedBase
	{
		private bool _active;
		public virtual bool Active
		{
			get => _active;
			set => SetField(ref _active, value);
		}

		public int Id { get; set; }

		public string Name { get; set; }
	}
}
