using Pacs.Admin.Client.Consumers.Definitions;
using Pacs.Calls.Consumers.Definitions;
using Pacs.Operators.Client.Consumers.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pacs.Core
{
	public class PacsEndpointsConnector
	{
		private readonly MessageEndpointConnector _messageEndpointConnector;
		private readonly IPacsOperatorProvider _pacsOperatorProvider;
		private readonly IPacsAdministratorProvider _pacsAdministratorProvider;

		public PacsEndpointsConnector(
			MessageEndpointConnector messageEndpointConnector, 
			IPacsOperatorProvider pacsOperatorProvider,
			IPacsAdministratorProvider pacsAdministratorProvider)
		{
			_messageEndpointConnector = messageEndpointConnector ?? throw new ArgumentNullException(nameof(messageEndpointConnector));
			_pacsOperatorProvider = pacsOperatorProvider ?? throw new ArgumentNullException(nameof(pacsOperatorProvider));
			_pacsAdministratorProvider = pacsAdministratorProvider ?? throw new ArgumentNullException(nameof(pacsAdministratorProvider));
		}

		public void ConnectPacsEndpoints()
		{
			Task.Run(async () =>
			{
				var connectEndpointTasks = Enumerable.Concat(
					GetOperatorsEndpointsTask(),
					GetAdminEndpointsTask()
				);
				await Task.WhenAll(connectEndpointTasks);
			});
		}

		private IEnumerable<Task> GetOperatorsEndpointsTask()
		{
			if(_pacsOperatorProvider.OperatorId == null)
			{
				yield break;
			}
			yield return _messageEndpointConnector.TryConnectEndpoint<OperatorStateConsumerDefinition>();
			yield return _messageEndpointConnector.TryConnectEndpoint<OperatorsOnBreakConsumerDefinition>();
			yield return _messageEndpointConnector.TryConnectEndpoint<OperatorSettingsConsumerDefinition>();
		}

		private IEnumerable<Task> GetAdminEndpointsTask()
		{
			if(_pacsAdministratorProvider.AdministratorId == null)
			{
				yield break;
			}
			yield return _messageEndpointConnector.TryConnectEndpoint<SettingsConsumerDefinition>();
			yield return _messageEndpointConnector.TryConnectEndpoint<OperatorStateAdminConsumerDefinition>();
			yield return _messageEndpointConnector.TryConnectEndpoint<PacsCallEventConsumerDefinition>();
		}
	}
}
