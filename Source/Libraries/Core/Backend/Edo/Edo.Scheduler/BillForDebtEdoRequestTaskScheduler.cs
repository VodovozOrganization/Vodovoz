﻿using Vodovoz.Core.Domain.Edo;

namespace Edo.Scheduler.Service
{
	public class BillForDebtEdoRequestTaskScheduler
	{
		public EdoTask CreateTask(BillForDebtEdoRequest edoRequest)
		{
			var task = new DocumentEdoTask
			{
				CustomerEdoRequest = edoRequest,
				DocumentType = edoRequest.DocumentType,

				// как-то нужно заполнить организацию
				//FromOrganization = edoRequest.,

				//клиента берем из сущности счета без отгрузки
				//ToClient = edoRequest.Customer.Id,

				Status = EdoTaskStatus.New
			};
			return task;
		}
	}
}
