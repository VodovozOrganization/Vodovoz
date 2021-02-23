using System;
using System.ServiceModel.Web;
using BitrixApi.DTO.DataContractJsonSerializer;
using BitrixIntegration.ServiceInterfaces;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;

namespace BitrixIntegration
{
	public class BitrixService : IBitrixService, /*IBitrixEventService,*/ IBitrixServiceWeb
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IBitrixServiceSettings bitrixServiceSettings;

		public BitrixService(IBitrixServiceSettings bitrixServiceSettings)
		{
			BitrixManager.Init();
			this.bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
		}

		//Вызов функции из ДВ
		public Tuple<bool, string> SendNewStatus(OrderStatus status, Order order)
		{
			return BitrixManager.sendOrderStatusToBitrix(status, order);
		}

		public int Add(int a, int b)
			=> a + b;
		
		
		
		public void PostEvent(BitrixPostResponse response)
		{
			// logger.Info("Получен из битрикса: \n" + response.ToString());
			BitrixManager.AddEvent(response);
			if (WebOperationContext.Current != null){
				logger.Info(WebOperationContext.Current.IncomingResponse.ToString());
				WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
			}
		}

		public bool ServiceStatus()
		{
			int bitrixInQueue = BitrixManager.CountOrderNewStatusesInQueue();
			if(bitrixInQueue > bitrixServiceSettings.MaxStatusesInQueueForWorkingService) {
				return false;
			}
			return true;
		}

	}
}
