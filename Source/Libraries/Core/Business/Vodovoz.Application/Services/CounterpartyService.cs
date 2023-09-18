using Dadata.Model;
using QS.DomainModel.UoW;
using RevenueService.Client;
using RevenueService.Client.Dto;
using RevenueService.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.Extensions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Application.Services
{
	public class CounterpartyService : ICounterpartyService
	{
		private readonly IRevenueServiceClient _revenueServiceClient;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public CounterpartyService(IRevenueServiceClient revenueServiceClient, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_revenueServiceClient = revenueServiceClient ?? throw new ArgumentNullException(nameof(revenueServiceClient));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
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
				counterparty.CloseDelivery(employee);
				counterparty.AddCloseDeliveryComment($"Автоматическое закрытие поставок: контрагент в статусе \"{status.GetUserFriendlyName()}\" в ФНС. Оформление заказа невозможно.", employee);
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
	}
}
