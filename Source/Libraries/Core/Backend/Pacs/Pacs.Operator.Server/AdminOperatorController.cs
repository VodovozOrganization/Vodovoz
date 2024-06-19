using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Commands;
using Pacs.Server.Operators;
using System;
using System.Threading.Tasks;

namespace Pacs.Operators.Server
{
	[ApiController]
	[Route("pacs/admin/operator")]
	[Authorize]
	public class AdminOperatorController
	{
		private readonly ILogger<OperatorController> _logger;
		private readonly IOperatorStateService _operatorStateService;

		public AdminOperatorController(ILogger<OperatorController> logger, IOperatorStateService controllerProvider)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_operatorStateService = controllerProvider ?? throw new ArgumentNullException(nameof(controllerProvider));
		}

		[HttpPost]
		[Route("startbreak")]
		public async Task<OperatorResult> StartBreak([FromBody] AdminStartBreak command)
		{
			_logger.LogTrace("Начало {BreakType} перерыва оператора {OperatorId} вызванное командой администратора {AdminId}", 
				command.BreakType, command.OperatorId, command.AdminId);
			var controller = _operatorStateService.GetOperatorController(command.OperatorId);
			var result = await controller.AdminStartBreak(command.BreakType, command.AdminId, command.Reason);

			return result;
		}

		[HttpPost]
		[Route("endbreak")]
		public async Task<OperatorResult> EndBreak([FromBody] AdminEndBreak command)
		{
			_logger.LogTrace("Завершение перерыва оператора {OperatorId} вызванное командой администратора {AdminId}",
				command.OperatorId, command.AdminId);
			var controller = _operatorStateService.GetOperatorController(command.OperatorId);
			var result = await controller.AdminEndBreak(command.AdminId, command.Reason);

			return result;
		}

		[HttpPost]
		[Route("endworkshift")]
		public async Task<OperatorResult> EndWorkShift([FromBody] AdminEndWorkShift command)
		{
			_logger.LogTrace("Завершение смены оператора {OperatorId} вызванное командой администратора {AdminId}",
				command.OperatorId, command.AdminId);
			var controller = _operatorStateService.GetOperatorController(command.OperatorId);
			var result = await controller.AdminEndWorkShift(command.AdminId, command.Reason);

			return result;
		}
	}
}
