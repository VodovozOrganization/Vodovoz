using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Data.Repositories;
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

		public OperatorServerAgent CreateOperatorAgent(int operatorId)
		{
			var logger = _serviceProvider.GetRequiredService<ILogger<OperatorServerAgent>>();
			var pacsSettings = _serviceProvider.GetRequiredService<IPacsSettings>();
			var operatorRepository = _serviceProvider.GetRequiredService<IOperatorRepository>();
			var operatorNotifier = _serviceProvider.GetRequiredService<IOperatorNotifier>();
			var phoneController = _serviceProvider.GetRequiredService<IPhoneController>();
			var operatorBreakController = _serviceProvider.GetRequiredService<IOperatorBreakController>();
			var uowFactory = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();

			return new OperatorServerAgent(
				operatorId,
				logger,
				pacsSettings,
				operatorRepository,
				operatorNotifier,
				phoneController,
				operatorBreakController,
				uowFactory);
		}
	}
}
