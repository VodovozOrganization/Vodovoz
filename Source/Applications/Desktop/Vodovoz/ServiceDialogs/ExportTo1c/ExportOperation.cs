using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSProjectsLib;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;

namespace Vodovoz.ExportTo1c
{
	public class ExportOperation : IDisposable
    {
	    private readonly IOrderRepository _orderRepository = ScopeProvider.Scope.Resolve<IOrderRepository>();
        private readonly IUnitOfWork uow;
        private readonly DateTime start;
        private readonly DateTime end;
        private readonly Export1cMode mode;
        private readonly IOrderSettings orderSettings;
        private readonly int? organizationId;
        private IList<Order> orders;

        public int Steps => orders.Count;
        public ExportData Result { get; private set; }

        public ExportOperation(Export1cMode mode, IOrderSettings orderSettings, DateTime start, DateTime end, int? organizationId = null)
        {
            this.orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
            uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
            this.start = start;
            this.end = end;
            this.mode = mode;
            this.organizationId = organizationId;
        }

        public void Run(IWorker worker)
        {
            worker.OperationName = "Подготовка данных";
            worker.ReportProgress(0, "Загрузка заказов");
            orders = _orderRepository.GetOrdersToExport1c8(uow, orderSettings, mode, start, end, organizationId);
            worker.OperationName = "Выгрузка реализаций и счетов-фактур";
            worker.StepsCount = this.orders.Count;
            Result = new ExportData(uow, mode, start, end);
            int i = 0;

            while(!worker.IsCancelled && i < orders.Count) {
                worker.ReportProgress(i, "Заказ");
                Result.AddOrder(orders[i]);
                i++;
            }
            
            Result.FinishRetailDocuments();
        }

        public void Dispose()
        {
            uow?.Dispose();
        }
    }
}
