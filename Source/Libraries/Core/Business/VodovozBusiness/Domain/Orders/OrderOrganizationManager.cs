using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Settings;

namespace VodovozBusiness.Domain.Orders
{
	public class OrderOrganizationManager
	{
		private readonly OrderOurOrganizationForOrderHandler _orderOurOrganization;
		private readonly OrganizationFromClientForOrderHandler _organizationFromClient;
		private readonly ContractOrganizationForOrderHandler _contractOrganization;
		private readonly OrganizationByOrderContentForOrderHandler _organizationByOrderContentForOrderHandler;
		private readonly OrganizationByPaymentTypeForOrderHandler _organizationByPaymentTypeForOrderHandler;

		private readonly Queue<IGetOrganizationForOrder> _handlers = new Queue<IGetOrganizationForOrder>();

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
			AddHandler(_orderOurOrganization);
			AddHandler(_organizationFromClient);
			AddHandler(_contractOrganization);
			AddHandler(_organizationByOrderContentForOrderHandler);
			AddHandler(_organizationByPaymentTypeForOrderHandler);
		}
		
		public IReadOnlyDictionary<Organization, IEnumerable<OrderItem>> GetOrganizationForOrder(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
		{
			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}
			
			IReadOnlyDictionary<Organization, IEnumerable<OrderItem>> organizations = null;
			
			while(organizations is null)
			{
				if(_handlers.Any())
				{
					organizations =
						_handlers
							.Dequeue()
							.GetOrganizationsForOrder(order, uow, paymentType);
				}
				else
				{
					break;
				}
			}

			return organizations;
		}

