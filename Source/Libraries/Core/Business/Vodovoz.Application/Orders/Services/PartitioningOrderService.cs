using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.Models.Orders;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class PartitioningOrderService : IPartitioningOrderService
	{
		private readonly ILogger<IPartitioningOrderService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IFlyerRepository _flyerRepository;
		private readonly IOrderContractUpdater _orderContractUpdater;
		private readonly IOrderConfirmationService _orderConfirmationService;

		public PartitioningOrderService(
			ILogger<IPartitioningOrderService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			INomenclatureSettings nomenclatureSettings,
			IFlyerRepository flyerRepository,
			IOrderContractUpdater orderContractUpdater,
			IOrderConfirmationService orderConfirmationService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
			_orderConfirmationService = orderConfirmationService ?? throw new ArgumentNullException(nameof(orderConfirmationService));
		}
		
		public Result<IEnumerable<int>> CreatePartOrdersAndSave(
			int baseOrderId,
			Employee employee,
			PartitionedOrderByOrganizations partitionedOrderByOrganizations)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Разделение заказа на подзаказы по организациям"))
			{
				var i = 0;
				var orders = new List<Order>();

				foreach(var partOrderWithGoods in partitionedOrderByOrganizations.OrderParts)
				{
					var resultOrder = i == 0 ? uow.GetById<Order>(baseOrderId) : new Order();
					resultOrder.UoW = uow;
					orders.Add(resultOrder);

					var partitionedOrder = new PartitionedOrder(
						uow,
						uow.GetById<Order>(baseOrderId),
						resultOrder,
						_nomenclatureSettings,
						_flyerRepository,
						_orderContractUpdater);

					if(i == 0)
					{
						partitionedOrder.ClearGoodsAndEquipmentsAndDeposits();
					}
					else
					{
						partitionedOrder.CopyFields();
					}

					partitionedOrder
						.CopyPromotionalSets(partOrderWithGoods.Goods)
						.CopyOrderItems(partOrderWithGoods.Goods, true)
						.CopyOrderEquipments(partOrderWithGoods.OrderEquipments)
						.CopyOrderDepositItems(partOrderWithGoods.OrderDepositItems)
						.CopyAttachedDocuments();

					resultOrder.UpdateDocuments();
					_orderContractUpdater.ForceUpdateContract(uow, resultOrder, partOrderWithGoods.Organization);
					
					_orderConfirmationService.AcceptOrder(uow, employee, resultOrder, false);

					i++;
				}

				var orderIds = orders.Select(x => x.Id).ToList();
				var parts = GenerateOrderPartsString(orderIds);

				foreach(var order in orders)
				{
					if(order.GetTotalWater19LCount() == 0)
					{
						order.BottlesReturn = null;
						order.ReturnedTare = null;
					}
					
					order.Comment = $"Заказ был разбит на части. Номера созданных заказов: {parts}\n" + order.Comment;
					order.OrderPartsIds = parts;
				}

				uow.Commit();
				return Result.Success<IEnumerable<int>>(orderIds);
			}
		}

		private string GenerateOrderPartsString(IEnumerable<int> orderIds)
		{
			var sb = new StringBuilder();

			foreach(var id in orderIds)
			{
				sb.Append(id);
				sb.Append(',');
			}
			
			return sb.ToString().TrimEnd(',');
		}
	}
}
