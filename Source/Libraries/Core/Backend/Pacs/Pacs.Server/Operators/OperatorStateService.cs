using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Commands;
using Pacs.Core.Messages.Events;
using Pacs.Server.Breaks;
using Pacs.Server.Phones;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Core.Domain.Repositories;

namespace Pacs.Server.Operators
{
	public class OperatorStateService : IOperatorStateService
	{
		private readonly ILogger<OperatorStateService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGlobalBreakController _globalBreakController;
		private readonly IOperatorBreakAvailabilityService _operatorBreakAvailabilityService;
		private readonly IPacsRepository _pacsRepository;
		private readonly IOperatorPhoneService _operatorPhoneService;
		private readonly IGenericRepository<Operator> _operatorRepository;
		private readonly IGenericRepository<OperatorState> _operatorStateRepository;

		public OperatorStateService(
			ILogger<OperatorStateService> logger,
			IUnitOfWork unitOfWork,
			IGlobalBreakController globalBreakController,
			IOperatorBreakAvailabilityService operatorBreakAvailabilityService,
			IPacsRepository pacsRepository,
			IOperatorPhoneService phoneController,
			IGenericRepository<Operator> operatorRepository,
			IGenericRepository<OperatorState> operatorStateRepository)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_globalBreakController = globalBreakController
				?? throw new ArgumentNullException(nameof(globalBreakController));
			_operatorBreakAvailabilityService = operatorBreakAvailabilityService
				?? throw new ArgumentNullException(nameof(operatorBreakAvailabilityService));
			_pacsRepository = pacsRepository
				?? throw new ArgumentNullException(nameof(pacsRepository));
			_operatorPhoneService = phoneController
				?? throw new ArgumentNullException(nameof(phoneController));
			_operatorRepository = operatorRepository
				?? throw new ArgumentNullException(nameof(operatorRepository));
			_operatorStateRepository = operatorStateRepository
				?? throw new ArgumentNullException(nameof(operatorStateRepository));
		}

		#region Подключение/отключение оператора

