using Pacs.Core;
using Pacs.Core.Messages.Commands;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;

namespace Pacs.Server.Operators
{
	public class OperatorStateService : IOperatorStateService
	{
		private readonly IOperatorControllerFactory _operatorControllerFactory;
		private readonly IPacsRepository _pacsRepository;
		private readonly IOperatorRepository _operatorRepository;
		private readonly ConcurrentDictionary<int, OperatorController> _operatorControllers;

		private readonly TimeSpan _warmUpOperatorStatesFromNodTimeSpan = TimeSpan.FromHours(-15);

		public OperatorStateService(
			IOperatorControllerFactory operatorControllerFactory,
			IPacsRepository pacsRepository,
			IOperatorRepository operatorRepository)
		{
			_operatorControllerFactory = operatorControllerFactory
				?? throw new ArgumentNullException(nameof(operatorControllerFactory));
			_pacsRepository = pacsRepository
				?? throw new ArgumentNullException(nameof(pacsRepository));
			_operatorRepository = operatorRepository
				?? throw new ArgumentNullException(nameof(operatorRepository));

			_operatorControllers = new ConcurrentDictionary<int, OperatorController>();

			WarmUpOperatorStates();
		}

		public async Task<OperatorResult> ChangePhone(int operatorId, string phoneNumber)
		{
			return await GetOperatorController(operatorId).ChangePhone(phoneNumber);
		}

		public async Task<OperatorResult> Connect(int operatorId)
		{
			return await GetOperatorController(operatorId).Connect();
		}

		public async Task<OperatorResult> Disconnect(int operatorId)
		{
			return await GetOperatorController(operatorId).Disconnect();
		}

		public async Task<OperatorResult> StartBreak(int operatorId, OperatorBreakType breakType)
		{
			return await GetOperatorController(operatorId).StartBreak(breakType);
		}

		public async Task<OperatorResult> EndBreak(int operatorId)
		{
			return await GetOperatorController(operatorId).EndBreak();
		}

		public async Task<OperatorResult> StartWorkShift(int operatorId, string phoneNumber)
		{
			return await GetOperatorController(operatorId).StartWorkShift(phoneNumber);
		}

		public async Task<OperatorResult> EndWorkShift(int operatorId, string reason)
		{
			return await GetOperatorController(operatorId).EndWorkShift(reason);
		}

		public async Task KeepAlive(int operatorId)
		{
			await GetOperatorController(operatorId).KeepAlive();
		}

		public async Task<OperatorResult> AdminStartBreak(int operatorId, OperatorBreakType breakType, int adminId, string reason)
		{
			return await GetOperatorController(operatorId).AdminStartBreak(breakType, adminId, reason);
		}

		public async Task<OperatorResult> AdminEndBreak(int operatorId, int adminId, string reason)
		{
			return await GetOperatorController(operatorId).AdminEndBreak(adminId, reason);
		}

		public async Task<OperatorResult> AdminEndWorkShift(int operatorId, int adminId, string reason)
		{
			return await GetOperatorController(operatorId).AdminEndWorkShift(adminId, reason);
		}

		public async Task TakeCall(string toExtension, string callId)
		{
			await FindOperatorControllerByOperatorPhone(toExtension)?.TakeCall(callId);
		}

		public async Task EndCall(string toExtension, string callId)
		{
			await FindOperatorControllerByOperatorPhone(toExtension)?.EndCall(callId);
		}

		/// <summary>
		/// Прогрев кэша
		/// </summary>
		private void WarmUpOperatorStates()
		{
			var lastOperators = _pacsRepository.GetOperatorStatesFrom(DateTime.Now.Add(_warmUpOperatorStatesFromNodTimeSpan));

			if(!lastOperators.Any())
			{
				return;
			}

			foreach(var operatorState in lastOperators)
			{
				if(!_operatorControllers.TryGetValue(operatorState.OperatorId, out var _))
				{
					_operatorControllers.TryAdd(operatorState.OperatorId, _operatorControllerFactory.CreateOperatorController(operatorState.OperatorId));
				}
			}
		}

		/// <summary>
		/// Обработка события отключение оператора
		/// </summary>
		/// <param name="sender">Объект оповещающий о событии</param>
		/// <param name="operatorId">Идентификатор оператора</param>
		private void OnOperatorDisconnected(object sender, int operatorId)
		{
			if(_operatorControllers.TryRemove(operatorId, out var controller))
			{
				controller.Dispose();
				controller.OnDisconnect -= OnOperatorDisconnected;
			}
		}

		private OperatorController FindOperatorControllerByOperatorPhone(string phoneNumber)
		{
			var controller = _operatorControllers
				.Select(x => x.Value)
				.FirstOrDefault(x => x.AssignedToPhone(phoneNumber));

			return controller;
		}

		private OperatorController GetOperatorController(int operatorId)
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

			controller = _operatorControllerFactory.CreateOperatorController(operatorId);

			if(!_operatorControllers.TryAdd(operatorId, controller))
			{
				return _operatorControllers[operatorId];
			}

			controller.OnDisconnect += OnOperatorDisconnected;

			return controller;
		}
	}
}
