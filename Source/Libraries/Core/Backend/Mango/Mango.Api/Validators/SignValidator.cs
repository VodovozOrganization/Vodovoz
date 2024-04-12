using Mango.Core.Sign;
using Microsoft.Extensions.Logging;
using System;

namespace Mango.Api.Validators
{
	public class SignValidator
	{
		private readonly ILogger<SignValidator> _logger;
		private readonly IDefaultSignGenerator _signGenerator;

		public SignValidator(ILogger<SignValidator> logger, IDefaultSignGenerator signGenerator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_signGenerator = signGenerator ?? throw new ArgumentNullException(nameof(signGenerator));
		}

		public bool Validate(string requestSign, string json)
		{
			var sign = _signGenerator.GetSign(json);
			var result = sign == requestSign;
			if(!result)
			{
				_logger.LogError("Подпись в запросе ({requestSign}) не совпадает со сгенерированной подписью ({sign}). Проверьте подпись в настройках.", requestSign, sign);
			}

			return result;
		}
	}
}
