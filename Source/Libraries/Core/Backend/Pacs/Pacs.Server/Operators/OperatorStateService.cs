using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Commands;
using Pacs.Core.Messages.Events;
using Pacs.Server.Breaks;
using Pacs.Server.Phones;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public class OperatorStateService : IOperatorStateService
	{
		private readonly ILogger<OperatorStateService> _logger;
		private readonly IOperatorServerStateMachineFactory _operatorStateMachineFactory;
		private readonly IGlobalBreakController _globalBreakController;
		private readonly OperatorBreakController _operatorBreakController;
		private readonly IPacsRepository _pacsRepository;
		private readonly IPhoneController _phoneController;
		private readonly IOperatorRepository _operatorRepository;

		private readonly ConcurrentDictionary<int, OperatorServerStateMachine> _operatorControllers;

		private readonly TimeSpan _warmUpOperatorStatesFromNodTimeSpan = TimeSpan.FromHours(-15);

		public OperatorStateService(
			ILogger<OperatorStateService> logger,
			IOperatorServerStateMachineFactory operatorStateMachineFactory,
			IGlobalBreakController globalBreakController,
			OperatorBreakController operatorBreakController,
			IPacsRepository pacsRepository,
			IPhoneController phoneController,
			IOperatorRepository operatorRepository)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_operatorStateMachineFactory = operatorStateMachineFactory
				?? throw new ArgumentNullException(nameof(operatorStateMachineFactory));
			_globalBreakController = globalBreakController
				?? throw new ArgumentNullException(nameof(globalBreakController));
			_operatorBreakController = operatorBreakController
				?? throw new ArgumentNullException(nameof(operatorBreakController));
			_pacsRepository = pacsRepository
				?? throw new ArgumentNullException(nameof(pacsRepository));
			_phoneController = phoneController
				?? throw new ArgumentNullException(nameof(phoneController));
			_operatorRepository = operatorRepository
				?? throw new ArgumentNullException(nameof(operatorRepository));

			_operatorControllers = new ConcurrentDictionary<int, OperatorServerStateMachine>();

			WarmUpOperatorStates();
		}

		public async Task<OperatorResult> ChangePhone(int operatorId, string phoneNumber)
		{
			var operatorStateMachine = GetOperatorController(operatorId);

			try
			{
				if(!ValidateOperator(operatorStateMachine, out var result))
				{
					return result;
				}

				await CheckConnection(operatorStateMachine);

				if(!_phoneController.ValidatePhone(phoneNumber))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"Неизвестный номер телефона {phoneNumber}");
				}

				if(!_phoneController.CanAssign(phoneNumber, operatorId))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"Номер телефона {phoneNumber}, уже используется другим оператором");
				}


				if(!operatorStateMachine.CanChangedBy(OperatorTrigger.ChangePhone))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"В данный момент нельзя сменить номер телефона");
				}

				await operatorStateMachine.ChangePhone(phoneNumber);

				return new OperatorResult(GetResultContent(operatorStateMachine));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором завершить рабочую смену.");

				return new OperatorResult(GetResultContent(operatorStateMachine), ex.Message);
			}
		}

		public async Task<OperatorResult> Connect(int operatorId)
		{
			var operatorStateMachine = GetOperatorController(operatorId);

			try
			{
				if(!ValidateOperator(operatorStateMachine, out var result))
				{
					return result;
				}

				if(operatorStateMachine.CanChangedBy(OperatorTrigger.Connect))
				{
					await operatorStateMachine.Connect();
				}

				return new OperatorResult(GetResultContent(operatorStateMachine));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке подключения оператора");

				return new OperatorResult(GetResultContent(operatorStateMachine), ex.Message);
			}
		}

		public async Task<OperatorResult> Disconnect(int operatorId)
		{
			var operatorStateMachine = GetOperatorController(operatorId);

			try
			{
				await operatorStateMachine.Disconnect();

				return new OperatorResult(GetResultContent(operatorStateMachine));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке отключения оператора {OperatorId}", operatorId);

				return new OperatorResult(GetResultContent(operatorStateMachine), ex.Message);
			}
		}

		public async Task<OperatorResult> StartBreak(int operatorId, OperatorBreakType breakType)
		{
			var operatorStateMachine = GetOperatorController(operatorId);

			try
			{
				if(!ValidateOperator(operatorStateMachine, out var result))
				{
					return result;
				}

				await CheckConnection(operatorStateMachine);

				var checkResult = GetCheckStartBreakResult(operatorStateMachine, breakType);

				if(checkResult != null)
				{
					return checkResult;
				}

				await operatorStateMachine.StartBreak(breakType);

				return new OperatorResult(GetResultContent(operatorStateMachine));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором начать перерыв.");

				return new OperatorResult(GetResultContent(operatorStateMachine), ex.Message);
			}
		}

		public async Task<OperatorResult> EndBreak(int operatorId)
		{
			var operatorStateMachine = GetOperatorController(operatorId);

			try
			{
				if(!ValidateOperator(operatorStateMachine, out var result))
				{
					return result;
				}

				await CheckConnection(operatorStateMachine);

				if(!operatorStateMachine.CanChangedBy(OperatorTrigger.EndBreak))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"В данный момент нельзя завершить перерыв");
				}

				await operatorStateMachine.EndBreak();

				return new OperatorResult(GetResultContent(operatorStateMachine));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором завершить перерыв.");

				return new OperatorResult(GetResultContent(operatorStateMachine), ex.Message);
			}
		}

		public async Task<OperatorResult> StartWorkShift(int operatorId, string phoneNumber)
		{
			var operatorStateMachine = GetOperatorController(operatorId);

			try
			{
				if(!ValidateOperator(operatorStateMachine, out var result))
				{
					return result;
				}

				await CheckConnection(operatorStateMachine);

				if(!_phoneController.ValidatePhone(phoneNumber))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"Неизвестный номер телефона {phoneNumber}");
				}

				if(!_phoneController.CanAssign(phoneNumber, operatorId))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"Номер телефона {phoneNumber}, уже используется другим оператором");
				}

				if(!operatorStateMachine.CanChangedBy(OperatorTrigger.StartWorkShift))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"В данный момент нельзя начать смену");
				}

				await operatorStateMachine.StartWorkShift(phoneNumber);

				return new OperatorResult(GetResultContent(operatorStateMachine));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором начать рабочую смену.");

				return new OperatorResult(GetResultContent(operatorStateMachine), ex.Message);
			}
		}

		public async Task<OperatorResult> EndWorkShift(int operatorId, string reason)
		{
			var operatorStateMachine = GetOperatorController(operatorId);

			try
			{
				if(!ValidateOperator(operatorStateMachine, out var result))
				{
					return result;
				}

				await CheckConnection(operatorStateMachine);

				if(!operatorStateMachine.CanChangedBy(OperatorTrigger.EndWorkShift))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"В данный момент нельзя завершить смену");
				}

				if(!operatorStateMachine.CanEndWorkshift(reason))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"Необходимо указать причину закрытия смены, если завершается раньше планируемого");
				}

				await operatorStateMachine.EndWorkShift(reason);

				return new OperatorResult(GetResultContent(operatorStateMachine));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором завершить рабочую смену.");

				return new OperatorResult(GetResultContent(operatorStateMachine), ex.Message);
			}
		}

		public async Task KeepAlive(int operatorId)
		{
			try
			{
				var operatorStateMachine = GetOperatorController(operatorId);

				await operatorStateMachine.KeepAlive();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке вызова KeepAlive оператора {OperatorId}", operatorId);
			}
		}

		public async Task<OperatorResult> AdminStartBreak(int operatorId, OperatorBreakType breakType, int adminId, string reason)
		{
			var operatorStateMachine = GetOperatorController(operatorId);

			try
			{
				if(!ValidateOperator(operatorStateMachine, out var result))
				{
					return result;
				}

				await CheckConnection(operatorStateMachine);

				if(reason.IsNullOrWhiteSpace())
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), "Основание должно быть заполнено");
				}

				await operatorStateMachine.AdminStartBreak(breakType, adminId, reason);

				return new OperatorResult(GetResultContent(operatorStateMachine));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке администратором {AdminId} начать перерыв оператору {OperatorId}.",
					adminId, operatorId);

				return new OperatorResult(GetResultContent(operatorStateMachine), ex.Message);
			}
		}

		public async Task<OperatorResult> AdminEndBreak(int operatorId, int adminId, string reason)
		{
			var operatorStateMachine = GetOperatorController(operatorId);

			try
			{
				if(!ValidateOperator(operatorStateMachine, out var result))
				{
					return result;
				}

				await CheckConnection(operatorStateMachine);

				if(!operatorStateMachine.CanChangedBy(OperatorTrigger.EndBreak))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"В данный момент нельзя завершить перерыв");
				}

				if(reason.IsNullOrWhiteSpace())
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), "Основание должно быть заполнено");
				}

				await operatorStateMachine.AdminEndBreak(adminId, reason);

				return new OperatorResult(GetResultContent(operatorStateMachine));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке администратором {AdminId} завершить перерыв оператора {OperatorId}.",
					adminId, operatorId);

				return new OperatorResult(GetResultContent(operatorStateMachine), ex.Message);
			}
		}

		public async Task<OperatorResult> AdminEndWorkShift(int operatorId, int adminId, string reason)
		{
			var operatorStateMachine = GetOperatorController(operatorId);

			try
			{
				if(!ValidateOperator(operatorStateMachine, out var result))
				{
					return result;
				}

				await CheckConnection(operatorStateMachine);

				if(!operatorStateMachine.CanChangedBy(OperatorTrigger.EndWorkShift))
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), $"В данный момент нельзя завершить смену");
				}

				if(reason.IsNullOrWhiteSpace())
				{
					return new OperatorResult(GetResultContent(operatorStateMachine), "Основание должно быть заполнено");
				}

				await operatorStateMachine.AdminEndWorkShift(adminId, reason);

				return new OperatorResult(GetResultContent(operatorStateMachine));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке администратором {AdminId} завершить смену оператора {OperatorId}.",
					adminId, operatorId);

				return new OperatorResult(GetResultContent(operatorStateMachine), ex.Message);
			}
		}

		public async Task TakeCall(string toExtension, string callId)
		{
			try
			{
				var operatorStateMachine = FindOperatorControllerByOperatorPhone(toExtension);

				await CheckConnection(operatorStateMachine);

				if(!operatorStateMachine.CanChangedBy(OperatorTrigger.TakeCall))
				{
					return;
				}

				await operatorStateMachine.TakeCallEvent(callId);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке принятия звонка");
			}
		}

		public async Task EndCall(string toExtension, string callId)
		{
			try
			{
				var operatorStateMachine = FindOperatorControllerByOperatorPhone(toExtension);

				await CheckConnection(operatorStateMachine);

				if(!operatorStateMachine.CanChangedBy(OperatorTrigger.EndCall))
				{
					return;
				}

				await operatorStateMachine.EndCallEvent();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке завершения звонка");
			}
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
					_operatorControllers.TryAdd(operatorState.OperatorId, _operatorStateMachineFactory.CreateOperatorAgent(operatorState.OperatorId));
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
			InvalidateOperatorCache(operatorId);
		}

		/// <summary>
		/// Поиск стейт машины по номеру телефона
		/// </summary>
		/// <param name="phoneNumber">Добавочный номер оператора</param>
		/// <returns></returns>
		private OperatorServerStateMachine FindOperatorControllerByOperatorPhone(string phoneNumber)
		{
			var controller = _operatorControllers
				.Select(x => x.Value)
				.FirstOrDefault(x => x.OperatorState.PhoneNumber == phoneNumber);

			return controller;
		}

		/// <summary>
		/// Получение стейт машины оператора
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		/// <exception cref="PacsException"></exception>
		private OperatorServerStateMachine GetOperatorController(int operatorId)
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

			controller = _operatorStateMachineFactory.CreateOperatorAgent(operatorId);

			if(!_operatorControllers.TryAdd(operatorId, controller))
			{
				return _operatorControllers[operatorId];
			}

			controller.OnDisconnect += OnOperatorDisconnected;

			return controller;
		}

		private void InvalidateOperatorCache(int operatorId)
		{
			if(_operatorControllers.TryRemove(operatorId, out var stateMachine))
			{
				stateMachine.Dispose();
			}
		}

		private async Task CheckConnection(OperatorServerStateMachine operatorServerStateMachine)
		{
			if(operatorServerStateMachine.OperatorState.State == OperatorStateType.Disconnected)
			{
				await operatorServerStateMachine.Connect();
			}
		}

		private bool ValidateOperator(OperatorServerStateMachine operatorServerStateMachine, out OperatorResult operatorResult)
		{
			if(operatorServerStateMachine.OperatorEnabled())
			{
				operatorResult = null;

				return true;
			}
			else
			{
				operatorResult = new OperatorResult(GetResultContent(operatorServerStateMachine), $"Оператор отключен от СКУД");

				return false;
			}
		}

		private OperatorStateEvent GetResultContent(OperatorServerStateMachine operatorServerStateMachine)
		{
			var content = new OperatorStateEvent
			{
				State = operatorServerStateMachine.OperatorState,
				BreakAvailability = operatorServerStateMachine.BreakAvailability,
			};

			return content;
		}

		private OperatorResult GetCheckStartBreakResult(OperatorServerStateMachine operatorServerStateMachine, OperatorBreakType breakType)
		{
			string description = "";
			bool canStartGlobal;
			bool canStart;

			if(breakType == OperatorBreakType.Long)
			{
				canStartGlobal = _globalBreakController.BreakAvailability.LongBreakAvailable;

				if(!canStartGlobal)
				{
					description = _globalBreakController.BreakAvailability.LongBreakDescription;
				}

				canStart = operatorServerStateMachine.BreakAvailability.LongBreakAvailable;

				if(!canStart)
				{
					description = operatorServerStateMachine.BreakAvailability.LongBreakDescription;
				}
			}
			else
			{
				canStartGlobal = _globalBreakController.BreakAvailability.ShortBreakAvailable;

				if(!canStartGlobal)
				{
					description = _globalBreakController.BreakAvailability.ShortBreakDescription;
				}

				canStart = operatorServerStateMachine.BreakAvailability.ShortBreakAvailable;
				
				if(!canStart)
				{
					description = operatorServerStateMachine.BreakAvailability.ShortBreakDescription;
				}
			}

			if(!canStartGlobal || !canStart)
			{
				return new OperatorResult(GetResultContent(operatorServerStateMachine), description);
			}

			if(!operatorServerStateMachine.CanChangedBy(OperatorTrigger.StartBreak))
			{
				return new OperatorResult(GetResultContent(operatorServerStateMachine), $"В данный момент нельзя начать перерыв");
			}

			return null;
		}
	}
}
