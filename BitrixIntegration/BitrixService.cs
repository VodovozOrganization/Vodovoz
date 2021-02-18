using System;
using System.Net;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using BitrixApi.DTO.DataContractJsonSerializer;
using BitrixApi.REST;
using BitrixIntegration.DTO.Mailjet;
using BitrixIntegration.ServiceInterfaces;
using QS.DomainModel.UoW;
using QS.Project.DB;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;
using Email = BitrixIntegration.DTO.Email;

namespace BitrixIntegration
{
	public class BitrixService : IBitrixService, IBitrixEventService, IBitrixServiceWeb
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
		{
			return a + b;
		}
		
		
		public void PostEvent(BitrixPostResponse response)
		{
			BitrixManager.AddEvent(response);
			if (WebOperationContext.Current != null)
				WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
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
