using System;
using System.Threading;
using TrueMarkApi.Library;
using Vodovoz.Settings.Edo;
using VodovozHealthCheck;

namespace TrueMarkApi.HealthChecks
{
	public class TrueMarkHealthCheck : VodovozHealthCheckBase
	{
		private readonly IEdoSettings _edoSettings;

		public TrueMarkHealthCheck(IEdoSettings edoSettings)
		{
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
		}

		protected override VodovozHealthResultDto GetHealthResult()
		{
			var healthResult = new VodovozHealthResultDto();

			var controllerIsHealthy = CheckControllerIsHealthy();

			if(!controllerIsHealthy)
			{
				healthResult.AdditionalResults.Add("Не пройдена проверка контроллера.");
			}

			var serviceIsHealthy = CheckDocumentServiceIsHealthy();

			if(!serviceIsHealthy)
			{
				healthResult.AdditionalResults.Add("Не пройдена проверка сервиса документов.");
			}

			healthResult.IsHealthy = controllerIsHealthy && serviceIsHealthy;

			return healthResult;
		}

		private bool CheckControllerIsHealthy()
		{
			var client = new TrueMarkApiClient(_edoSettings.TrueMarkApiBaseUrl, _edoSettings.TrueMarkApiToken);
			var result = client.GetParticipantRegistrationForWaterStatusAsync(_edoSettings.TrueMarkApiParticipantRegistrationForWaterUri, "7816453294", new CancellationToken());

			if(result != null && result.Result.RegistrationStatusString == "Зарегистрирован")
			{
				return true;
			}

			return false;
		}

		public bool IsHealthy { get; set; }

		private bool CheckDocumentServiceIsHealthy() => IsHealthy;

	}
}
