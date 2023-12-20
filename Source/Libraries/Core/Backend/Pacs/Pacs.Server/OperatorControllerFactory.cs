using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pacs.Operators.Server;
using System;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public class OperatorControllerFactory : IOperatorControllerFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public OperatorControllerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public OperatorController CreateOperatorController(Operator @operator)
		{
			var logger = _serviceProvider.GetRequiredService<ILogger<OperatorController>>();
			var operatorAgentFactory = _serviceProvider.GetRequiredService<IOperatorAgentFactory>();
			var phoneController = _serviceProvider.GetRequiredService<IPhoneController>();
			var globalBreakController = _serviceProvider.GetRequiredService<GlobalBreakController>();
			var agent = operatorAgentFactory.CreateOperatorAgent(@operator);

			return new OperatorController(logger, agent, phoneController, globalBreakController);
		}
	}
}
