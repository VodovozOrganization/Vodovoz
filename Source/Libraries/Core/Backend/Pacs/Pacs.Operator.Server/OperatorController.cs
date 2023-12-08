using Microsoft.AspNetCore.Mvc;
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
		private readonly IOperatorControllerProvider _controllerProvider;

		public OperatorController(IOperatorControllerProvider controllerProvider)
		{
			_controllerProvider = controllerProvider ?? throw new ArgumentNullException(nameof(controllerProvider));
		}

		[HttpPost("connect")]
		public async Task<OperatorResult> Connect([FromBody] Connect command)
		{
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.Connect();

			return result;
		}

		[HttpPost("disconnect")]
		public async Task<OperatorResult> Disconnect([FromBody] Disconnect command)
		{
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.Disconnect();

			return result;
		}

		[HttpPost("startworkshift")]
		public async Task<OperatorResult> StartWorkShift([FromBody] StartWorkShift command)
		{
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.StartWorkShift(command.PhoneNumber);

			return result;
		}

		[HttpPost("endworkshift")]
		public async Task<OperatorResult> EndWorkShift([FromBody] EndWorkShift command)
		{
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.EndWorkShift();

			return result;
		}

		[HttpPost("changephone")]
		public async Task<OperatorResult> ChangePhone([FromBody] ChangePhone command)
		{
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.ChangePhone(command.PhoneNumber);

			return result;
		}

		[HttpPost("startbreak")]
		public async Task<OperatorResult> StartBreak([FromBody] StartBreak command)
		{
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.StartBreak(command.BreakType);

			return result;
		}

		[HttpPost("endbreak")]
		public async Task<OperatorResult> EndBreak([FromBody] EndBreak command)
		{
			var controller = _controllerProvider.GetOperatorController(command.OperatorId);
			var result = await controller.EndBreak();

			return result;
		}
	}
}
