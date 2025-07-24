using System;
using System.Collections.Generic;
using ExportTo1c.Library;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;

namespace Vodovoz.ServiceDialogs.ExportTo1c
{
	public class ExportOperation : IDisposable
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly DateTime start;
		private readonly DateTime end;
		private readonly Export1cMode mode;
		private readonly IOrderSettings orderSettings;
		private readonly int? organizationId;
		private IList<Order> orders;

		public int Steps => orders.Count;
		public ExportData Result { get; private set; }

		public ExportOperation(
			Export1cMode mode,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IUnitOfWorkFactory unitOfWorkFactory,
			DateTime start,
			DateTime end,
			int? organizationId = null)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			this.orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Экспорт в 1с");
			this.start = start;
			this.end = end;
			this.mode = mode;
			this.organizationId = organizationId;
		}

		public void Run(IWorker worker)
		{
			worker.OperationName = "Подготовка данных";
			worker.ReportProgress(0, "Загрузка заказов");
			orders = _orderRepository.GetOrdersToExport1c8(_unitOfWork, orderSettings, mode, start, end, organizationId);
			worker.OperationName = "Выгрузка реализаций и счетов-фактур";
			worker.StepsCount = orders.Count;
			Result = new ExportData(_unitOfWork, mode, start, end);
			int i = 0;

			while(!worker.IsCancelled && i < orders.Count)
			{
				worker.ReportProgress(i, "Заказ");
				Result.AddOrder(orders[i]);
				i++;
			}

			Result.FinishRetailDocuments();
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
