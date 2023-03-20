using NLog;
using QS.DomainModel.UoW;
using QS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;
using Vodovoz.Services;
using VodovozSalesReceiptsService.DTO;

namespace VodovozSalesReceiptsService
{
	/// <summary>
	/// Класс автоматической отправки чеков для заказов
	/// </summary>
	public class FiscalizationWorker
	{
		public FiscalizationWorker(CashReceiptsSender cashReceiptsSender)
		{
			this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			this.salesReceiptSender = salesReceiptSender ?? throw new ArgumentNullException(nameof(salesReceiptSender));
			this.orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			this.cashBoxes = cashBoxes ?? throw new ArgumentNullException(nameof(cashBoxes));
			_cashReceiptsSender = cashReceiptsSender ?? throw new ArgumentNullException(nameof(cashReceiptsSender));
		}

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IOrderRepository orderRepository;
		private readonly ISalesReceiptSender salesReceiptSender;
		private readonly IOrderParametersProvider orderParametersProvider;
		private readonly IEnumerable<CashBox> cashBoxes;
		private readonly TimeSpan initialDelay = TimeSpan.FromSeconds(5);
		private readonly TimeSpan delay = TimeSpan.FromSeconds(45);

		/// <summary>
		/// Максимальное число чеков которое можно отправить <see cref="FiscalizationWorker"/> за один цикл 
		/// </summary>
		private readonly int maxReceiptsAllowedToSendInOneGo = 30;
		private readonly CashReceiptsSender _cashReceiptsSender;

		/// <summary>
		/// Запускает процесс автоматической отправки чеков
		/// </summary>
		public void Start()
		{
			Task.Run(() =>
			{
				Task.Delay(initialDelay).Wait();

				while(true)
				{
					try
					{
						PrepareAndSendReceipts();
					}
					catch(Exception ex)
					{
						logger.Error(ex);
					}

					Delay();
				}
			});
		}

		private void Delay()
		{
			if(DateTime.Now.Hour >= 1 && DateTime.Now.Hour < 5)
			{
				logger.Info("Ночь. Не пытаемся отсылать чеки с 1 до 5 утра.");
				var fiveHrsOfToday = DateTime.Today.AddHours(5);
				Task.Delay(fiveHrsOfToday.Subtract(DateTime.Now)).Wait();
			}
			else
			{
				logger.Info($"Ожидание {delay.Seconds} секунд перед отправкой чеков");
				Task.Delay(delay).Wait();
			}
		}
	}
}
