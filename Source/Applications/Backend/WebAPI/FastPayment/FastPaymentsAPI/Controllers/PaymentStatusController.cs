using FastPaymentsApi.Contracts.Requests;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Notifications;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System.Linq;
using System.Net.Http;
using Vodovoz.Domain.FastPayments;
using Vodovoz.EntityRepositories.FastPayments;

namespace FastPaymentsAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PaymentStatusController : Controller
	{
		private readonly IUnitOfWork _uow;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IFastPaymentFactory _fastPaymentAPIFactory;
		private readonly NotificationModel _notificationModel;

		public PaymentStatusController(
			IUnitOfWork uow, 
			IFastPaymentRepository fastPaymentRepository, 
			IFastPaymentFactory fastPaymentAPIFactory,
			NotificationModel notificationModel
			)
		{
			_uow = uow ?? throw new System.ArgumentNullException(nameof(uow));
			_fastPaymentRepository = fastPaymentRepository ?? throw new System.ArgumentNullException(nameof(fastPaymentRepository));
			_fastPaymentAPIFactory = fastPaymentAPIFactory ?? throw new System.ArgumentNullException(nameof(fastPaymentAPIFactory));
			_notificationModel = notificationModel ?? throw new System.ArgumentNullException(nameof(notificationModel));
		}

		/// <summary>
		/// Эндпойнт для получения информации об оплате онлайн-заказа с сайта или мобильного приложения
		/// </summary>
		/// <param name="orderId">Id заказа</param>
		/// <returns></returns>
		[HttpGet]
		[Route("/api/GetPaymentStatus")]
		public FastPaymentStatusDto GetPaymentStatus([FromQuery] int orderId)
		{
			if(orderId < 1)
			{
				throw new HttpRequestException($"Аргумент {nameof(orderId)} должен иметь положительное значение");
			}

			var result = new FastPaymentStatusDto();

			var payments = _fastPaymentRepository.GetAllPaymentsByOnlineOrder(_uow, orderId);

			if(!payments.Any())
			{
				result.PaymentStatus = RequestPaymentStatus.NotFound;
				return result;
			}

			var payment = payments.First();
			result.PaymentStatus = (RequestPaymentStatus)payment.FastPaymentStatus;
			result.PaymentDetails = _fastPaymentAPIFactory.GetNewOnlinePaymentDetailsDto(orderId, payment.Amount);

			_notificationModel.SaveNotification(payment, FastPaymentNotificationType.Site, true);
			_notificationModel.SaveNotification(payment, FastPaymentNotificationType.MobileApp, true);

			return result;
		}

		/// <summary>
		/// Эндпойнт для получения информации об оплате онлайн-заказа с сайта или мобильного приложения
		/// </summary>
		/// <param name="orderId">Id заказа</param>
		/// <returns></returns>
		[HttpGet]
		[Route("/api/GetCheckPaymentStatus")]
		public FastPaymentStatusDto GetCheckPaymentStatus([FromQuery] int orderId)
		{
			if(orderId < 1)
			{
				throw new HttpRequestException($"Аргумент {nameof(orderId)} должен иметь положительное значение");
			}

			var result = new FastPaymentStatusDto();

			var payments = _fastPaymentRepository.GetAllPaymentsByOnlineOrder(_uow, orderId);

			if(!payments.Any())
			{
				result.PaymentStatus = RequestPaymentStatus.NotFound;
				return result;
			}

			var payment = payments.First();
			result.PaymentStatus = (RequestPaymentStatus)payment.FastPaymentStatus;
			result.PaymentDetails = _fastPaymentAPIFactory.GetNewOnlinePaymentDetailsDto(orderId, payment.Amount);

			return result;
		}
	}
}
