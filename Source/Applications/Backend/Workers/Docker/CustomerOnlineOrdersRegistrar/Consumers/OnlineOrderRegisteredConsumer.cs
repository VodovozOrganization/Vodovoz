﻿using System;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.Factories;
using CustomerOrdersApi.Library.Dto.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Application.Orders.Services;

namespace CustomerOnlineOrdersRegistrar.Consumers
{
	public class OnlineOrderRegisteredConsumer : OnlineOrderConsumer, IConsumer<OnlineOrderInfoDto>
	{
		private readonly IBus _bus;

		public OnlineOrderRegisteredConsumer(
			ILogger<OnlineOrderRegisteredConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactory onlineOrderFactory,
			IOrderService orderService,
			IBus bus) : base(logger, unitOfWorkFactory, onlineOrderFactory, orderService)
		{
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}
		
		public Task Consume(ConsumeContext<OnlineOrderInfoDto> context)
		{
			var message = context.Message;
			Logger.LogInformation("Пришел онлайн заказ {ExternalOrderId}, регистрируем...", message.ExternalOrderId);
			
			try
			{
				TryRegisterOnlineOrder(message);
				return Task.CompletedTask;
			}
			catch(Exception e)
			{
				Logger.LogError(e, "Ошибка при регистрации онлайн заказа {ExternalOrderId}", message.ExternalOrderId);
				message.FaultedMessage = true;
				_bus.Publish<OnlineOrderInfoDto>(message);
				return Task.CompletedTask;
			}
		}
	}
}
