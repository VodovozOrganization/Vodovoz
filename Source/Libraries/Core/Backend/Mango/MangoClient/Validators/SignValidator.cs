using Mango.Api.Dto;
using Mango.Core.Sign;
using Microsoft.Extensions.Logging;
using System;

namespace Mango.Api.Validators
{
	public class SignValidator : IValidator
	{
		private readonly ILogger<SignValidator> _logger;
		private readonly IDefaultSignGenerator _signGenerator;

		public SignValidator(ILogger<SignValidator> logger, IDefaultSignGenerator signGenerator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_signGenerator = signGenerator ?? throw new ArgumentNullException(nameof(signGenerator));
		}

		public bool Validate(EventRequestBase eventRequest)
		{
			var sign = _signGenerator.GetSign(eventRequest.Json);
			var result = sign == eventRequest.Sign;
			if(!result)
			{
				_logger.LogError("Подпись в запросе ({requestSign}) не совпадает со сгенерированной подписью ({sign}). Проверьте ключ и соль в настройках.", eventRequest.Sign, sign);
			}

			return result;
		}
	}
}
