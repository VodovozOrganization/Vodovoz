﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.V4
{
	[ApiController]
	[Route("/api/[action]")]
	public class SignatureControllerBase : ControllerBase
	{
		private const string _invalidSignature = "Неккоректная контрольная сумма";
		
		public SignatureControllerBase(ILogger<SignatureControllerBase> logger)
		{
			Logger = logger;
		}

		protected ILogger<SignatureControllerBase> Logger { get; }
		
		protected IActionResult InvalidSignature(string requestSignature, string generatedSignature)
		{
			Logger.LogWarning(
				"{InvalidSignature}. Пришла {ReceivedSign}, должна быть {GeneratedSign}",
				_invalidSignature,
				requestSignature,
				generatedSignature
			);

			return ValidationProblem(_invalidSignature);
		}
	}
}
