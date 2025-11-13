using Vodovoz.Core.Domain.Edo;

namespace Edo.Scheduler.Service
{
	public class BillForPaymentEdoRequestTaskScheduler
	{
		public EdoTask CreateTask(BillForPaymentEdoRequest edoRequest)
		{
			var task = new DocumentEdoTask
			{
				DocumentType = edoRequest.DocumentType,

				// как-то нужно заполнить организацию
				//FromOrganization = edoRequest.,

				//клиента берем из сущности счета без отгрузки
				//ToClient = edoRequest.Customer.Id,

				Status = EdoTaskStatus.New
			};

			edoRequest.Task = task;

			return task;
		}
	}
}
