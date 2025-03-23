using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Основной обработчик для подбора организаций для заказа
	/// </summary>
	public class OrderOrganizationManager : IOrderOrganizationManager
	{
		private readonly OrderOurOrganizationForOrderHandler _orderOurOrganization;
		private readonly OrganizationFromClientForOrderHandler _organizationFromClient;
		private readonly ContractOrganizationForOrderHandler _contractOrganization;
		private readonly OrganizationByOrderContentForOrderHandler _organizationByOrderContentForOrderHandler;
		private readonly OrganizationByPaymentTypeForOrderHandler _organizationByPaymentTypeForOrderHandler;

		public OrderOrganizationManager(
			OrderOurOrganizationForOrderHandler orderOurOrganization,
			OrganizationFromClientForOrderHandler organizationFromClient,
			ContractOrganizationForOrderHandler contractOrganization,
			OrganizationByOrderContentForOrderHandler organizationByOrderContentForOrderHandler,
			OrganizationByPaymentTypeForOrderHandler organizationByPaymentTypeForOrderHandler
			)
		{
			_orderOurOrganization = orderOurOrganization ?? throw new ArgumentNullException(nameof(orderOurOrganization));
			_organizationFromClient = organizationFromClient ?? throw new ArgumentNullException(nameof(organizationFromClient));
			_contractOrganization = contractOrganization ?? throw new ArgumentNullException(nameof(contractOrganization));
			_organizationByOrderContentForOrderHandler =
				organizationByOrderContentForOrderHandler ?? throw new ArgumentNullException(nameof(organizationByOrderContentForOrderHandler));
			_organizationByPaymentTypeForOrderHandler =
				organizationByPaymentTypeForOrderHandler ?? throw new ArgumentNullException(nameof(organizationByPaymentTypeForOrderHandler));

			Initialize();
		}

		private void Initialize()
		{
			_orderOurOrganization.SetNextHandler(_organizationFromClient);
			_organizationFromClient.SetNextHandler(_contractOrganization);
			_contractOrganization.SetNextHandler(_organizationByOrderContentForOrderHandler);
			_organizationByOrderContentForOrderHandler.SetNextHandler(_organizationByPaymentTypeForOrderHandler);
		}

		public OrderForOrderWithGoodsEquipmentsAndDeposits GetOrderPartsByOrganizations(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow = null)
		{
			var orderParts = GetOrganizationsWithOrderItems(requestTime, order, uow);
			var canSplitOrderWithDeposits = true;

			if(order.OrderDepositItems.Any())
			{
				foreach(var orderPart in orderParts)
				{
					if(order.DepositsSum > orderPart.GoodsSum)
					{
						canSplitOrderWithDeposits = false;
					}
					else
					{
						canSplitOrderWithDeposits = true;
						orderPart.OrderDepositItems = order.OrderDepositItems;
						break;
					}
				}
			}

			return new OrderForOrderWithGoodsEquipmentsAndDeposits
			{
				CanSplitOrderWithDeposits = canSplitOrderWithDeposits,
				OrderParts = orderParts
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="order">Обрабатываемый заказ</param>
		/// <param name="uow">unit Of Work</param>
		/// <returns>Список организаций с товарами</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow = null)
		{
			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			return _orderOurOrganization.GetOrganizationsWithOrderItems(requestTime, order, uow);
		}
	}
}
