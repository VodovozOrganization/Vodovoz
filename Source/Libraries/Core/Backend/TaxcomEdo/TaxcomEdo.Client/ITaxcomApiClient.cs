﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
		/// Передача данных по Счету в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования Счета по ЭДО</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task SendDataForCreateBillByEdo(InfoForCreatingEdoBill data, CancellationToken cancellationToken = default);
		/// <summary>
		/// Передача данных по Счету без отгрузки в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования Счета без отгрузки по ЭДО</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task SendDataForCreateBillWithoutShipmentByEdo(
			InfoForCreatingBillWithoutShipmentEdo data,
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
		/// Принятие приглашение к обмену по ЭДО
		/// </summary>
		/// <param name="edxClientId">Номер кабинета ЭДО провайдера клиента</param>
		/// <param name="cancellationToken">Токен для остановки выполнения задачи</param>
		/// <returns></returns>
		Task AcceptContact(string edxClientId, CancellationToken cancellationToken = default);
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
	}
}
