using System;
using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto.Edo;
using CustomerAppsApi.Library.Errors;
using CustomerAppsApi.Library.Repositories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
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
		private readonly ILogger<CustomerAppEdoService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly ICustomerAppEdoOperatorRepository _customerAppEdoOperatorRepository;
		private readonly IGenericRepository<CounterpartyEdoAccount> _counterpartyEdoAccountRepository;
		private readonly IGenericRepository<EdoOperator> _edoOperatorRepository;

		public CustomerAppEdoService(
			ILogger<CustomerAppEdoService> logger,
			IUnitOfWork unitOfWork,
			IOrganizationSettings organizationSettings,
			ICustomerAppEdoOperatorRepository customerAppEdoOperatorRepository,
			IGenericRepository<CounterpartyEdoAccount> counterpartyEdoAccountRepository,
			IGenericRepository<EdoOperator> edoOperatorRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_customerAppEdoOperatorRepository =
				customerAppEdoOperatorRepository ?? throw new ArgumentNullException(nameof(customerAppEdoOperatorRepository));
			_counterpartyEdoAccountRepository =
				counterpartyEdoAccountRepository ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountRepository));
			_edoOperatorRepository = edoOperatorRepository ?? throw new ArgumentNullException(nameof(edoOperatorRepository));
		}

		/// <inheritdoc/>
		public Result AddEdoAccount(AddingEdoAccount dto)
		{
			var counterparty = _unitOfWork.GetById<Counterparty>(dto.ErpCounterpartyId);

			if(counterparty is null)
			{
				_logger.LogWarning("Не нашли юр лица с таким Id {LegalCounterpartyId}", dto.ErpCounterpartyId);
				return Result.Failure(CounterpartyErrors.CounterpartyNotExists());
			}

			var edoAccount = _counterpartyEdoAccountRepository
				.GetFirstOrDefault(
					_unitOfWork,
					x => x.Counterparty.Id == dto.ErpCounterpartyId
						&& x.OrganizationId == _organizationSettings.VodovozOrganizationId);

			if(edoAccount != null)
			{
				_logger.LogWarning("У клиента с Id {LegalCounterpartyId} уже есть ЭДО аккаунт {EdoAccount} по ВВ",
					dto.ErpCounterpartyId,
					edoAccount.PersonalAccountIdInEdo);
				return Result.Failure(EdoAccountErrors.CounterpartyHasEdoAccount());
			}

			edoAccount = _counterpartyEdoAccountRepository
				.GetFirstOrDefault(
					_unitOfWork,
					x => x.Counterparty.Id == dto.ErpCounterpartyId
						&& x.OrganizationId == _organizationSettings.VodovozOrganizationId
						&& string.Equals(x.PersonalAccountIdInEdo, dto.EdoAccount, StringComparison.OrdinalIgnoreCase));

			var upperProvidedEdoAccount = dto.EdoAccount.ToUpper();
			
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
				_logger.LogWarning("У клиента с Id {LegalCounterpartyId} уже есть ЭДО аккаунт {EdoAccount} по ВВ",
					dto.ErpCounterpartyId,
					edoAccount.PersonalAccountIdInEdo);
				return Result.Failure(EdoAccountErrors.EdoAccountExists());
			}
			
			return Result.Success();
		}

		/// <inheritdoc/>
		public IEnumerable<EdoOperatorDto> GetEdoOperators()
		{
			return _customerAppEdoOperatorRepository.GetAllEdoOperators(_unitOfWork);
		}
	}
}
