using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Commands;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Server;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public class OperatorController : IDisposable
	{
		private readonly ILogger<OperatorController> _logger;
		private readonly OperatorServerAgent _operatorAgent;
		private readonly IPhoneController _phoneController;
		private readonly GlobalBreakController _globalBreakController;

		public event EventHandler<int> OnDisconnect;

		public OperatorController(
			ILogger<OperatorController> logger, 
			OperatorServerAgent operatorAgent, 
			IPhoneController phoneController,
			GlobalBreakController globalBreakController
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_operatorAgent = operatorAgent ?? throw new ArgumentNullException(nameof(operatorAgent));
			_phoneController = phoneController ?? throw new ArgumentNullException(nameof(phoneController));
			_globalBreakController = globalBreakController ?? throw new ArgumentNullException(nameof(globalBreakController));

			operatorAgent.OnDisconnect += OperatorAgentOnDisconnect;
		}

		private void OperatorAgentOnDisconnect(object sender, int operatorId)
		{
			OnDisconnect?.Invoke(sender, operatorId);
		}

		public bool AssignedToPhone(string phone)
		{
			return _operatorAgent.OperatorState.PhoneNumber == phone;
		}

		public async Task<OperatorResult> Connect()
		{
			try
			{
				if(_operatorAgent.CanChangedBy(OperatorTrigger.Connect))
				{
					await _operatorAgent.Connect();
				}

				return new OperatorResult(GetResultContent());
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке подключения оператора");
				return new OperatorResult(GetResultContent(), ex.Message);
			}
		}

		public async Task<OperatorResult> StartWorkShift(string phoneNumber)
		{
			try
			{
				await CheckConnection();

				if(!_phoneController.ValidatePhone(phoneNumber))
				{
					return new OperatorResult(GetResultContent(), $"Неизвестный номер телефона {phoneNumber}");
				}

				if(!_phoneController.CanAssign(phoneNumber, _operatorAgent.OperatorId))
				{
					return new OperatorResult(GetResultContent(), $"Номер телефона {phoneNumber}, уже используется другим оператором");
				}

				if(!_operatorAgent.CanChangedBy(OperatorTrigger.StartWorkShift))
				{
					return new OperatorResult(GetResultContent(), $"В данный момент нельзя начать смену");
				}

				await _operatorAgent.StartWorkShift(phoneNumber);
				return new OperatorResult(GetResultContent());
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором начать рабочую смену.");
				return new OperatorResult(GetResultContent(), ex.Message);
			}
		}

		public async Task<OperatorResult> EndWorkShift(string reason = null)
		{
			try
			{
				await CheckConnection();

				if(!_operatorAgent.CanChangedBy(OperatorTrigger.EndWorkShift))
				{
					return new OperatorResult(GetResultContent(), $"В данный момент нельзя завершить смену");
				}

				if(!_operatorAgent.CanEndWorkshift(reason))
				{
					return new OperatorResult(GetResultContent(), $"Необходимо указать причину закрытия смены, если завершается раньше планируемого");
				}

				await _operatorAgent.EndWorkShift(reason);
				return new OperatorResult(GetResultContent());
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором завершить рабочую смену.");
				return new OperatorResult(GetResultContent(), ex.Message);
			}
		}

		public async Task<OperatorResult> StartBreak(OperatorBreakType breakType)
		{
			try
			{
				await CheckConnection();

				var checkResult = GetCheckStartBreakResult(breakType);
				if(checkResult != null)
				{
					return checkResult;
				}

				await _operatorAgent.StartBreak(breakType);
				return new OperatorResult(GetResultContent());
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором начать перерыв.");
				return new OperatorResult(GetResultContent(), ex.Message);
			}
		}

		public async Task<OperatorResult> AdminStartBreak(OperatorBreakType breakType, int adminId, string reason)
		{
			try
			{
				await CheckConnection();

				if(reason.IsNullOrWhiteSpace())
				{
					return new OperatorResult(GetResultContent(), "Основание должно быть заполнено");
				}

				await _operatorAgent.AdminStartBreak(breakType, adminId, reason);
				return new OperatorResult(GetResultContent());
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке администратором {AdminId} начать перерыв оператору {OperatorId}.", 
					adminId, _operatorAgent.OperatorId);
				return new OperatorResult(GetResultContent(), ex.Message);
			}
		}

		private OperatorResult GetCheckStartBreakResult(OperatorBreakType breakType)
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
				canStart = _operatorAgent.BreakAvailability.LongBreakAvailable;
				if(!canStart)
				{
					description = _operatorAgent.BreakAvailability.LongBreakDescription;
				}
			}
			else
			{
				canStartGlobal = _globalBreakController.BreakAvailability.ShortBreakAvailable;
				if(!canStartGlobal)
				{
					description = _globalBreakController.BreakAvailability.ShortBreakDescription;
				}
				canStart = _operatorAgent.BreakAvailability.ShortBreakAvailable;
				if(!canStart)
				{
					description = _operatorAgent.BreakAvailability.ShortBreakDescription;
				}
			}

			if(!canStartGlobal || !canStart)
			{
				return new OperatorResult(GetResultContent(), description);
			}

			if(!_operatorAgent.CanChangedBy(OperatorTrigger.StartBreak))
			{
				return new OperatorResult(GetResultContent(), $"В данный момент нельзя начать перерыв");
			}

			return null;
		}

		public async Task<OperatorResult> EndBreak()
		{
			try
			{
				await CheckConnection();

				if(!_operatorAgent.CanChangedBy(OperatorTrigger.EndBreak))
				{
					return new OperatorResult(GetResultContent(), $"В данный момент нельзя завершить перерыв");
				}

				await _operatorAgent.EndBreak();
				return new OperatorResult(GetResultContent());
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором завершить перерыв.");
				return new OperatorResult(GetResultContent(), ex.Message);
			}
		}

		public async Task<OperatorResult> AdminEndBreak(int adminId, string reason)
		{
			try
			{
				await CheckConnection();

				if(!_operatorAgent.CanChangedBy(OperatorTrigger.EndBreak))
				{
					return new OperatorResult(GetResultContent(), $"В данный момент нельзя завершить перерыв");
				}

				if(reason.IsNullOrWhiteSpace())
				{
					return new OperatorResult(GetResultContent(), "Основание должно быть заполнено");
				}

				await _operatorAgent.AdminEndBreak(adminId, reason);
				return new OperatorResult(GetResultContent());
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке администратором {AdminId} завершить перерыв оператора {OperatorId}.",
					adminId, _operatorAgent.OperatorId);
				return new OperatorResult(GetResultContent(), ex.Message);
			}
		}

		public async Task<OperatorResult> ChangePhone(string phoneNumber)
		{
			try
			{
				await CheckConnection();

				if(!_phoneController.ValidatePhone(phoneNumber))
				{
					return new OperatorResult(GetResultContent(), $"Неизвестный номер телефона {phoneNumber}");
				}

				if(!_phoneController.CanAssign(phoneNumber, _operatorAgent.OperatorId))
				{
					return new OperatorResult(GetResultContent(), $"Номер телефона {phoneNumber}, уже используется другим оператором");
				}

				if(!_operatorAgent.CanChangedBy(OperatorTrigger.ChangePhone))
				{
					return new OperatorResult(GetResultContent(), $"В данный момент нельзя сменить номер телефона");
				}

				await _operatorAgent.ChangePhone(phoneNumber);
				return new OperatorResult(GetResultContent());
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором завершить рабочую смену.");
				return new OperatorResult(GetResultContent(), ex.Message);
			}
		}

		public async Task TakeCall(string callId)
		{
			try
			{
				await CheckConnection();

				if(!_operatorAgent.CanChangedBy(OperatorTrigger.TakeCall))
				{
					return;
				}
				await _operatorAgent.TakeCallEvent(callId);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке принятия звонка");
			}
		}

		public async Task EndCall(string callId)
		{
			try
			{
				await CheckConnection();

				if(!_operatorAgent.CanChangedBy(OperatorTrigger.EndCall))
				{
					return;
				}
				await _operatorAgent.EndCallEvent();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке завершения звонка");
			}
		}

		public async Task<OperatorResult> Disconnect()
		{
			try
			{
				await _operatorAgent.Disconnect();
				return new OperatorResult(GetResultContent());
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке отключения оператора {OperatorId}", _operatorAgent.OperatorId);
				return new OperatorResult(GetResultContent(), ex.Message);
			}
		}

		public async Task KeepAlive()
		{
			try
			{
				await _operatorAgent.KeepAlive();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке вызова KeepAlive оператора {OperatorId}", _operatorAgent.OperatorId);
			}
		}

		private async Task CheckConnection()
		{
			if(_operatorAgent.OperatorState.State == OperatorStateType.Disconnected)
			{
				await _operatorAgent.Connect();
			}
		}

		private OperatorStateEvent GetResultContent()
		{
			var content = new OperatorStateEvent
			{
				State = _operatorAgent.OperatorState,
				BreakAvailability = _operatorAgent.BreakAvailability,
			};
			return content;
		}

		public void Dispose()
		{
			_operatorAgent.Dispose();
		}
	}
}
