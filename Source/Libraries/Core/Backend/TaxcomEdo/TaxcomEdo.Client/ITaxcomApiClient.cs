using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Dto;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdo.Client
{
	public interface ITaxcomApiClient
	{
		/// <summary>
		/// Передача данных по УПД в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования УПД по ЭДО</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task SendDataForCreateUpdByEdo(InfoForCreatingEdoUpd data, CancellationToken cancellationToken = default);
		/// <summary>
		/// Передача данных по УПД в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования УПД по ЭДО</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task<bool> SendDataForCreateUpdByEdo(UniversalTransferDocumentInfo data, CancellationToken cancellationToken = default);
		/// <summary>
		/// Передача данных по Счету в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования Счета по ЭДО</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task SendDataForCreateBillByEdo(InfoForCreatingEdoBill data, CancellationToken cancellationToken = default);
		/// <summary>
		/// Передача данных по документу заказа в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования Счета по ЭДО</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task SendDataForCreateInformalOrderDocumentByEdo(InfoForCreatingEdoInformalOrderDocument data, CancellationToken cancellationToken = default);
		/// <summary>
		/// Передача данных по Счету без отгрузки на долг в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования Счета без отгрузки на долг по ЭДО</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task SendDataForCreateBillWithoutShipmentForDebtByEdo(
			InfoForCreatingBillWithoutShipmentForDebtEdo data,
			CancellationToken cancellationToken = default);
		/// <summary>
		/// Передача данных по Счету без отгрузки на постоплату в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования Счета без отгрузки на постоплату по ЭДО</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task SendDataForCreateBillWithoutShipmentForPaymentByEdo(
			InfoForCreatingBillWithoutShipmentForPaymentEdo data,
			CancellationToken cancellationToken = default);
		/// <summary>
		/// Передача данных по Счету без отгрузки на предоплату в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования Счета без отгрузки на предоплату по ЭДО</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task SendDataForCreateBillWithoutShipmentForAdvancePaymentByEdo(
			InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo data,
			CancellationToken cancellationToken = default);
		/// <summary>
		/// Запрос изменений списка контактов
		/// </summary>
		/// <param name="lastCheckContactsUpdates">Время с которого нужно смотреть изменения</param>
		/// <param name="contactState">Ограничение по статусу</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task<EdoContactList> GetContactListUpdates(
			DateTime? lastCheckContactsUpdates, EdoContactStateCode? contactState, CancellationToken cancellationToken = default);
		/// <summary>
		/// Принятие приглашения к обмену по ЭДО
		/// </summary>
		/// <param name="edxClientId">Номер кабинета ЭДО провайдера клиента</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task<bool> AcceptContact(string edxClientId, CancellationToken cancellationToken = default);
		/// <summary>
		/// Получение архива со всеми документами из документооборота
		/// </summary>
		/// <param name="docFlowId">Id документооборота</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task<IEnumerable<byte>> GetDocFlowRawData(string docFlowId, CancellationToken cancellationToken = default);
		/// <summary>
		/// Получение списка изменений документов
		/// </summary>
		/// <param name="docFlowsUpdatesParameters">Параметры запроса получения изменений по документам</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task<EdoDocFlowUpdates> GetDocFlowsUpdates(
			GetDocFlowsUpdatesParameters docFlowsUpdatesParameters, CancellationToken cancellationToken = default);
		/// <summary>
		/// Отправка запроса на запуск необходимых транзакций по ЭДО
		/// </summary>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task StartProcessAutoSendReceive(CancellationToken cancellationToken = default);
		/// <summary>
		/// Отправка запроса на аннулирование документооборота
		/// </summary>
		/// <param name="docFlowId">Идентификатор документооборота</param>
		/// <param name="reason">Причина аннулирования</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task SendOfferCancellation(string docFlowId, string reason, CancellationToken cancellationToken = default);
		/// <summary>
		/// Отправка запроса на подписание документооборота
		/// </summary>
		/// <param name="docflowId">Id документооборота</param>
		/// <param name="organization">Название организации, от которой подписывается входящий документ</param>
		/// <returns></returns>
		Task<bool> AcceptIngoingDocflow(Guid? docflowId, string organization, CancellationToken cancellationToken = default);
		/// <summary>
		/// Отправка запроса на аннулирование документооборота
		/// </summary>
		/// <param name="docFlowId">Идентификатор документооборота</param>
		/// <param name="reason">Причина аннулирования</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task SendOfferCancellationRaw(string docFlowId, string comment, CancellationToken cancellationToken = default);
		/// <summary>
		/// Принятие запроса на аннулирование документооборота
		/// </summary>
		/// <param name="docFlowId">Идентификатор документооборота</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task AcceptOfferCancellation(string docFlowId, CancellationToken cancellationToken = default);
		/// <summary>
		/// Отказ запроса на аннулирование документооборота
		/// </summary>
		/// <param name="docFlowId">Идентификатор документооборота</param>
		/// <param name="reason">Причина аннулирования</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task RejectOfferCancellation(string docFlowId, string comment, CancellationToken cancellationToken = default);
	}
}
