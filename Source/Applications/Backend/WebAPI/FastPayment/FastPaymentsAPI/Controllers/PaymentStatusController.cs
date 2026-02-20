using System;
using FastPaymentsApi.Contracts.Requests;
using FastPaymentsAPI.Library.Factories;
using Microsoft.AspNetCore.Mvc;
using QS.DomainModel.UoW;
using System.Linq;
using System.Net.Http;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.FastPayments;
using Vodovoz.EntityRepositories.FastPayments;
using VodovozHealthCheck.Helpers;

namespace FastPaymentsAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PaymentStatusController : Controller
	{
		private readonly IUnitOfWork _uow;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IFastPaymentFactory _fastPaymentApiFactory;
		private readonly IGenericRepository<FastPaymentStatusUpdatedEvent> _statusUpdatedEventRepository;

		public PaymentStatusController(
			IUnitOfWork uow, 
			IFastPaymentRepository fastPaymentRepository, 
			IFastPaymentFactory fastPaymentApiFactory,
			IGenericRepository<FastPaymentStatusUpdatedEvent> statusUpdatedEventRepository
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_fastPaymentApiFactory = fastPaymentApiFactory ?? throw new ArgumentNullException(nameof(fastPaymentApiFactory));
			_statusUpdatedEventRepository = statusUpdatedEventRepository ?? throw new ArgumentNullException(nameof(statusUpdatedEventRepository));
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
			result.PaymentDetails = _fastPaymentApiFactory.GetNewOnlinePaymentDetailsDto(orderId, payment.Amount);

			var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);

			if(!isDryRun)
			{
				var statusUpdatedEvents = _statusUpdatedEventRepository
					.Get(_uow, x => x.FastPayment.Id == payment.Id)
					.ToList();

				foreach(var @event in statusUpdatedEvents)
				{
					@event.HttpCode = 200;
					_uow.Save(@event);
				}

				_uow.Commit();
			}

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
			result.PaymentDetails = _fastPaymentApiFactory.GetNewOnlinePaymentDetailsDto(orderId, payment.Amount);

			return result;
		}
	}
}
