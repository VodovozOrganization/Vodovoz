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
			this.orders = orders = OrderRepository.GetCompleteOrdersBetweenDates(uow, start, end);
			worker.OperationName = "Выгрузка реализаций и счет-фактур";
			worker.StepsCount = this.orders.Count;
			Result = new ExportData(uow);
			Result.Version = "2.0";
			Result.ExportDate = DateTime.Now;
			Result.StartPeriodDate = this.start;
			Result.EndPeriodDate = this.end;
			Result.SourceName = "Торговля+Склад, редакция 9.2";
			Result.DestinationName = "БухгалтерияПредприятия";
			Result.ConversionRulesId = "70e9dbac-59df-44bb-82c6-7d4f30d37c74";
			Result.Comment = "";

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

