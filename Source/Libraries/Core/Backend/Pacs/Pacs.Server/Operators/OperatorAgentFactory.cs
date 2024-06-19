using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pacs.Server.Breaks;
using Pacs.Server.Phones;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Settings.Pacs;

namespace Pacs.Server.Operators
{
	public class OperatorAgentFactory : IOperatorAgentFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public OperatorAgentFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public OperatorServerStateMachine CreateOperatorAgent(int operatorId)
		{
			var operatorServerLogger = _serviceProvider.GetRequiredService<ILogger<OperatorServerStateMachine>>();
			var operatorBreakControllerLogger = _serviceProvider.GetRequiredService<ILogger<OperatorBreakController>>();
			var pacsSettings = _serviceProvider.GetRequiredService<IPacsSettings>();
			var operatorRepository = _serviceProvider.GetRequiredService<IOperatorRepository>();
			var operatorNotifier = _serviceProvider.GetRequiredService<IOperatorNotifier>();
			var phoneController = _serviceProvider.GetRequiredService<IPhoneController>();
			var uowFactory = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
			var pacsRepository = _serviceProvider.GetRequiredService<IPacsRepository>();
			var globalBreakController = _serviceProvider.GetRequiredService<IGlobalBreakController>();
			var breakController = new OperatorBreakController(operatorId, operatorBreakControllerLogger, globalBreakController, pacsRepository);
			return new OperatorServerStateMachine(
				operatorId,
				operatorServerLogger,
				pacsSettings,
				operatorRepository,
				operatorNotifier,
				phoneController,
				globalBreakController,
				breakController,
				uowFactory);
		}
	}
}
