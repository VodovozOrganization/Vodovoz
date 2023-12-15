using Microsoft.AspNetCore.Mvc;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Server;
using System;
using System.Threading.Tasks;

namespace Pacs.Server
{
	[ApiController]
	[Route("pacs/global-break-availability")]
	public class GlobalBreakAvailabilityController
	{
		private readonly GlobalBreakController _globalBreakController;

		public GlobalBreakAvailabilityController(GlobalBreakController globalBreakController)
		{
			_globalBreakController = globalBreakController ?? throw new ArgumentNullException(nameof(globalBreakController));
		}

		[HttpGet("get")]
		public async Task<GlobalBreakAvailability> Get()
		{
			return await Task.FromResult(_globalBreakController.BreakAvailability);
		}

		[HttpGet("get-operators-on-break")]
		public async Task<OperatorsOnBreakEvent> GetOperatorsOnBreak()
		{
			return await Task.FromResult(_globalBreakController.GetOperatorsOnBreak());
		}
	}
}
