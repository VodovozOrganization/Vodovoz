using System;
using System.Collections.Generic;
using ExportTo1c.Library;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;

namespace Vodovoz.ViewModels.ViewModels.Service
{
	public class ExportTo1cOperation : IDisposable
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly DateTime _start;
		private readonly DateTime _end;
		private readonly Export1cMode _mode;
		private readonly IOrderSettings _orderSettings;
		private readonly int? _organizationId;
		private IList<Order> _orders;

		public ExportTo1cOperation(
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
			
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Экспорт в 1с");
			_start = start;
			_end = end;
			_mode = mode;
			_organizationId = organizationId;
		}
		
		public int Steps => _orders.Count;
		
		public ExportData Result { get; private set; }

		public void Run(IWorker worker)
		{
			worker.OperationName = "Подготовка данных";
			worker.ReportProgress(0, "Загрузка заказов");
			_orders = _orderRepository.GetOrdersToExport1c8(_unitOfWork, _orderSettings, _mode, _start, _end, _organizationId);
			worker.OperationName = "Выгрузка реализаций и счетов-фактур";
			worker.StepsCount = _orders.Count;
			Result = new ExportData(_unitOfWork, _mode, _start, _end);
			
			int i = 0;

			while(!worker.IsCancelled && i < _orders.Count)
			{
				worker.ReportProgress(i, "Заказ");
				Result.AddOrder(_orders[i]);
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
