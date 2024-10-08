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

			var phonesHavingPromotionalSetsByDeliveryPointsForPeriod =
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

			var phonesHavingPromotionalSetsByClientForPeriod =
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

			var allPhonesHavingPromotionalSetsForPeriod =
				phonesHavingPromotionalSetsByDeliveryPointsForPeriod
				.Union(phonesHavingPromotionalSetsByClientForPeriod)
				.Distinct()
				.ToList();

			var ordersWithPhonesByDeliveryPoint =
				(from order in UoW.Session.Query<Order>()
				 join orderItem in UoW.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				 join phone in UoW.Session.Query<Phone>() on order.DeliveryPoint.Id equals phone.DeliveryPoint.Id
				 where
				 orderItem.PromoSet.Id != null
				 && !phone.IsArchive
				 && !notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 && allPhonesHavingPromotionalSetsForPeriod.Contains(phone.DigitsNumber)
				 select new OrderWithPhoneDataNode
				 {
					 OrderId = order.Id,
					 ClientId = order.Client.Id,
					 DeliveryPointId = order.DeliveryPoint.Id,
					 OrderCreateDate = order.CreateDate,
					 OrderDeliveryDate = order.DeliveryDate,
					 AuthorId = order.Author.Id,
					 PhoneNumber = phone.Number,
					 PhoneDigitNumber = phone.DigitsNumber
				 }).ToList();

			var ordersWithPhonesByClient =
				(from order in UoW.Session.Query<Order>()
				 join orderItem in UoW.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				 join phone in UoW.Session.Query<Phone>() on order.Client.Id equals phone.Counterparty.Id
				 where
				 orderItem.PromoSet.Id != null
				 && !phone.IsArchive
				 && !notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 && allPhonesHavingPromotionalSetsForPeriod.Contains(phone.DigitsNumber)
				 select new OrderWithPhoneDataNode
				 {
					 OrderId = order.Id,
					 ClientId = order.Client.Id,
					 DeliveryPointId = order.DeliveryPoint.Id,
					 OrderCreateDate = order.CreateDate,
					 OrderDeliveryDate = order.DeliveryDate,
					 AuthorId = order.Author.Id,
					 PhoneNumber = phone.Number,
					 PhoneDigitNumber = phone.DigitsNumber
				 }).ToList();

			var ordersWithPhones = new List<OrderWithPhoneDataNode>();

			ordersWithPhones.AddRange(ordersWithPhonesByDeliveryPoint);
			ordersWithPhones.AddRange(ordersWithPhonesByClient);
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
