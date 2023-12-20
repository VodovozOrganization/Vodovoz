using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pacs.Operators.Server;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Settings.Pacs;

namespace Pacs.Server
{
	public class OperatorAgentFactory : IOperatorAgentFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public OperatorAgentFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public OperatorServerAgent CreateOperatorAgent(Operator @operator)
		{
			var logger = _serviceProvider.GetRequiredService<ILogger<OperatorServerAgent>>();
			var pacsSettings = _serviceProvider.GetRequiredService<IPacsSettings>();
			var operatorRepository = _serviceProvider.GetRequiredService<IOperatorRepository>();
			var operatorNotifier = _serviceProvider.GetRequiredService<IOperatorNotifier>();
			var phoneController = _serviceProvider.GetRequiredService<IPhoneController>();
			var uowFactory = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
			var pacsRepository = _serviceProvider.GetRequiredService<IPacsRepository>();
			var globalBreakController = _serviceProvider.GetRequiredService<GlobalBreakController>();
			var breakController = new OperatorBreakController(@operator.Id, globalBreakController, pacsRepository);
			return new OperatorServerAgent(
				@operator,
				logger,
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
