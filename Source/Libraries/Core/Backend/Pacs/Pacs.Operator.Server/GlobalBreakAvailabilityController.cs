using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pacs.Core.Messages.Events;
using Pacs.Server.Breaks;
using System;
using System.Threading.Tasks;

namespace Pacs.Server
{
	[ApiController]
	[Route("pacs/global-break-availability")]
	[Authorize]
	public class GlobalBreakAvailabilityController
	{
		private readonly IGlobalBreakController _globalBreakController;

		public GlobalBreakAvailabilityController(IGlobalBreakController globalBreakController)
		{
			_globalBreakController = globalBreakController ?? throw new ArgumentNullException(nameof(globalBreakController));
		}

		[HttpGet]
		[Route("get")]
		public async Task<GlobalBreakAvailabilityEvent> Get()
		{
			return await Task.FromResult(_globalBreakController.BreakAvailability);
		}

		[HttpGet]
		[Route("get-operators-on-break")]
		public async Task<OperatorsOnBreakEvent> GetOperatorsOnBreak()
		{
			return await Task.FromResult(_globalBreakController.GetOperatorsOnBreak());
		}
	}
}
