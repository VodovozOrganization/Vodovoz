using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Commands;
using Pacs.Server;
using System;
using System.Threading.Tasks;

namespace Pacs.Operators.Server
{
	[ApiController]
	[Route("pacs/admin/operator")]
	public class AdminOperatorController
	{
		private readonly ILogger<OperatorController> _logger;
		private readonly IOperatorControllerProvider _controllerProvider;

		public AdminOperatorController(ILogger<OperatorController> logger, IOperatorControllerProvider controllerProvider)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_controllerProvider = controllerProvider ?? throw new ArgumentNullException(nameof(controllerProvider));
		}

		[HttpPost("startbreak")]
		public async Task<OperatorResult> StartBreak([FromBody] AdminStartBreak command)
		{
			_logger.LogTrace("Начало {BreakType} перерыва оператора {OperatorId} вызванное командой администратора {AdminId}", 
				command.BreakType, command.OperatorId, command.AdminId);
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.AdminStartBreak(command.BreakType, command.AdminId, command.Reason);

			return result;
		}

		[HttpPost("endbreak")]
		public async Task<OperatorResult> EndBreak([FromBody] AdminEndBreak command)
		{
			_logger.LogTrace("Завершение перерыва оператора {OperatorId} вызванное командой администратора {AdminId}",
				command.OperatorId, command.AdminId);
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.AdminEndBreak(command.AdminId, command.Reason);

			return result;
		}
	}
}
