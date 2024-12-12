using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Commands;
using Pacs.Core.Messages.Events;
using Pacs.Server.Operators;
using System;
using System.Threading.Tasks; 

namespace Pacs.Operators.Server
{
	[ApiController]
	[Route("pacs/operator")]
	[Authorize]
	public class OperatorController : ControllerBase
	{
		private readonly ILogger<OperatorController> _logger;
		private readonly IOperatorStateService _operatorStateService;

		public OperatorController(
			ILogger<OperatorController> logger,
			IOperatorStateService operatorStateService)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_operatorStateService = operatorStateService
				?? throw new ArgumentNullException(nameof(operatorStateService));
		}

		[HttpPost]
		[Route("connect")]
		public async Task<OperatorResult> Connect([FromBody] Connect command)
		{
			_logger.LogTrace("Подключение оператора {OperatorId}", command.OperatorId);

			return await _operatorStateService.Connect(command.OperatorId);
		}

		[HttpPost]
		[Route("disconnect")]
		public async Task<OperatorResult> Disconnect([FromBody] Disconnect command)
		{
			_logger.LogTrace("Отключение оператора {OperatorId}", command.OperatorId);

			return await _operatorStateService.Disconnect(command.OperatorId);
		}

		[HttpPost]
		[Route("keep_alive")]
		public async Task KeepAlive([FromBody] KeepAlive command)
		{
			_logger.LogTrace("Поддержание подключения оператора {OperatorId}", command.OperatorId);

			await _operatorStateService.KeepAlive(command.OperatorId);
		}

		[HttpPost]
		[Route("startworkshift")]
		public async Task<OperatorResult> StartWorkShift([FromBody] StartWorkShift command)
		{
			_logger.LogTrace("Начало смены оператора {OperatorId}", command.OperatorId);

			return await _operatorStateService.StartWorkShift(command.OperatorId, command.PhoneNumber);
		}

		[HttpPost]
		[Route("endworkshift")]
		public async Task<OperatorResult> EndWorkShift([FromBody] EndWorkShift command)
		{
			_logger.LogTrace("Завершение смены оператора {OperatorId}", command.OperatorId);

			return await _operatorStateService.EndWorkShift(command.OperatorId, command.Reason);
		}

		[HttpPost]
		[Route("changephone")]
		public async Task<OperatorResult> ChangePhone([FromBody] ChangePhone command)
		{
			_logger.LogTrace("Смена телефона оператора {OperatorId} на {Phone}", command.OperatorId, command.PhoneNumber);

			return await _operatorStateService.ChangePhone(command.OperatorId, command.PhoneNumber);
		}

		[HttpPost]
		[Route("startbreak")]
		public async Task<OperatorResult> StartBreak([FromBody] StartBreak command)
		{
			_logger.LogTrace("Начало {BreakType} перерыва оператора {OperatorId}", command.BreakType, command.OperatorId);

			return await _operatorStateService.StartBreak(command.OperatorId, command.BreakType);
		}

		[HttpPost]
		[Route("endbreak")]
		public async Task<OperatorResult> EndBreak([FromBody] EndBreak command)
		{
			_logger.LogTrace("Завершение перерыва оператора {OperatorId}", command.OperatorId);

			return await _operatorStateService.EndBreak(command.OperatorId);
		}

		[HttpGet("break-availability")]
		public async Task<OperatorBreakAvailability> GetBreakAvailabilityAsync(int operatorId)
		{
			_logger.LogTrace("Обновление возможности начать перерыв оператора {OperatorId}", operatorId);

			return await _operatorStateService.GetBreakAvailability(operatorId);
		}
	}
}
