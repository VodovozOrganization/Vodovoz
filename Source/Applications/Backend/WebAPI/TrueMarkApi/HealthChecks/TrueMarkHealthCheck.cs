﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using TrueMarkApi.Library;
using Vodovoz.Settings.Edo;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace TrueMarkApi.HealthChecks
{
	public class TrueMarkHealthCheck : VodovozHealthCheckBase
	{
		private readonly IEdoSettings _edoSettings;

		public TrueMarkHealthCheck(ILogger<TrueMarkHealthCheck> logger, IEdoSettings edoSettings, IUnitOfWorkFactory unitOfWorkFactory)
			: base(logger, unitOfWorkFactory)
		{
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
		}

		protected override Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthResult = new VodovozHealthResultDto();

			var controllerIsHealthy = CheckControllerIsHealthy();

			if(!controllerIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("Не пройдена проверка контроллера.");
			}

			var serviceIsHealthy = CheckDocumentServiceIsHealthy();

			if(!serviceIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("Не пройдена проверка сервиса документов.");
			}

			healthResult.IsHealthy = controllerIsHealthy && serviceIsHealthy;

			return Task.FromResult(healthResult);
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
