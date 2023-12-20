using Pacs.Core;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Vodovoz.Core.Data.Repositories;

namespace Pacs.Server
{
	public class OperatorControllerProvider : IOperatorControllerProvider
	{
		private readonly IOperatorControllerFactory _operatorControllerFactory;
		private readonly IPacsRepository _pacsRepository;
		private ConcurrentDictionary<int, OperatorController> _controllers;

		public OperatorControllerProvider(IOperatorControllerFactory operatorControllerFactory, IPacsRepository pacsRepository)
		{
			_controllers = new ConcurrentDictionary<int, OperatorController>();
			_operatorControllerFactory = operatorControllerFactory ?? throw new ArgumentNullException(nameof(operatorControllerFactory));
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
		}

		public OperatorController GetOperatorController(int operatorId)
		{
			if(_controllers.TryGetValue(operatorId, out var controller))
			{
				return controller;
			}

			var @operator = _pacsRepository.GetOperator(operatorId);
			if(@operator == null)
			{
				throw new PacsException($"Оператор {operatorId} не зарегистрирован");
			}

			controller = _operatorControllerFactory.CreateOperatorController(@operator);
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
