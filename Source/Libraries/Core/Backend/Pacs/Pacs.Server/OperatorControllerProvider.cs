using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Pacs.Server
{
	public class OperatorControllerProvider : IOperatorControllerProvider
	{
		private readonly IOperatorControllerFactory _operatorControllerFactory;
		private ConcurrentDictionary<int, OperatorController> _controllers;

		public OperatorControllerProvider(IOperatorControllerFactory operatorControllerFactory)
		{
			_controllers = new ConcurrentDictionary<int, OperatorController>();
			_operatorControllerFactory = operatorControllerFactory ?? throw new ArgumentNullException(nameof(operatorControllerFactory));
		}

		public OperatorController GetOperatorController(int operatorId)
		{
			if(_controllers.TryGetValue(operatorId, out var controller))
			{
				return controller;
			}

			controller = _operatorControllerFactory.CreateOperatorController(operatorId);
			if(!_controllers.TryAdd(operatorId, controller))
			{
				return _controllers[operatorId];
			}

			controller.OnDisconnect += ControllerOnDisconnect;

			return controller;
		}

		public OperatorController GetOperatorController(string phoneNumber)
		{
			var controller = _controllers.Select(x => x.Value).FirstOrDefault(x => x.AssignedToPhone(phoneNumber));
			return controller;
		}

		private void ControllerOnDisconnect(object sender, int operatorId)
		{
			if(_controllers.TryRemove(operatorId, out var controller))
			{
				controller.Dispose();
				controller.OnDisconnect -= ControllerOnDisconnect;
			}
		}
	}
}
