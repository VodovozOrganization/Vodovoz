using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Commands;
using Pacs.Server;
using System;
using System.Threading.Tasks;

namespace Pacs.Operators.Server
{
	[ApiController]
	[Route("pacs/operator")]
	public class OperatorController : ControllerBase
	{
		private readonly ILogger<OperatorController> _logger;
		private readonly IOperatorControllerProvider _controllerProvider;

		public OperatorController(ILogger<OperatorController> logger, IOperatorControllerProvider controllerProvider)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_controllerProvider = controllerProvider ?? throw new ArgumentNullException(nameof(controllerProvider));
		}

		[HttpPost("connect")]
		public async Task<OperatorResult> Connect([FromBody] Connect command)
		{
			_logger.LogTrace("Подключение оператора {OperatorId}", command.OperatorId);
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.Connect();
			return result;
		}

		[HttpPost("disconnect")]
		public async Task<OperatorResult> Disconnect([FromBody] Disconnect command)
		{
			_logger.LogTrace("Отключение оператора {OperatorId}", command.OperatorId);
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.Disconnect();

			return result;
		}

		[HttpPost("startworkshift")]
		public async Task<OperatorResult> StartWorkShift([FromBody] StartWorkShift command)
		{
			_logger.LogTrace("Начало смены оператора {OperatorId}", command.OperatorId);
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.StartWorkShift(command.PhoneNumber);

			return result;
		}

		[HttpPost("endworkshift")]
		public async Task<OperatorResult> EndWorkShift([FromBody] EndWorkShift command)
		{
			_logger.LogTrace("Завершение смены оператора {OperatorId}", command.OperatorId);
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.EndWorkShift(command.Reason);

			return result;
		}

		[HttpPost("changephone")]
		public async Task<OperatorResult> ChangePhone([FromBody] ChangePhone command)
		{
			_logger.LogTrace("Смена телефона оператора {OperatorId} на  {Phone}", command.OperatorId, command.PhoneNumber);
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.ChangePhone(command.PhoneNumber);

			return result;
		}

		[HttpPost("startbreak")]
		public async Task<OperatorResult> StartBreak([FromBody] StartBreak command)
		{
			_logger.LogTrace("Начало {BreakType} перерыва оператора {OperatorId}", command.BreakType, command.OperatorId);
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.StartBreak(command.BreakType);

			return result;
		}

		[HttpPost("endbreak")]
		public async Task<OperatorResult> EndBreak([FromBody] EndBreak command)
		{
			_logger.LogTrace("Завершение перерыва оператора {OperatorId}", command.OperatorId);
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.EndBreak();

			return result;
		}
	}
}
