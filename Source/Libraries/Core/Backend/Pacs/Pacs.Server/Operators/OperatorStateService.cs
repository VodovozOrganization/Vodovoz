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

		/// <summary>
		/// Смена телефона оператора
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="phoneNumber">Новый номер телефона</param>
		/// <returns></returns>
		public async Task<OperatorResult> ChangePhone(int operatorId, string phoneNumber)
		{
			return await GetOperatorController(operatorId).ChangePhone(phoneNumber);
		}

		/// <summary>
		/// Подключение оператора
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		public async Task<OperatorResult> Connect(int operatorId)
		{
			return await GetOperatorController(operatorId).Connect();
		}

		/// <summary>
		/// Отключение оператора
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		public async Task<OperatorResult> Disconnect(int operatorId)
		{
			return await GetOperatorController(operatorId).Disconnect();
		}

		/// <summary>
		/// Начало перерыва
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="breakType"><see cref="OperatorBreakType">Тип перерыва</see></param>
		/// <returns></returns>
		public async Task<OperatorResult> StartBreak(int operatorId, OperatorBreakType breakType)
		{
			return await GetOperatorController(operatorId).StartBreak(breakType);
		}

		/// <summary>
		/// Завершение перерыва
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		public async Task<OperatorResult> EndBreak(int operatorId)
		{
			return await GetOperatorController(operatorId).EndBreak();
		}

		/// <summary>
		/// Начало смены
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="phoneNumber">Номер телефона</param>
		/// <returns></returns>
		public async Task<OperatorResult> StartWorkShift(int operatorId, string phoneNumber)
		{
			return await GetOperatorController(operatorId).StartWorkShift(phoneNumber);
		}

		/// <summary>
		/// Завершение смены оператора
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="reason">Причина</param>
		/// <returns></returns>
		public async Task<OperatorResult> EndWorkShift(int operatorId, string reason)
		{
			return await GetOperatorController(operatorId).EndWorkShift(reason);
		}

		/// <summary>
		/// Поддержание состояния подключения
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		public async Task KeepAlive(int operatorId)
		{
			await GetOperatorController(operatorId).KeepAlive();
		}

		private void ControllerOnDisconnect(object sender, int operatorId)
		{
			if(_operatorControllers.TryRemove(operatorId, out var controller))
			{
				controller.Dispose();
				controller.OnDisconnect -= ControllerOnDisconnect;
			}
		}

		public OperatorController GetOperatorController(string phoneNumber)
		{
			var controller = _operatorControllers
				.Select(x => x.Value)
				.FirstOrDefault(x => x.AssignedToPhone(phoneNumber));

			return controller;
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

			controller = _operatorControllerFactory.CreateOperatorController(operatorId);

			if(!_operatorControllers.TryAdd(operatorId, controller))
			{
				return _operatorControllers[operatorId];
			}

			controller.OnDisconnect += ControllerOnDisconnect;

			return controller;
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
	}
}
