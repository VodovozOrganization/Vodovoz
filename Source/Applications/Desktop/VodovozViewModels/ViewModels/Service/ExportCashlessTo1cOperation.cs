using ExportTo1c.Library;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;

namespace Vodovoz.ViewModels.ViewModels.Service
{
	public class ExportCashlessTo1cOperation : IDisposable
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly DateTime _start;
		private readonly DateTime _end;
		private readonly Export1cMode _mode;
		private readonly IOrderSettings _orderSettings;
		private readonly int? _organizationId;
		private IList<Order> _orders;

		public ExportCashlessTo1cOperation(
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

		public void Run(IProgressBarDisplayable progressBarDisplayable, CancellationToken cancellationToken)
		{
			progressBarDisplayable.Update("Получаем заказы...");
			_orders = _orderRepository.GetOrdersToExport1c8(_unitOfWork, _orderSettings, _mode, _start, _end, _organizationId);
			progressBarDisplayable.Start(_orders.Count, 0, "Выгрузка реализаций и счетов-фактур");
			Result = new ExportData(_unitOfWork, _mode, _start, _end);

			int i = 0;

			var ordersCount = _orders.Count;

			while(i < ordersCount)
			{
				Result.AddOrder(_orders[i]);
				i++;
				progressBarDisplayable.Add(1, $"Заказ {i}/{ordersCount}");

				cancellationToken.ThrowIfCancellationRequested();
			}

			Result.FinishRetailDocuments();

			progressBarDisplayable.Update("Выгрузка безнала завершена");
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
