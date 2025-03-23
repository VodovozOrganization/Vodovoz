using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

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
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public OrderOrganizationManager(
			OrderOurOrganizationForOrderHandler orderOurOrganization,
			OrganizationFromClientForOrderHandler organizationFromClient,
			ContractOrganizationForOrderHandler contractOrganization,
			OrganizationByOrderContentForOrderHandler organizationByOrderContentForOrderHandler,
			OrganizationByPaymentTypeForOrderHandler organizationByPaymentTypeForOrderHandler,
			IUnitOfWorkFactory unitOfWorkFactory
			)
		{
			_orderOurOrganization = orderOurOrganization ?? throw new ArgumentNullException(nameof(orderOurOrganization));
			_organizationFromClient = organizationFromClient ?? throw new ArgumentNullException(nameof(organizationFromClient));
			_contractOrganization = contractOrganization ?? throw new ArgumentNullException(nameof(contractOrganization));
			_organizationByOrderContentForOrderHandler =
				organizationByOrderContentForOrderHandler ?? throw new ArgumentNullException(nameof(organizationByOrderContentForOrderHandler));
			_organizationByPaymentTypeForOrderHandler =
				organizationByPaymentTypeForOrderHandler ?? throw new ArgumentNullException(nameof(organizationByPaymentTypeForOrderHandler));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			Initialize();
		}

		private void Initialize()
		{
			_orderOurOrganization.SetNextHandler(_organizationFromClient);
			_organizationFromClient.SetNextHandler(_contractOrganization);
			_contractOrganization.SetNextHandler(_organizationByOrderContentForOrderHandler);
			_organizationByOrderContentForOrderHandler.SetNextHandler(_organizationByPaymentTypeForOrderHandler);
		}

		public PartitionedOrderByOrganizations GetOrderPartsByOrganizations(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			var orderParts = GetOrganizationsWithOrderItems(uow, requestTime, organizationChoice);
			var canSplitOrderWithDeposits = true;

			if(organizationChoice.OrderDepositItems.Any())
			{
				var depositsSum = organizationChoice.OrderDepositItems.Sum(x => x.ActualSum);
				
				foreach(var orderPart in orderParts)
				{
					if(depositsSum > orderPart.GoodsSum)
					{
						canSplitOrderWithDeposits = false;
					}
					else
					{
						canSplitOrderWithDeposits = true;
						orderPart.OrderDepositItems = organizationChoice.OrderDepositItems;
						break;
					}
				}
			}

			return new PartitionedOrderByOrganizations
			{
				CanSplitOrderWithDeposits = canSplitOrderWithDeposits,
				OrderParts = orderParts
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="organizationChoice">Обрабатываемые данные</param>
		/// <param name="uow">unit Of Work</param>
		/// <returns>Список организаций с товарами</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			if(organizationChoice is null)
			{
				throw new ArgumentNullException(nameof(organizationChoice));
			}

			return _orderOurOrganization.GetOrganizationsWithOrderItems(uow, requestTime, organizationChoice);
		}

		public bool OrderHasGoodsFromSeveralOrganizations(
			TimeSpan requestTime, IList<int> nomenclatureIds, bool isSelfDelivery, PaymentType paymentType)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				return _organizationByOrderContentForOrderHandler.OrderHasGoodsFromSeveralOrganizations(
					uow, requestTime, nomenclatureIds, isSelfDelivery, paymentType);
			}
		}
	}
}
