﻿using System;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class CounterpartyController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly ICounterpartyModel _counterpartyModel;

		public CounterpartyController(
			ILogger<CounterpartyController> logger,
			ICounterpartyModel counterpartyModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_counterpartyModel = counterpartyModel ?? throw new ArgumentNullException(nameof(counterpartyModel));
		}

		[HttpPost]
		[Route("GetCounterparty")]
		public CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto)
		{
			try
			{
				return _counterpartyModel.GetCounterparty(counterpartyContactInfoDto);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при идентификации контрагента");
				throw;
			}
		}
		
		[HttpPost]
		[Route("RegisterCounterparty")]
		public CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto)
		{
			try
			{
				return _counterpartyModel.RegisterCounterparty(counterpartyDto);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при регистрации контрагента");
				throw;
			}
		}
		
		[HttpPost]
		[Route("UpdateCounterpartyInfo")]
		public CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto)
		{
			try
			{
				return _counterpartyModel.UpdateCounterpartyInfo(counterpartyDto);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при обновлении данных контрагента");
				throw;
			}
		}
	}
}
