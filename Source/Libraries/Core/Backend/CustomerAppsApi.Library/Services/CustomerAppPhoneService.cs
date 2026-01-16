using System;
using System.Linq;
using CustomerAppsApi.Library.Dto.Phones;
using CustomerAppsApi.Library.Errors;
using CustomerAppsApi.Library.Repositories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Contacts;

namespace CustomerAppsApi.Library.Services
{
	/// <summary>
	/// Сервис по работе с телефонами
	/// </summary>
	public class CustomerAppPhoneService
	{
		private readonly ILogger<CustomerAppPhoneService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICounterpartyServiceDataHandler _counterpartyServiceDataHandler;

		public CustomerAppPhoneService(
			ILogger<CustomerAppPhoneService> logger,
			IUnitOfWork unitOfWork,
			ICounterpartyServiceDataHandler counterpartyServiceDataHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_counterpartyServiceDataHandler =
				counterpartyServiceDataHandler ?? throw new ArgumentNullException(nameof(counterpartyServiceDataHandler));
		}
		
		public Result AddPhoneToCounterparty(AddingPhoneNumberDto dto)
		{
			var activationStates =
				_counterpartyServiceDataHandler.GetOnlineLegalCounterpartyActivations(_unitOfWork, dto);

			if(!activationStates.Any())
			{
				_logger.LogWarning("Попытка добавить телефон юр лицу без онлайн активации");
				return Result.Failure(LegalCounterpartyActivationErrors.ActivationNotExists());
			}
			
			if(activationStates.Count() > 1)
			{
				_logger.LogWarning("Найдено несколько одинаковых аккаунтов при попытке добавить телефон юр лицу");
				return Result.Failure(LegalCounterpartyActivationErrors.MoreThanOneExternalLegalCounterpartyAccounts());
			}

			var activationState = activationStates.First();
			var counterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_unitOfWork, dto.CounterpartyErpId);

			if(activationState.AddingPhoneNumberState == AddingPhoneNumberState.Done)
			{
				_logger.LogWarning("Попытка добавить телефон юр лицу, когда он уже добавлен");
				return Result.Failure(LegalCounterpartyActivationErrors.ActivationInWrongState());
			}
			
			if(!counterpartyExists)
			{
				_logger.LogWarning("Не найдено юр лицо с Id {LegalCounterpartyId}", dto.CounterpartyErpId);
				return Result.Failure(CounterpartyErrors.CounterpartyNotExists());
			}

			var digitsNumber = dto.PhoneNumber.TrimStart('7');
			var phoneExists = _counterpartyServiceDataHandler.PhoneExists(_unitOfWork, dto.CounterpartyErpId, digitsNumber);
			
			if(phoneExists)
			{
				_logger.LogWarning("Попытка добавить телефон юр лицу, когда он уже добавлен");
				return Result.Failure(PhoneErrors.PhoneExists());
			}

			activationState.AddingPhoneNumberState = AddingPhoneNumberState.Done;
			var phone = Phone.Create(dto.CounterpartyErpId, dto.PhoneNumber);
			_unitOfWork.Save(phone);
			_unitOfWork.Save(activationState);
			_unitOfWork.Commit();
			
			return Result.Success();
		}
	}
}
