using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dadata.Model;
using QS.DomainModel.UoW;
using RevenueService.Client;
using RevenueService.Client.Dto;
using RevenueService.Client.Extensions;
using Vodovoz.Application.Extensions;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Application.Clients.Services
{
	public class CounterpartyService : ICounterpartyService
	{
		private readonly IRevenueServiceClient _revenueServiceClient;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;

		public CounterpartyService(
			IRevenueServiceClient revenueServiceClient,
			IUnitOfWorkFactory unitOfWorkFactory,
			IGenericRepository<Counterparty> counterpartyRepository)
		{
			_revenueServiceClient = revenueServiceClient
				?? throw new ArgumentNullException(nameof(revenueServiceClient));
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_counterpartyRepository = counterpartyRepository
				?? throw new ArgumentNullException(nameof(counterpartyRepository));
		}

		public async Task<IEnumerable<CounterpartyRevenueServiceInfo>> GetRevenueServiceInformation(
			string inn,
			string kpp,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(inn))
			{
				return Enumerable.Empty<CounterpartyRevenueServiceInfo>();
			}

			var request = new DadataRequestDto
			{
				Inn = inn,
			};

			if(!string.IsNullOrWhiteSpace(kpp))
			{
				request.Kpp = kpp;
			}

			var result = await _revenueServiceClient.GetCounterpartyInfoAsync(request, cancellationToken);

			if(!string.IsNullOrWhiteSpace(result.ErrorMessage))
			{
				return Enumerable.Empty<CounterpartyRevenueServiceInfo>();
			}

			return result.CounterpartyDetailsList.ToCounterpartyRevenueServiceInfo();
		}

		public async Task StopShipmentsIfNeeded(Counterparty counterparty, Employee employee, CancellationToken cancellationToken)
		{
			if(counterparty.IsLiquidating
				|| counterparty.IsDeliveriesClosed
				|| counterparty.PersonType != PersonType.legal
				|| string.IsNullOrWhiteSpace(counterparty.INN))
			{
				return;
			}

			var status = await _revenueServiceClient
				.GetCounterpartyStatus(counterparty.INN, counterparty.KPP, cancellationToken);

			if(status != PartyStatus.ACTIVE)
			{
				counterparty.IsLiquidating = true;
				StopShipmentsIfNeeded(counterparty, employee, true, status.GetUserFriendlyName());
			}
		}

		public async Task StopShipmentsIfNeeded(int counterpartyId, int employeeId, CancellationToken cancellationToken)
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateForRoot<Counterparty>(counterpartyId, "Автоматическое закрытие поставок: контрагент в статусе ликвидации"))
			{
				var employee = unitOfWork.GetById<Employee>(employeeId);

				await StopShipmentsIfNeeded(unitOfWork.Root, employee, cancellationToken);

				unitOfWork.Save();
			}
		}

		public void StopShipmentsIfNeeded(Counterparty counterparty, Employee employee, bool isLiquidating, string statusName)
		{
			if(!isLiquidating)
			{
				return;
			}

			if(counterparty.IsDeliveriesClosed)
			{
				return;
			}

			counterparty.CloseDelivery(employee);
			counterparty.AddCloseDeliveryComment($"Автоматическое закрытие поставок: контрагент в статусе \"{statusName}\" в ФНС. Оформление заказа невозможно.", employee);
		}

		public void UpdateDetailsFromRevenueServiceInfoIfNeeded(
			int counterpartyId,
			CounterpartyRevenueServiceInfo revenueServiceInfo)
		{
			var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Обновление сведений Контрагентов из ФНС");

			var counterparty = unitOfWork.GetById<Counterparty>(counterpartyId);

			if(revenueServiceInfo is null)
			{
				throw new ArgumentNullException(nameof(revenueServiceInfo));
			}

			if(string.IsNullOrWhiteSpace(counterparty.Name))
			{
				counterparty.Name = revenueServiceInfo.Name;
			}

			if(string.IsNullOrWhiteSpace(counterparty.FullName))
			{
				counterparty.FullName = revenueServiceInfo.FullName;
			}

			if(string.IsNullOrWhiteSpace(counterparty.RawJurAddress))
			{
				counterparty.RawJurAddress = revenueServiceInfo.LegalAddress;
			}

			if(string.IsNullOrWhiteSpace(counterparty.TypeOfOwnership))
			{
				counterparty.TypeOfOwnership = revenueServiceInfo.TypeOfOwnership;
			}

			if(string.IsNullOrWhiteSpace(counterparty.SignatoryFIO))
			{
				counterparty.SignatoryFIO = revenueServiceInfo.SignatoryFIO;
			}

			if(string.IsNullOrWhiteSpace(counterparty.Surname))
			{
				counterparty.Surname = revenueServiceInfo.Surname;
			}

			if(string.IsNullOrWhiteSpace(counterparty.FirstName))
			{
				counterparty.FirstName = revenueServiceInfo.FirstName;
			}

			if(string.IsNullOrWhiteSpace(counterparty.Patronymic))
			{
				counterparty.Patronymic = revenueServiceInfo?.Patronymic;
			}

			var existingEmails = counterparty.Emails.Select(x => x.Address);

			var enailsToAdd = revenueServiceInfo.Emails.Where(x => !existingEmails.Contains(x));

			foreach(var emailToAdd in enailsToAdd)
			{
				counterparty.Emails.Add(
					new Domain.Contacts.Email
					{
						Address = emailToAdd,
						Counterparty = counterparty
					});
			}

			var existingPhones = counterparty.Phones.Select(x => x.Number);

			var phonesToAdd = revenueServiceInfo.Phones.Where(x => !existingPhones.Contains(x));

			foreach(var phoneToAdd in phonesToAdd)
			{
				counterparty.Phones.Add(new Domain.Contacts.Phone
					{
						Number = phoneToAdd,
						Counterparty = counterparty
					});
			}

			unitOfWork.Commit();
		}

		public IEnumerable<Counterparty> GetByNormalizedPhoneNumber(IUnitOfWork unitOfWork, string normalizedPhone)
		{
			if(normalizedPhone != normalizedPhone.NormalizePhone())
			{
				throw new ArgumentException("В аргумент передан не нормализованный телефон", nameof(normalizedPhone));
			}

			return _counterpartyRepository
				.Get(unitOfWork, c => c.Phones.Any(p => p.DigitsNumber == normalizedPhone));
		}
	}
}
