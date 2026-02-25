using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.Default
{
	public class SignatureControllerBase : VersionedController
	{
		private const string _invalidSignature = "Неккоректная контрольная сумма";
		
		public SignatureControllerBase(ILogger<SignatureControllerBase> logger) : base(logger)
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
