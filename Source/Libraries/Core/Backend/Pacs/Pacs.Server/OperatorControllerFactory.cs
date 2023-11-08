using Microsoft.Extensions.Logging;
using System;

namespace Pacs.Server
{
	public class OperatorControllerFactory : IOperatorControllerFactory
	{
		private readonly ILogger<OperatorController> _logger;
		private readonly IOperatorAgentFactory _operatorAgentFactory;
		private readonly IPhoneController _phoneController;

		public OperatorControllerFactory(ILogger<OperatorController> logger, IOperatorAgentFactory operatorAgentFactory, IPhoneController phoneController)
		{
			_logger = logger;
			_operatorAgentFactory = operatorAgentFactory ?? throw new ArgumentNullException(nameof(operatorAgentFactory));
			_phoneController = phoneController ?? throw new ArgumentNullException(nameof(phoneController));
		}

		public OperatorController CreateOperatorController(int operatorId)
		{
			var agent = _operatorAgentFactory.CreateOperatorAgent(operatorId);
			return new OperatorController(_logger, agent, _phoneController);
		}
	}
}
