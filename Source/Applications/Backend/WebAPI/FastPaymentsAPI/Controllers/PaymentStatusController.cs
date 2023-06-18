﻿using FastPaymentsAPI.Library.DTO_s.Requests;
using FastPaymentsAPI.Library.Factories;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System.Linq;
using System.Net.Http;
using Vodovoz.EntityRepositories.FastPayments;

namespace FastPaymentsAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PaymentStatusController : Controller
	{
		private readonly IUnitOfWork _uow;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IFastPaymentAPIFactory _fastPaymentAPIFactory;

		public PaymentStatusController(IUnitOfWork uow, IFastPaymentRepository fastPaymentRepository, IFastPaymentAPIFactory fastPaymentAPIFactory)
		{
			_uow = uow ?? throw new System.ArgumentNullException(nameof(uow));
			_fastPaymentRepository = fastPaymentRepository ?? throw new System.ArgumentNullException(nameof(fastPaymentRepository));
			_fastPaymentAPIFactory = fastPaymentAPIFactory ?? throw new System.ArgumentNullException(nameof(fastPaymentAPIFactory));
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

			return result;
		}
	}
}
