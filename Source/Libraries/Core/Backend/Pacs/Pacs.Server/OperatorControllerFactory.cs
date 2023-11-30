using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Pacs.Server
{
	public class OperatorControllerFactory : IOperatorControllerFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public OperatorControllerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public OperatorController CreateOperatorController(int operatorId)
		{
			var logger = _serviceProvider.GetRequiredService<ILogger<OperatorController>>();
			var operatorAgentFactory = _serviceProvider.GetRequiredService<IOperatorAgentFactory>();
			var phoneController = _serviceProvider.GetRequiredService<IPhoneController>();
			var operatorBreakController = _serviceProvider.GetRequiredService<IOperatorBreakController>();
			var agent = operatorAgentFactory.CreateOperatorAgent(operatorId);

			return new OperatorController(logger, agent, phoneController, operatorBreakController);
		}
	}
}