		public async Task<OperatorResult> Connect(int operatorId)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не найден");
					}

					if(!@operator.PacsEnabled)
					{
						return new OperatorResult(null, $"Оператор {operatorId} отключен от СКУД");
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						lastOperatorState = OperatorState.CreateNewForOperator(operatorId);
					}

					if(lastOperatorState.State != OperatorStateType.Disconnected
						&& lastOperatorState.State != OperatorStateType.New)
					{
						return new OperatorResult(null, "Оператор уже подключен");
					}

					var newState = lastOperatorState.Copy();

					newState.State = OperatorStateType.Connected;
					newState.Started = DateTime.Now;
					newState.Trigger = OperatorTrigger.Connect;

					lastOperatorState.Ended = newState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newState);

					transaction.Commit();

					_logger.LogInformation("Подключение оператора {OperatorId}", operatorId);

					return await Task.FromResult(new OperatorResult(GetResultContent(newState)));
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке подключения оператора");

					if(transaction?.IsActive ?? false)
					{
						transaction?.Rollback();
					}

					return new OperatorResult(null, ex.Message);
				}
			}
		}

		public async Task<OperatorResult> Disconnect(int operatorId)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не найден");
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не подключен");
					}

					if(lastOperatorState.State == OperatorStateType.Disconnected)
					{
						return new OperatorResult(null, $"Оператор {operatorId} уже отключен");
					}

					var newState = lastOperatorState.Copy();
					newState.State = OperatorStateType.Disconnected;
					newState.Started = DateTime.Now;
					newState.Trigger = OperatorTrigger.Disconnect;

					lastOperatorState.Ended = newState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newState);

					transaction.Commit();

					_logger.LogInformation("Отключение оператора {OperatorId}", operatorId);

					return await Task.FromResult(new OperatorResult(GetResultContent(newState)));
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке отключения оператора {OperatorId}", operatorId);

					if(transaction?.IsActive ?? false)
					{
						transaction?.Rollback();
					}

					return new OperatorResult(null, ex.Message);
				}
			}
		}

		#endregion Подключение/отключение оператора

		#region Начало/завершение рабочей смены

		public async Task<OperatorResult> StartWorkShift(int operatorId, string phoneNumber)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!_operatorPhoneService.PhoneExists(phoneNumber))
					{
						return new OperatorResult(null, $"Неизвестный номер телефона {phoneNumber}");
					}

					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не найден");
					}

					if(!@operator.PacsEnabled)
					{
						return new OperatorResult(null, $"Оператор {operatorId} отключен от СКУД");
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState)
						|| lastOperatorState.State != OperatorStateType.Connected)
					{
						return new OperatorResult(null, $"Оператор {operatorId} не подключен");
					}

					var currentAssignedOperatorId = _operatorPhoneService.GetAssignedOperator(phoneNumber);

					if(currentAssignedOperatorId != null
						&& currentAssignedOperatorId != operatorId)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), $"Номер телефона {phoneNumber}, уже используется другим оператором");
					}

					if(lastOperatorState.State != OperatorStateType.Connected)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя начать смену в текущем состоянии");
					}

					var newOperatorState = lastOperatorState.Copy();

					newOperatorState.State = OperatorStateType.WaitingForCall;
					newOperatorState.PhoneNumber = phoneNumber;
					newOperatorState.Started = DateTime.Now;
					newOperatorState.Trigger = OperatorTrigger.StartWorkShift;

					newOperatorState.WorkShift = OperatorWorkshift.Create(operatorId, newOperatorState.Started, @operator.WorkShift);

					lastOperatorState.Ended = newOperatorState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newOperatorState);

					transaction.Commit();

					_logger.LogInformation("Начало рабочей смены {OperatorWorkshiftId} оператора {OperatorId} в {WorkShiftStartedAt}",
						newOperatorState.WorkShift.Id,
						operatorId,
						newOperatorState.Started);

					return await Task.FromResult(new OperatorResult(GetResultContent(newOperatorState)));
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке оператором начать рабочую смену.");

					if(transaction?.IsActive ?? false)
					{
						transaction?.Rollback();
					}

					return new OperatorResult(null, ex.Message);
				}
			}
		}

		public async Task<OperatorResult> EndWorkShift(int operatorId, string reason)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не найден");
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не подключен");
					}

					if(lastOperatorState.State != OperatorStateType.WaitingForCall)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя завершить смену в текущем состоянии");
					}

					if(lastOperatorState.WorkShift.GetPlannedEndTime() > DateTime.Now
						&& string.IsNullOrWhiteSpace(reason))
					{
						return new OperatorResult(GetResultContent(lastOperatorState), $"Необходимо указать причину закрытия смены, если завершается раньше планируемого");
					}

					var newOperatorState = lastOperatorState.Copy();

					newOperatorState.State = OperatorStateType.Connected;
					newOperatorState.Started = DateTime.Now;
					newOperatorState.Trigger = OperatorTrigger.EndWorkShift;
					newOperatorState.PhoneNumber = null;

					newOperatorState.WorkShift.Reason = reason;

					lastOperatorState.Ended = newOperatorState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newOperatorState);

					transaction.Commit();

					_logger.LogInformation("Завершение рабочей смены оператора {OperatorId}", operatorId);

					return await Task.FromResult(new OperatorResult(GetResultContent(newOperatorState)));
				}
				catch(Exception ex)
				{

					_logger.LogError(ex, "Произошло исключение при попытке оператором завершить рабочую смену.");

					if(transaction?.IsActive ?? false)
					{
						transaction?.Rollback();
					}

					return new OperatorResult(null, ex.Message);
				}
			}
		}

		#endregion Начало/завершение рабочей смены

		public async Task<OperatorResult> ChangePhone(int operatorId, string phoneNumber)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!_operatorPhoneService.PhoneExists(phoneNumber))
					{
						return new OperatorResult(null, $"Неизвестный номер телефона {phoneNumber}");
					}

					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не найден");
					}

					if(!@operator.PacsEnabled)
					{
						return new OperatorResult(null, $"Оператор {operatorId} отключен от СКУД");
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не подключен");
					}

					if(lastOperatorState.State != OperatorStateType.WaitingForCall)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя сменить номер телефона в текущем состоянии");
					}

					if(lastOperatorState.PhoneNumber == phoneNumber)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), $"Номер телефона {phoneNumber}, уже назначен текущему оператору");
					}

					var currentAssignedOperator = _operatorPhoneService.GetAssignedOperator(phoneNumber);

					if(currentAssignedOperator != null)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), $"Номер телефона {phoneNumber}, уже используется другим оператором");
					}

					var newOperatorState = lastOperatorState.Copy();
					newOperatorState.PhoneNumber = phoneNumber;
					newOperatorState.State = OperatorStateType.WaitingForCall;
					newOperatorState.Started = DateTime.Now;
					newOperatorState.Trigger = OperatorTrigger.ChangePhone;

					lastOperatorState.Ended = newOperatorState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newOperatorState);

					transaction.Commit();

					_logger.LogInformation("Смена номера телефона оператора {OperatorId} на {PhoneNumber}", operatorId, phoneNumber);

					return await Task.FromResult(new OperatorResult(GetResultContent(newOperatorState)));
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке оператором завершить рабочую смену.");

					if(transaction?.IsActive ?? false)
					{
						transaction?.Rollback();
					}

					return new OperatorResult(null, ex.Message);
				}
			}
		}

		#region Начало/завершение перерыва

		public async Task<OperatorResult> StartBreak(int operatorId, OperatorBreakType breakType)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не найден");
					}

					if(!@operator.PacsEnabled)
					{
						return new OperatorResult(null, $"Оператор {operatorId} отключен от СКУД");
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не подключен");
					}

					if(lastOperatorState.State == OperatorStateType.Talk)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя начать перерыв во время разговора");
					}
					else if(lastOperatorState.State == OperatorStateType.Break)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя начать перерыв, вы уже на перерыве");
					}
					else if(lastOperatorState.State == OperatorStateType.New
						|| lastOperatorState.State == OperatorStateType.Disconnected
						|| lastOperatorState.State == OperatorStateType.Connected)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя начать перерыв, вы не на смене");
					}

					if(lastOperatorState.State != OperatorStateType.WaitingForCall)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя начать перерыв в текущем состоянии");
					}

					var checkResult = GetCheckStartBreakResult(lastOperatorState, breakType);

					if(checkResult != null)
					{
						return checkResult;
					}

					var newOperatorState = lastOperatorState.Copy();

					newOperatorState.State = OperatorStateType.Break;
					newOperatorState.BreakType = breakType;
					newOperatorState.Started = DateTime.Now;
					newOperatorState.Trigger = OperatorTrigger.StartBreak;
					newOperatorState.BreakChangedBy = BreakChangedBy.Operator;

					lastOperatorState.Ended = newOperatorState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newOperatorState);

					transaction.Commit();

					// TODO: Оповестить о начале перерыва

					_logger.LogInformation("Начало перерыва оператора {OperatorId}", operatorId);

					return await Task.FromResult(new OperatorResult(GetResultContent(newOperatorState)));
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке оператором начать перерыв.");

					if(transaction?.IsActive ?? false)
					{
						transaction?.Rollback();
					}

					return new OperatorResult(null, ex.Message);
				}
			}
		}

		public async Task<OperatorResult> EndBreak(int operatorId)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не найден");
					}

					if(!@operator.PacsEnabled)
					{
						return new OperatorResult(null, $"Оператор {operatorId} отключен от СКУД");
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не подключен");
					}

					if(lastOperatorState.State != OperatorStateType.Break)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя завершить перерыв в текущем состоянии");
					}

					var newOperatorState = lastOperatorState.Copy();
					newOperatorState.State = OperatorStateType.WaitingForCall;
					newOperatorState.Started = DateTime.Now;
					newOperatorState.Trigger = OperatorTrigger.EndBreak;
					newOperatorState.BreakChangedBy = BreakChangedBy.Operator;

					lastOperatorState.Ended = newOperatorState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newOperatorState);

					transaction.Commit();

					// TODO: Оповестить о завершении перерыва

					_logger.LogInformation("Завершение перерыва оператора {OperatorId}", operatorId);

					return await Task.FromResult(new OperatorResult(GetResultContent(newOperatorState)));
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке оператором завершить перерыв.");

					if(transaction?.IsActive ?? false)
					{
						transaction?.Rollback();
					}

					return new OperatorResult(null, ex.Message);
				}
			}
		}

		#endregion Начало/завершение перерыва

		#region Администраторские действия

		public async Task<OperatorResult> AdminStartBreak(int operatorId, OperatorBreakType breakType, int adminId, string reason)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не найден");
					}

					if(!@operator.PacsEnabled)
					{
						return new OperatorResult(null, $"Оператор {operatorId} отключен от СКУД");
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не подключен");
					}

					if(lastOperatorState.State != OperatorStateType.WaitingForCall)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя начать перерыв в текущем состоянии");
					}

					if(string.IsNullOrWhiteSpace(reason))
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Основание должно быть заполнено");
					}

					var newOperatorState = lastOperatorState.Copy();

					newOperatorState.State = OperatorStateType.Break;
					newOperatorState.Started = DateTime.Now;
					newOperatorState.Trigger = OperatorTrigger.StartBreak;
					newOperatorState.BreakChangedBy = BreakChangedBy.Admin;
					newOperatorState.BreakChangedByAdminId = adminId;
					newOperatorState.BreakAdminReason = reason;

					lastOperatorState.Ended = newOperatorState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newOperatorState);

					transaction.Commit();

					_logger.LogInformation("Администратор {AdminId} начал перерыв оператора {OperatorId} по причине: {Reason}.", adminId, operatorId, reason);

					// TODO: Оповестить о начале перерыва

					return await Task.FromResult(new OperatorResult(GetResultContent(newOperatorState)));
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке администратором {AdminId} начать перерыв оператору {OperatorId}.",
						adminId, operatorId);

					if(transaction?.IsActive ?? false)
					{
						transaction?.Rollback();
					}

					return new OperatorResult(null, ex.Message);
				}
			}
		}

		public async Task<OperatorResult> AdminEndBreak(int operatorId, int adminId, string reason)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не найден");
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не подключен");
					}

					if(lastOperatorState.State != OperatorStateType.Break)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя завершить перерыв в текущем состоянии");
					}

					if(string.IsNullOrWhiteSpace(reason))
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Основание должно быть заполнено");
					}

					var newOperatorState = lastOperatorState.Copy();
					newOperatorState.State = OperatorStateType.WaitingForCall;
					newOperatorState.Started = DateTime.Now;
					newOperatorState.Trigger = OperatorTrigger.EndBreak;
					newOperatorState.BreakChangedBy = BreakChangedBy.Admin;
					newOperatorState.BreakChangedByAdminId = adminId;
					newOperatorState.BreakAdminReason = reason;

					lastOperatorState.Ended = newOperatorState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newOperatorState);

					transaction.Commit();

					_logger.LogInformation("Администратор {AdminId} завершил перерыв оператора {OperatorId} по причине: {Reason}.", adminId, operatorId, reason);

					// TODO: Оповестить о завершении перерыва

					return await Task.FromResult(new OperatorResult(GetResultContent(newOperatorState)));
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке администратором {AdminId} завершить перерыв оператора {OperatorId}.",
						adminId, operatorId);

					return new OperatorResult(null, ex.Message);
				}
			}
		}

		public async Task<OperatorResult> AdminEndWorkShift(int operatorId, int adminId, string reason)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не найден");
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						return new OperatorResult(null, $"Оператор {operatorId} не подключен");
					}

					if(lastOperatorState.State == OperatorStateType.Disconnected
						|| lastOperatorState.State == OperatorStateType.New
						|| lastOperatorState.State == OperatorStateType.Connected)
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Нельзя завершить смену в текущем состоянии");
					}

					if(string.IsNullOrWhiteSpace(reason))
					{
						return new OperatorResult(GetResultContent(lastOperatorState), "Основание должно быть заполнено");
					}

					if(lastOperatorState.State == OperatorStateType.Break)
					{
						_logger.LogWarning("Оператор {OperatorId} находится на перерыве, перерыв будет завершен.", operatorId);
					}

					if(lastOperatorState.State == OperatorStateType.Talk)
					{
						_logger.LogWarning("Оператор {OperatorId} находится в разговоре, разговор будет завершен.", operatorId);
					}

					var newOperatorState = lastOperatorState.Copy();
					newOperatorState.State = OperatorStateType.Connected;
					newOperatorState.Started = DateTime.Now;
					newOperatorState.Trigger = OperatorTrigger.EndWorkShift;
					newOperatorState.BreakChangedBy = BreakChangedBy.Admin;
					newOperatorState.BreakChangedByAdminId = adminId;
					newOperatorState.BreakAdminReason = reason;

					lastOperatorState.Ended = newOperatorState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newOperatorState);

					transaction.Commit();

					_logger.LogInformation("Администратор {AdminId} завершил рабочую смену оператора {OperatorId} по причине: {Reason}.", adminId, operatorId, reason);

					// TODO: Оповестить о завершении перерыва, если он был
					// TODO: Оповестить о завершении смены

					return await Task.FromResult(new OperatorResult(GetResultContent(newOperatorState)));
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке администратором {AdminId} завершить смену оператора {OperatorId}.",
						adminId, operatorId);

					return new OperatorResult(null, ex.Message);
				}
			}
		}

		#endregion Администраторские действия

		#region Начало/завершение звонка

		public async Task TakeCall(string toExtension, string callId)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!_operatorPhoneService.PhoneExists(toExtension))
					{
						_logger.LogWarning("Неизвестный номер телефона {InnerPhoneNumber}", toExtension);
						return;
					}

					var operatorId = _operatorPhoneService.GetAssignedOperator(toExtension);

					if(operatorId == null || operatorId == 0)
					{
						_logger.LogWarning("Номер телефона {InnerPhoneNumber} не назначен ни одному оператору", toExtension);
						return;
					}

					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						_logger.LogWarning("Оператор {OperatorId} не найден", operatorId);
						return;
					}

					if(!@operator.PacsEnabled)
					{
						_logger.LogWarning("Оператор {OperatorId} отключен от СКУД", operatorId);
						return;
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						_logger.LogWarning("Оператор {OperatorId} не подключен", operatorId);
						return;
					}

					if(lastOperatorState.State != OperatorStateType.WaitingForCall)
					{
						_logger.LogWarning("Нельзя принять звонок в текущем состоянии оператора {OperatorId}", operatorId);
						return;
					}

					var callHistory = await _pacsRepository.GetCallHistoryByCallIdAsync(callId);

					if(callHistory.Any(ce => ce.State == CallState.Disconnected))
					{
						_logger.LogWarning("Звонок {CallId} уже завершен", callId);
						return;
					}

					var newOperatorState = lastOperatorState.Copy();
					newOperatorState.State = OperatorStateType.Talk;
					newOperatorState.Started = DateTime.Now;
					newOperatorState.Trigger = OperatorTrigger.TakeCall;
					newOperatorState.CallId = callId;

					lastOperatorState.Ended = newOperatorState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newOperatorState);

					transaction.Commit();

					_logger.LogInformation("Оператор {OperatorId} принял звонок {CallId}", operatorId, callId);

					// TODO: Оповещение о начале звонка

					await Task.CompletedTask;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке принятия звонка");

					if(transaction?.IsActive ?? false)
					{
						transaction?.Rollback();
					}
				}
			}
		}

		public async Task EndCall(string toExtension, string callId)
		{
			using(var transaction = _unitOfWork.Session.BeginTransaction())
			{
				try
				{
					if(!_operatorPhoneService.PhoneExists(toExtension))
					{
						_logger.LogWarning("Неизвестный номер телефона {InnerPhoneNumber}", toExtension);
						return;
					}

					var operatorId = _operatorPhoneService.GetAssignedOperator(toExtension);

					if(operatorId == null || operatorId == 0)
					{
						_logger.LogWarning("Номер телефона {InnerPhoneNumber} не назначен ни одному оператору", toExtension);
						return;
					}

					if(!(_operatorRepository
						.GetFirstOrDefault(
							_unitOfWork,
							x => x.Id == operatorId) is Operator @operator))
					{
						_logger.LogWarning("Оператор {OperatorId} не найден", operatorId);
						return;
					}

					if(!(_operatorStateRepository
						.GetLastOrDefault(
							_unitOfWork,
							x => x.OperatorId == operatorId
								&& x.Ended == null) is OperatorState lastOperatorState))
					{
						_logger.LogError("Оператор {OperatorId} не подключен", operatorId);
						return;
					}

					if(lastOperatorState.State != OperatorStateType.Talk)
					{
						_logger.LogWarning("Нельзя завершить звонок в текущем состоянии оператора {OperatorId}", operatorId);
						return;
					}

					if(lastOperatorState.CallId != callId)
					{
						_logger.LogWarning("Оператор {OperatorId} разговаривает с другим абонентом. Идентификатор звонка{CallId}", operatorId, lastOperatorState.CallId);
						return;
					}

					var newOperatorState = lastOperatorState.Copy();
					newOperatorState.State = OperatorStateType.WaitingForCall;
					newOperatorState.Started = DateTime.Now;
					newOperatorState.Trigger = OperatorTrigger.EndCall;
					newOperatorState.CallId = lastOperatorState.CallId;

					lastOperatorState.Ended = newOperatorState.Started;

					_unitOfWork.Save(lastOperatorState);
					_unitOfWork.Save(newOperatorState);

					transaction.Commit();

					_logger.LogInformation("Оператор {OperatorId} завершил звонок {CallId}", operatorId, callId);

					// TODO: Оповещение о завершении звонка

					await Task.CompletedTask;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошло исключение при попытке завершения звонка");

					if(transaction?.IsActive ?? false)
					{
						transaction?.Rollback();
					}
				}
			}
		}

		#endregion Начало/завершение звонка

		public async Task<OperatorBreakAvailability> GetBreakAvailability(int operatorId)
		{
			return await Task.FromResult(_operatorBreakAvailabilityService.GetBreakAvailability(operatorId));
		}

		private OperatorStateEvent GetResultContent(OperatorState operatorState, [CallerMemberName] string caller = null)
		{
			var currentBreakAviability = _operatorBreakAvailabilityService.GetBreakAvailability(operatorState.OperatorId);

			_logger.LogInformation("Получение контента события, состояние оператора: {OperatorState}, из метода {MethodName}", operatorState.State, caller);

			var content = new OperatorStateEvent
			{
				EventId = Guid.NewGuid(),
				State = operatorState,
				BreakAvailability = currentBreakAviability,
			};

			return content;
		}

		private OperatorResult GetCheckStartBreakResult(OperatorState operatorState, OperatorBreakType breakType)
		{
			string description = "";
			bool canStartGlobal;
			bool canStart;

			var currentBreakAviability = _operatorBreakAvailabilityService.GetBreakAvailability(operatorState.OperatorId);

			if(breakType == OperatorBreakType.Long)
			{
				canStartGlobal = _globalBreakController.BreakAvailability.LongBreakAvailable;

				if(!canStartGlobal)
				{
					description = _globalBreakController.BreakAvailability.LongBreakDescription;
				}

				canStart = currentBreakAviability.LongBreakAvailable;

				if(!canStart)
				{
					description = currentBreakAviability.LongBreakDescription;
				}
			}
			else
			{
				canStartGlobal = _globalBreakController.BreakAvailability.ShortBreakAvailable;

				if(!canStartGlobal)
				{
					description = _globalBreakController.BreakAvailability.ShortBreakDescription;
				}

				canStart = currentBreakAviability.ShortBreakAvailable;

				if(!canStart)
				{
					description = currentBreakAviability.ShortBreakDescription;
				}
			}

			if(!canStartGlobal || !canStart)
			{
				return new OperatorResult(GetResultContent(operatorState), description);
			}

			return null;
		}
	}
}
