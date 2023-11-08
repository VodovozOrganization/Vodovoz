using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Commands;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public class OperatorController : IDisposable
	{
		private readonly ILogger<OperatorController> _logger;
		private readonly OperatorAgent _operatorAgent;
		private readonly IPhoneController _phoneController;

		public event EventHandler<int> OnDisconnect;

		public OperatorController(ILogger<OperatorController> logger, OperatorAgent operatorAgent, IPhoneController phoneController)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_operatorAgent = operatorAgent ?? throw new ArgumentNullException(nameof(operatorAgent));
			_phoneController = phoneController ?? throw new ArgumentNullException(nameof(phoneController));

			operatorAgent.OnDisconnect += OperatorAgentOnDisconnect;
		}

		private void OperatorAgentOnDisconnect(object sender, int operatorId)
		{
			OnDisconnect?.Invoke(sender, operatorId);
		}

		public async Task<OperatorResult> Connect()
		{
			try
			{
				await _operatorAgent.Connect();
				return new OperatorResult(_operatorAgent.OperatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке подключения оператора");
				return new OperatorResult(_operatorAgent.OperatorState, ex.Message);
			}
		}

		public async Task<OperatorResult> StartWorkShift(string phoneNumber)
		{
			try
			{
				if(!_phoneController.ValidatePhone(phoneNumber))
				{
					return new OperatorResult(_operatorAgent.OperatorState, $"Неизвестный номер телефона {phoneNumber}");
				}

				if(!_phoneController.CanAssign(phoneNumber, _operatorAgent.OperatorId))
				{
					return new OperatorResult(_operatorAgent.OperatorState, $"Номер телефона {phoneNumber}, уже используется другим оператором");
				}

				if(!_operatorAgent.CanChangedBy(OperatorTrigger.StartWorkShift))
				{
					return new OperatorResult(_operatorAgent.OperatorState, $"В данный момент нельзя начать смену");
				}

				await _operatorAgent.StartWorkShift(phoneNumber);
				return new OperatorResult(_operatorAgent.OperatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором начать рабочую смену.");
				return new OperatorResult(_operatorAgent.OperatorState, ex.Message);
			}
		}

		public async Task<OperatorResult> EndWorkShift()
		{
			try
			{
				if(!_operatorAgent.CanChangedBy(OperatorTrigger.EndWorkShift))
				{
					return new OperatorResult(_operatorAgent.OperatorState, $"В данный момент нельзя завершить смену");
				}

				await _operatorAgent.EndWorkShift();
				return new OperatorResult(_operatorAgent.OperatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором завершить рабочую смену.");
				return new OperatorResult(_operatorAgent.OperatorState, ex.Message);
			}
		}

		public async Task<OperatorResult> StartBreak()
		{
			try
			{
				if(!_operatorAgent.CanChangedBy(OperatorTrigger.StartBreak))
				{
					return new OperatorResult(_operatorAgent.OperatorState, $"В данный момент нельзя начать перерыв");
				}

				await _operatorAgent.StartBreak();
				return new OperatorResult(_operatorAgent.OperatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором начать перерыв.");
				return new OperatorResult(_operatorAgent.OperatorState, ex.Message);
			}
		}

		public async Task<OperatorResult> EndBreak()
		{
			try
			{
				if(!_operatorAgent.CanChangedBy(OperatorTrigger.EndBreak))
				{
					return new OperatorResult(_operatorAgent.OperatorState, $"В данный момент нельзя завершить перерыв");
				}

				await _operatorAgent.EndBreak();
				return new OperatorResult(_operatorAgent.OperatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором завершить перерыв.");
				return new OperatorResult(_operatorAgent.OperatorState, ex.Message);
			}
		}

		public async Task<OperatorResult> ChangePhone(string phoneNumber)
		{
			try
			{
				if(!_phoneController.ValidatePhone(phoneNumber))
				{
					return new OperatorResult(_operatorAgent.OperatorState, $"Неизвестный номер телефона {phoneNumber}");
				}

				if(!_phoneController.CanAssign(phoneNumber, _operatorAgent.OperatorId))
				{
					return new OperatorResult(_operatorAgent.OperatorState, $"Номер телефона {phoneNumber}, уже используется другим оператором");
				}

				if(!_operatorAgent.CanChangedBy(OperatorTrigger.ChangePhone))
				{
					return new OperatorResult(_operatorAgent.OperatorState, $"В данный момент нельзя сменить номер телефона");
				}

				await _operatorAgent.ChangePhone(phoneNumber);
				return new OperatorResult(_operatorAgent.OperatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке оператором завершить рабочую смену.");
				return new OperatorResult(_operatorAgent.OperatorState, ex.Message);
			}
		}

		public async Task<OperatorResult> Disconnect()
		{
			try
			{
				await _operatorAgent.Disconnect();
				return new OperatorResult(_operatorAgent.OperatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при попытке отключения оператора");
				return new OperatorResult(_operatorAgent.OperatorState, ex.Message);
			}
		}

		public void Dispose()
		{
			_operatorAgent.Dispose();
		}
	}
}
