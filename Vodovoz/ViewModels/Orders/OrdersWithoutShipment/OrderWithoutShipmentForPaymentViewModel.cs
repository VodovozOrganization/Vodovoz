using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using QS.Commands;
using Vodovoz.Domain.Orders;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Project.Journal;
using QS.Tdi;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Employees;
using VodOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForPaymentViewModel : EntityTabViewModelBase<OrderWithoutShipmentForPayment>, ITdiTabAddedNotifier
	{
		private GenericObservableList<OrderWithoutShipmentForPaymentNode> OrdersForPayment { get; set; }

		private DateTime? startDate /*= DateTime.Now.AddMonths(-1)*/;
		public DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime? endDate/* = DateTime.Now*/;
		public DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		public DelegateCommand SendEmailCommand { get; private set; }
		public Action<string> OpenCounterpatyJournal;
		public IEntityUoWBuilder EntityUoWBuilder { get; }
		
		public OrderWithoutShipmentForPaymentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices) : base(uowBuilder, uowFactory, commonServices)
		{
			TabName = "Счет без отгрузки на постоплату";
			
			EntityUoWBuilder = uowBuilder;

			if (uowBuilder.IsNewEntity)
			{
				Entity.Author = EmployeeSingletonRepository.GetInstance().GetEmployeeForCurrentUser(UoW);
			}

			CreateCommands();
		}

		private void CreateCommands()
		{
			CreateSendEmailCommand();
		}

		private void CreateSendEmailCommand()
		{
			SendEmailCommand = new DelegateCommand(
				() => Close(false, QS.Navigation.CloseSource.Cancel),
				() => true
			);
		}
		
		public IList<OrderWithoutShipmentForPaymentNode> UpdateNodes()
		{
			if (Entity.Client == null)
				return null;
			
			OrderWithoutShipmentForPaymentNode resultAlias = null;
			VodOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var cashlessOrdersQuery = UoW.Session.QueryOver(() => orderAlias)
					.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
					.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
					.Where(x => x.OrderStatus == OrderStatus.Closed)
					.Where(x => x.PaymentType == PaymentType.cashless);

			if(Entity.Client != null)
				cashlessOrdersQuery.Where(x => x.Client == Entity.Client);

			if(StartDate.HasValue && EndDate.HasValue)
				cashlessOrdersQuery.Where(x => x.CreateDate >= StartDate && x.CreateDate <= EndDate);
			
			var bottleCountSubquery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));
			
			var totalSum = QueryOver.Of(() => orderItemAlias)
					.Where(x => x.Order.Id == orderAlias.Id)
					.Select(
						Projections.Sum(
							Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "(?1 * IFNULL(?2, ?3) - ?4)"),
							NHibernateUtil.Decimal, new IProjection[] {
							Projections.Property(() => orderItemAlias.Price),
							Projections.Property(() => orderItemAlias.ActualCount),
							Projections.Property(() => orderItemAlias.Count),
							Projections.Property(() => orderItemAlias.DiscountMoney)})
						)
					);

			/*incomePaymentQuery.Where(
					GetSearchCriterion(
					() => orderAlias.Id
				)
			);*/

			var resultQuery = cashlessOrdersQuery
					.SelectList(list => list
				   	//.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
				    .Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
				   	.Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.OrderStatus)
				   	.Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.OrderDate)
				    .Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.DeliveryAddress)
					.SelectSubQuery(totalSum).WithAlias(() => resultAlias.OrderSum)
				    .SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.Bottles)
					)
				.OrderBy(x => x.CreateDate).Desc
				.TransformUsing(Transformers.AliasToBean<OrderWithoutShipmentForPaymentNode>())
				.List<OrderWithoutShipmentForPaymentNode>();

			return resultQuery;
		}

		public void OnTabAdded()
		{
			if(EntityUoWBuilder.IsNewEntity)
				OpenCounterpatyJournal?.Invoke(string.Empty);
		}
	}

	public class OrderWithoutShipmentForPaymentNode : JournalEntityNodeBase<VodOrder>
	{
		public bool IsSelected { get; set; }
		public int OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public OrderStatus OrderStatus { get; set; }
		public int Bottles { get; set; }
		public decimal OrderSum { get; set; }
		public string DeliveryAddress { get; set; }
	}
}
