using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories.Orders;

namespace Vodovoz.ExportTo1c
{
	public class ExportOperation
	{		
		private readonly IUnitOfWork uow;
		private readonly DateTime start;
		private readonly DateTime end;
		private Export1cMode mode;
		private IList<Order> orders;

		public int Steps{ get { return orders.Count; } }
		public ExportData Result{ get; private set;}

		public ExportOperation(Export1cMode mode, DateTime start, DateTime end)
		{			
			this.uow = UnitOfWorkFactory.CreateWithoutRoot();
			this.start = start;
			this.end = end;
			this.mode = mode;
		}

		public void Run(IWorker worker)
		{				
			worker.OperationName = "Подготовка данных";
			worker.ReportProgress(0, "Загрузка заказов");
			this.orders = OrderRepository.GetOrdersToExport1c8(uow, mode, start, end);
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

