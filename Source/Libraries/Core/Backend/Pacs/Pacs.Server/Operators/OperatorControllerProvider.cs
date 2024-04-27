using Pacs.Core;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Vodovoz.Core.Data.Repositories;

namespace Pacs.Server.Operators
{
	public class OperatorControllerProvider : IOperatorControllerProvider
	{
		private readonly IOperatorControllerFactory _operatorControllerFactory;
		private readonly IPacsRepository _pacsRepository;
		private readonly IOperatorRepository _operatorRepository;
		private readonly ConcurrentDictionary<int, OperatorController> _operatorControllers;

		public OperatorControllerProvider(IOperatorControllerFactory operatorControllerFactory, IPacsRepository pacsRepository, IOperatorRepository operatorRepository)
		{
			_operatorControllerFactory = operatorControllerFactory ?? throw new ArgumentNullException(nameof(operatorControllerFactory));
			_pacsRepository = pacsRepository;
			_operatorRepository = operatorRepository ?? throw new ArgumentNullException(nameof(operatorRepository));

			_operatorControllers = new ConcurrentDictionary<int, OperatorController>();

			var lastOperators = _pacsRepository.GetOperators(DateTime.Now.Add(TimeSpan.FromHours(15)));

			if(!lastOperators.Any())
			{
				return;
			}

			foreach(var op in lastOperators)
			{
				if(!_operatorControllers.TryGetValue(op.Id, out var _))
				{
					_operatorControllers.TryAdd(op.Id, _operatorControllerFactory.CreateOperatorController(_operatorRepository.GetOperator(op.Id)));
				}
			}
		}

		public OperatorController GetOperatorController(int operatorId)
		{
			if(_operatorControllers.TryGetValue(operatorId, out var controller))
			{
				return controller;
			}

			var @operator = _operatorRepository.GetOperator(operatorId);

			if(@operator == null)
			{
				throw new PacsException($"Оператор {operatorId} не зарегистрирован");
			}

			controller = _operatorControllerFactory.CreateOperatorController(@operator);

			if(!_operatorControllers.TryAdd(operatorId, controller))
			{
				return _operatorControllers[operatorId];
			}

			controller.OnDisconnect += ControllerOnDisconnect;

			return controller;
		}

		public OperatorController GetOperatorController(string phoneNumber)
		{
			var controller = _operatorControllers.Select(x => x.Value).FirstOrDefault(x => x.AssignedToPhone(phoneNumber));
			return controller;
		}

		private void ControllerOnDisconnect(object sender, int operatorId)
		{
			if(_operatorControllers.TryRemove(operatorId, out var controller))
			{
				controller.Dispose();
				controller.OnDisconnect -= ControllerOnDisconnect;
			}
		}
	}
}
