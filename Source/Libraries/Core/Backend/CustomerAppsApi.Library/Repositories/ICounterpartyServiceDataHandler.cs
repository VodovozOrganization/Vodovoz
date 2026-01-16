using System;
using System.Collections.Generic;
using CustomerAppsApi.Library.Dto.Edo;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Organizations;

namespace CustomerAppsApi.Library.Repositories
{
	public interface ICounterpartyServiceDataHandler
	{
		ExternalCounterparty GetExternalCounterparty(
			IUnitOfWork uow,
			Guid externalCounterpartyId,
			string phoneNumber,
			CounterpartyFrom counterpartyFrom);
		ExternalCounterparty GetExternalCounterparty(
			IUnitOfWork uow,
			Guid externalCounterpartyId,
			CounterpartyFrom counterpartyFrom);
		ExternalCounterparty GetExternalCounterparty(IUnitOfWork uow, string phoneNumber, CounterpartyFrom counterpartyFrom);
		bool ExternalCounterpartyMatchingExists(IUnitOfWork uow, Guid externalCounterpartyId, string phoneNumber);
		RoboAtsCounterpartyName GetRoboatsCounterpartyName(IUnitOfWork uow, string counterpartyName);
		RoboAtsCounterpartyPatronymic GetRoboatsCounterpartyPatronymic(IUnitOfWork uow, string counterpartyPatronymic);
		int GetCounterpartyBottlesDebt(IUnitOfWork uow, int counterpartyId, int counterpartyDebtCacheMinutes);
		Email GetEmailForExternalCounterparty(IUnitOfWork uow, int counterpartyId);
		EmailType GetEmailTypeForReceipts(IUnitOfWork uow);
		OrganizationOwnershipType GetOrganizationOwnershipTypeByCode(IUnitOfWork uow, string code);
		bool CounterpartyExists(IUnitOfWork uow, int counterpartyId);
		bool CounterpartyExists(IUnitOfWork uow, string inn);
		/// <summary>
		/// Получение адреса почты для связки с аккаунтом юр лица в ИПЗ
		/// </summary>
		/// <param name="uow">Unit Of Work</param>
		/// <param name="legalCounterpartyId">Идентификатор юр лица</param>
		/// <param name="email">Адрес электронной почты</param>
		/// <returns></returns>
		IEnumerable<Email> GetEmailForLinking(IUnitOfWork uow, int legalCounterpartyId, string email);
		/// <summary>
		/// Проверка наличия номера телефона у клиента
		/// </summary>
		/// <param name="unitOfWork">unitOfWork</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <param name="phoneNumber">Номер телефона в формате XXXXXXXXXX</param>
		/// <returns><c>true</c> - есть телефон, <c>false</c> - нет</returns>
		bool PhoneExists(IUnitOfWork unitOfWork, int counterpartyId, string phoneNumber);
		/// <summary>
		/// Получения информации о состоянии активации онлайн профиля юр лица
		/// </summary>
		/// <param name="unitOfWork">unitOfWork</param>
		/// <param name="dto">Данные для проверки</param>
		/// <returns></returns>
		IEnumerable<ExternalLegalCounterpartyAccountActivation> GetOnlineLegalCounterpartyActivations(
			IUnitOfWork unitOfWork, IFindExternalLegalCounterpartyAccountDto dto);
	}
}
