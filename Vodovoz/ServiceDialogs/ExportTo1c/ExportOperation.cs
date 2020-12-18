using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Repositories.Orders;

namespace Vodovoz.ExportTo1c
{
    public class ExportOperation : IDisposable
    {
        private readonly IUnitOfWork uow;
        private readonly DateTime start;
        private readonly DateTime end;
        private readonly Export1cMode mode;
        private readonly Organization organization;
        private IList<Order> orders;

        public int Steps => orders.Count;
        public ExportData Result { get; private set; }

        public ExportOperation(Export1cMode mode, DateTime start, DateTime end, Organization organization = null)
        {
            uow = UnitOfWorkFactory.CreateWithoutRoot();
            this.start = start;
            this.end = end;
            this.mode = mode;
            this.organization = organization;
        }

        public void Run(IWorker worker)
        {
            worker.OperationName = "Подготовка данных";
            worker.ReportProgress(0, "Загрузка заказов");
            orders = OrderSingletonRepository.GetInstance().GetOrdersToExport1c8(uow, mode, start, end, organization);
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