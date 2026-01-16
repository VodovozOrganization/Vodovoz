using System;
using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto.Edo;
using CustomerAppsApi.Library.Errors;
using CustomerAppsApi.Library.Extensions;
using CustomerAppsApi.Library.Repositories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Client;

namespace CustomerAppsApi.Library.Services
{
	/// <summary>
	/// Сервис по работе с параметрами ЭДО у клиента
	/// </summary>
	public class CustomerAppEdoService
	{
		private readonly ILogger<CustomerAppPhoneService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly ICounterpartyServiceDataHandler _counterpartyServiceDataHandler;
		private readonly ICustomerAppEdoOperatorRepository _customerAppEdoOperatorRepository;
		private readonly IGenericRepository<CounterpartyEdoAccount> _counterpartyEdoAccountRepository;
		private readonly IGenericRepository<EdoOperator> _edoOperatorRepository;

		public CustomerAppEdoService(
			ILogger<CustomerAppPhoneService> logger,
			IUnitOfWork unitOfWork,
			IOrganizationSettings organizationSettings,
			ICounterpartyServiceDataHandler counterpartyServiceDataHandler,
			ICustomerAppEdoOperatorRepository customerAppEdoOperatorRepository,
			IGenericRepository<CounterpartyEdoAccount> counterpartyEdoAccountRepository,
			IGenericRepository<EdoOperator> edoOperatorRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_counterpartyServiceDataHandler =
				counterpartyServiceDataHandler ?? throw new ArgumentNullException(nameof(counterpartyServiceDataHandler));
			_customerAppEdoOperatorRepository = customerAppEdoOperatorRepository ?? throw new ArgumentNullException(nameof(customerAppEdoOperatorRepository));
			_counterpartyEdoAccountRepository =
				counterpartyEdoAccountRepository ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountRepository));
			_edoOperatorRepository = edoOperatorRepository ?? throw new ArgumentNullException(nameof(edoOperatorRepository));
		}

		/// <inheritdoc/>
		public Result UpdateCounterpartyPurposeOfPurchase(UpdatingCounterpartyPurposeOfPurchase dto)
		{
			var activationStates = _counterpartyServiceDataHandler.GetOnlineLegalCounterpartyActivations(_unitOfWork, dto);

			if(!activationStates.Any())
			{
				_logger.LogWarning("Попытка обновить причину покупки воды, для организации без онлайн активации");
				return Result.Failure(LegalCounterpartyActivationErrors.ActivationNotExists());
			}
			
			if(activationStates.Count() > 1)
			{
				_logger.LogWarning("Найдено несколько одинаковых аккаунтов при попытке обновить причину покупки воды");
				return Result.Failure(LegalCounterpartyActivationErrors.MoreThanOneExternalLegalCounterpartyAccounts());
			}

			var activationState = activationStates.First();
			
			if(activationState.AddingReasonForLeavingState == AddingReasonForLeavingState.Done)
			{
				_logger.LogWarning("Попытка обновить причину покупки воды, когда она уже обновлена");
				return Result.Failure(LegalCounterpartyActivationErrors.ActivationInWrongState());
			}
			
			var counterparty = _unitOfWork.GetById<Counterparty>(dto.ErpCounterpartyId);

			if(counterparty is null)
			{
				return Result.Failure(CounterpartyErrors.CounterpartyNotExists());
			}

			var reasonForLeaving = dto.WaterPurposeOfPurchase.ToReasonForLeaving();
			counterparty.ReasonForLeaving = reasonForLeaving;
			activationState.AddingReasonForLeavingState = AddingReasonForLeavingState.Done;
			
			_unitOfWork.Save(counterparty);
			_unitOfWork.Save(activationState);
			_unitOfWork.Commit();
			
			return Result.Success();
		}

		/// <inheritdoc/>
		public Result AddEdoAccount(AddingEdoAccount dto)
		{
			var activationStates =
				_counterpartyServiceDataHandler.GetOnlineLegalCounterpartyActivations(_unitOfWork, dto);

			if(!activationStates.Any())
			{
				_logger.LogWarning("Попытка добавить ЭДО аккаунт, для организации без онлайн активации");
				return Result.Failure(LegalCounterpartyActivationErrors.ActivationNotExists());
			}
			
			if(activationStates.Count() > 1)
			{
				_logger.LogWarning("Найдено несколько одинаковых аккаунтов при попытке добавить ЭДО аккаунт");
				return Result.Failure(LegalCounterpartyActivationErrors.MoreThanOneExternalLegalCounterpartyAccounts());
			}

			var activationState = activationStates.First();

			if(activationState.AddingEdoAccountState != AddingEdoAccountState.NeedAdd)
			{
				_logger.LogWarning("Попытка добавить ЭДО аккаунт, не в том состоянии");
				return Result.Failure(LegalCounterpartyActivationErrors.ActivationInWrongState());
			}

			var counterparty = _unitOfWork.GetById<Counterparty>(dto.ErpCounterpartyId);

			if(counterparty is null)
			{
				return Result.Failure(CounterpartyErrors.CounterpartyNotExists());
			}

			var edoAccount = _counterpartyEdoAccountRepository
				.GetFirstOrDefault(
					_unitOfWork,
					x => x.Counterparty.Id == dto.ErpCounterpartyId
						&& string.Equals(x.PersonalAccountIdInEdo, dto.EdoAccount, StringComparison.OrdinalIgnoreCase));

			var upperProvidedEdoAccount = dto.EdoAccount.ToUpper();
			//TODO 5608: как поступаем, если пользователь ДВ внес данные по ЭДО аккаунту?
			//TODO 5608: должны ли мы сохранять предыдущий введенный ЭДО аккаунт? Ведь пользователь может ввести несколько аккаунтов или хотя бы ограничить их количество?
			if(edoAccount is null)
			{
				var edoOperator = upperProvidedEdoAccount[..3];
				
				var savedEdoOperator = _edoOperatorRepository
					.Get(_unitOfWork, x => x.Code == edoOperator)
					.FirstOrDefault();

				if(savedEdoOperator is null)
				{
					savedEdoOperator = EdoOperator.Create(edoOperator, "Не известно", "Не известно");
					_unitOfWork.Save(savedEdoOperator);
				}

				edoAccount = CounterpartyEdoAccount.Create(
					counterparty,
					savedEdoOperator,
					dto.EdoAccount,
					_organizationSettings.VodovozOrganizationId,
					counterparty.DefaultEdoAccount(_organizationSettings.VodovozOrganizationId) != null);
				
				_unitOfWork.Save(edoAccount);
				_unitOfWork.Commit();
			}
			else
			{
				if(edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
				{
					if(activationState.AddingEdoAccountState != AddingEdoAccountState.Done)
					{
						activationState.AddingEdoAccountState = AddingEdoAccountState.Done;
						_unitOfWork.Save(edoAccount);
						_unitOfWork.Commit();
					}
					return Result.Failure(EdoAccountErrors.EdoAccountExists());
				}

				if(edoAccount.ConsentForEdoStatus != ConsentForEdoStatus.Agree)
				{
					//TODO 5608:отправка http кода для существующего аккаунта без согласия по ЭДО
				}
			}
			
			//TODO 5608: обновление данных активации?
			
			return Result.Success();
		}

		/// <inheritdoc/>
		public IEnumerable<EdoOperatorDto> GetEdoOperators()
		{
			return _customerAppEdoOperatorRepository.GetAllEdoOperators(_unitOfWork);
		}
	}
}
