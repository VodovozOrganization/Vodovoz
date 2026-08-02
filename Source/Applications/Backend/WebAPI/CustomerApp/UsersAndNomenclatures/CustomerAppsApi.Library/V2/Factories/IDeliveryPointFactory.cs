using System.Collections.Generic;
using CustomerAppsApi.Library.V2.Dto;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace CustomerAppsApi.Library.V2.Factories
{
	public interface IDeliveryPointFactory
	{
		/// <summary>
		/// Создание временной создаваемой ТД
		/// </summary>
		/// <param name="source">Источник</param>
		/// <param name="uniqueKey">Уникальный ключ</param>
		/// <returns></returns>
		ExternalCreatingDeliveryPoint CreateNewExternalCreatingDeliveryPoint(Source source, string uniqueKey);
		/// <summary>
		/// Созданеи сущности ТД
		/// </summary>
		/// <param name="newDeliveryPointInfoDto">Данные для ТД</param>
		/// <returns></returns>
		DeliveryPoint CreateNewDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto);
		/// <summary>
		/// Создание информации по ТД клиента
		/// </summary>
		/// <param name="deliveryPointsForSend">Данные</param>
		/// <returns></returns>
		DeliveryPointsDto CreateDeliveryPointsDto(IEnumerable<DeliveryPointForSendNode> deliveryPointsForSend);
		/// <summary>
		/// Создание информации по ТД клиента с ошибкой
		/// </summary>
		/// <param name="errorMessage">Описание ошибки</param>
		/// <returns></returns>
		DeliveryPointsDto CreateErrorDeliveryPointsInfo(string errorMessage);
		/// <summary>
		/// Инофрмация по созданной ТД
		/// </summary>
		/// <param name="newDeliveryPointInfoDto">Данные по ТД</param>
		/// <param name="deliveryPointId">Идентификатор ТД</param>
		/// <returns></returns>
		CreatedDeliveryPointDto CreateDeliveryPointDto(NewDeliveryPointInfoDto newDeliveryPointInfoDto, int deliveryPointId);
	}
}
