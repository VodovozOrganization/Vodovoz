using System.Collections.Generic;
using Mango.Business.Models;
using Mango.Contracts.V1.Response;
using Mango.Domain.Entity;

namespace Mango.Business.Interfaces
{
	public interface ICallStatisticParser
	{
		/// <summary>
		/// Получить список сущностей для записи в БД на основе запроса и и фильтра по группам
		/// </summary>
		/// <param name="response">Ответ на запрос звонков</param>
		/// <param name="referenceData">Данные на основе которых выбирать нужные звонки</param>
		/// <returns>Список сущностей без хеша</returns>
		List<CallEntity> Parse(CallsResponse response, MangoReferenceData referenceData);
	}
}