		public void AddHandler(IGetOrganizationForOrder handler)
		{
			_handlers.Enqueue(handler);
		}
	}

	public interface IGetOrganizationForOrder
	{
		IReadOnlyDictionary<Organization, IEnumerable<OrderItem>> GetOrganizationsForOrder(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null);
	}
	
	/*public interface IManager : IGetOrganizationForOrder
	{
		void AddHandler(IGetOrganizationForOrder handler);
	}*/

	public class OrganizationByOrderContentForOrderHandler : IGetOrganizationForOrder
	{
		private readonly OrganizationByPaymentTypeForOrderHandler _organizationByPaymentTypeForOrderHandler;
		
		public OrganizationByOrderContentForOrderHandler(
			OrganizationByPaymentTypeForOrderHandler organizationByPaymentTypeForOrderHandler)
		{
			_organizationByPaymentTypeForOrderHandler =
				organizationByPaymentTypeForOrderHandler
				?? throw new ArgumentNullException(nameof(organizationByPaymentTypeForOrderHandler));
		}
		
		public IReadOnlyDictionary<Organization, IEnumerable<OrderItem>> GetOrganizationsForOrder(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
		{
			var result = new Dictionary<Organization, IEnumerable<OrderItem>>();
			
			var sets = uow.GetAll<OrganizationBasedOrderContentSettings>().ToArray();
			var subdivisionsForSets = uow.GetAll<OrganizationByOrderAuthorSettings>().ToArray();

			// может стоит убрать, т.к. ниже должно отработать правильно при таком раскладе
			if(!sets.Any())
			{
				//подбираем по форме оплаты
			}

			var processingOrderItems = order.OrderItems.ToList();
			
			/*	1 - заполнено первое множество
				1.1 - с организацией
				1.2 - без организации
				2 - заполнены оба множества
				2.1 - оба без организаций
				2.2 - первое с, второе без
				2.3 - первое без второе с
				2.4 - оба с организациями
			 */
			
			foreach(var set in sets)
			{
				var i = 0;
				var orderItemsForOrganization = new List<OrderItem>();

				while(i < processingOrderItems.Count)
				{
					if(OrderItemBelongsSet(processingOrderItems[i], set))
					{
						orderItemsForOrganization.Add(processingOrderItems[i]);
						processingOrderItems.RemoveAt(i);
					}

					i++;
				}

				if(!orderItemsForOrganization.Any())
				{
					continue;
				}

				var org =
					set.Organization ?? _organizationByPaymentTypeForOrderHandler.GetOrganizationForOrder(order, uow, paymentType);
				
				result.Add(org, orderItemsForOrganization);
			}

			if(!processingOrderItems.Any())
			{
				if(!result.Any())
				{
					var org = _organizationByPaymentTypeForOrderHandler.GetOrganizationForOrder(order, uow, paymentType);
					result.Add(org, null);
					return result;
				}

				return result;
			}
			
			//обработчик по автору
			
			var orgByPaymentType = _organizationByPaymentTypeForOrderHandler.GetOrganizationForOrder(order, uow, paymentType);
			if(result.TryGetValue(orgByPaymentType, out var items))
			{
				var list = items as List<OrderItem>;
				list.AddRange(processingOrderItems);
			}
			else
			{
				result.Add(orgByPaymentType, processingOrderItems);
			}

			return result;
		}
		
		private bool OrderItemBelongsSet(OrderItem orderItem, OrganizationBasedOrderContentSettings organizationBasedOrderContentSettings)
		{
			if(organizationBasedOrderContentSettings.Nomenclatures.Contains(orderItem.Nomenclature))
			{
				return true;
			}

			if(ContainsProductGroup(orderItem.Nomenclature.ProductGroup, organizationBasedOrderContentSettings.ProductGroups))
			{
				return true;
			}
			
			return false;
		}

		private bool ContainsProductGroup(ProductGroup itemProductGroup, IEnumerable<ProductGroup> productGroups) =>
			itemProductGroup != null
			&& productGroups.Any(discountProductGroup => ContainsProductGroup(itemProductGroup, discountProductGroup));
		
		private bool ContainsProductGroup(ProductGroup itemProductGroup, ProductGroup productGroup)
		{
			while(true)
			{
				if(itemProductGroup == productGroup)
				{
					return true;
				}

				if(itemProductGroup.Parent != null)
				{
					itemProductGroup = itemProductGroup.Parent;
					continue;
				}

				return false;
			}
		}
	}
	
	public class OrganizationByPaymentTypeForOrderHandler : IGetOrganizationForOrder
	{
		private readonly OrganizationForDeliveryOrderByPaymentTypeHandler _organizationForDeliveryOrderByPaymentTypeHandler;
		private readonly OrganizationForSelfDeliveryOrderByPaymentTypeHandler _organizationForSelfDeliveryOrderByPaymentTypeHandler;

		public OrganizationByPaymentTypeForOrderHandler(
			OrganizationForDeliveryOrderByPaymentTypeHandler organizationForDeliveryOrderByPaymentTypeHandler,
			OrganizationForSelfDeliveryOrderByPaymentTypeHandler organizationForSelfDeliveryOrderByPaymentTypeHandler)
		{
			_organizationForDeliveryOrderByPaymentTypeHandler =
				organizationForDeliveryOrderByPaymentTypeHandler
				?? throw new ArgumentNullException(nameof(organizationForDeliveryOrderByPaymentTypeHandler));
			_organizationForSelfDeliveryOrderByPaymentTypeHandler =
				organizationForSelfDeliveryOrderByPaymentTypeHandler
				?? throw new ArgumentNullException(nameof(organizationForSelfDeliveryOrderByPaymentTypeHandler));
		}
		
		public IReadOnlyDictionary<Organization, IEnumerable<OrderItem>> GetOrganizationsForOrder(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
		{
			return new Dictionary<Organization, IEnumerable<OrderItem>>
			{
				{ GetOrganizationForOrder(order, uow, paymentType), null }
			};
		}
		
		public Organization GetOrganizationForOrder(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
		{
			if(order.SelfDelivery || order.DeliveryPoint is null)
			{
				return _organizationForSelfDeliveryOrderByPaymentTypeHandler.GetOrganizationForOrder(order, uow, paymentType);
			}

			return _organizationForDeliveryOrderByPaymentTypeHandler.GetOrganizationForOrder(order, uow, paymentType);
		}
	}

	public class OrganizationForOrderWithOrderItems
	{
		public OrganizationForOrderWithOrderItems(
			Organization organization,
			IEnumerable<OrderItem> orderItems = null)
		{
			Organization = organization;
			OrderItems = orderItems;
		}
		
		public Organization Organization { get; }
		public IEnumerable<OrderItem> OrderItems { get; }
	}
}
