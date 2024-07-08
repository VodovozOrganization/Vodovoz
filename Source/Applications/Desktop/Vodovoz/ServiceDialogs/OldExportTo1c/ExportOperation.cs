using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSProjectsLib;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;

namespace Vodovoz.OldExportTo1c
{
	public class ExportOperation : IDisposable
	{
		private readonly IOrderRepository _orderRepository = ScopeProvider.Scope.Resolve<IOrderRepository>();
		private readonly IUnitOfWork uow;
		private readonly IOrderSettings orderSettings;
		private readonly DateTime start;
		private readonly DateTime end;
		private readonly Export1cMode mode;
		private IList<Order> orders;

		public int Steps => orders.Count;
		public ExportData Result{ get; private set;}

		public ExportOperation(IOrderSettings orderSettings, Export1cMode mode, DateTime start, DateTime end)
		{			
			this.orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			this.start = start;
			this.end = end;
			this.mode = mode;
		}

		public void Run(IWorker worker)
		{				
			worker.OperationName = "Подготовка данных";
			worker.ReportProgress(0, "Загрузка заказов");
			orders = _orderRepository.GetOrdersToExport1c8(uow, orderSettings, mode, start, end);
			worker.OperationName = "Выгрузка реализаций и счетов-фактур";
			worker.StepsCount = this.orders.Count;
			Result = new ExportData(uow, mode, start, end);
			int i = 0;
			while (!worker.IsCancelled && i < orders.Count)
			{
				worker.ReportProgress(i, "Заказ");
				Result.AddOrder(orders[i]);
				i++;
			}
		}

		public void Dispose()
		{
			uow?.Dispose();
		}
	}
}

