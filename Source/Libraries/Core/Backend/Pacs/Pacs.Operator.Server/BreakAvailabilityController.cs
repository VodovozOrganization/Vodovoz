using Microsoft.AspNetCore.Mvc;
using Pacs.Core.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pacs.Server
{
	[ApiController]
	[Route("pacs/break-availability")]
	public class BreakAvailabilityController
	{
		private readonly IOperatorBreakController _operatorBreakController;

		public BreakAvailabilityController(IOperatorBreakController operatorBreakController)
		{
			_operatorBreakController = operatorBreakController ?? throw new ArgumentNullException(nameof(operatorBreakController));
		}

		[HttpGet("get")]
		public async Task<bool> Get()
		{
			return await Task.FromResult<bool>(_operatorBreakController.CanStartBreak);
		}
	}
}
