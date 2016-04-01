using System;
using QSProjectsLib;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;
using QSOrmProject;
using Vodovoz.Repository;

namespace Vodovoz.ExportTo1c
{
	public class ExportOperation
	{		
		private readonly IUnitOfWork uow;
		private readonly DateTime start;
		private readonly DateTime end;
		private IList<Order> orders;

		public int Steps{ get { return orders.Count; } }
		public ExportData Result{ get; private set;}

		public ExportOperation(DateTime start, DateTime end)
		{			
			this.uow = UnitOfWorkFactory.CreateWithoutRoot();
			this.start = start;
			this.end = end;
		}

		public void Run(IWorker worker)
		{				
			worker.OperationName = "Подготовка данных";
			worker.ReportProgress(0, "Загрузка заказов");
			this.orders = OrderRepository.GetCompleteOrdersBetweenDates(uow, start, end);
			worker.OperationName = "Выгрузка реализаций и счетов-фактур";
			worker.StepsCount = this.orders.Count;
			Result = new ExportData(uow, start, end);
			int i = 0;
			while (!worker.IsCancelled && i < orders.Count)
			{
				worker.ReportProgress(i, "Заказ");
				Result.AddOrder(orders[i]);
				i++;
			}
		}
	}
}

