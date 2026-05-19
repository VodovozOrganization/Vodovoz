using QS.DomainModel.UoW;
using System;
using System.Text.Json;
using UnsubscribePage.Models;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;

namespace UnsubscribePage.Controllers
{
	public class UnsubscribeViewModelFactory : IUnsubscribeViewModelFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public UnsubscribeViewModelFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public UnsubscribeViewModel CreateNewUnsubscribeViewModel(
			Guid guid,
			IEmailRepository emailRepository,
			IEmailSettings emailSettings)
		{
			using var unitOfWork = _uowFactory.CreateWithoutRoot("Инициализация страницы отписки");

			var guidCounterpartyEmail = emailRepository.GetCounterpartyEmailByGuidForUnsubscribing(unitOfWork, guid);
			var reasons = emailRepository.GetUnsubscribingReasons(unitOfWork, emailSettings, isForUnsubscribePage: true);

			return new UnsubscribeViewModel
			{
				OtherReasonId = emailSettings.BulkEmailEventOtherReasonId,
				CounterpartyBulkSubscribeNodeSerialized = JsonSerializer.Serialize(guidCounterpartyEmail),
				ReasonsListSerialized = JsonSerializer.Serialize(reasons)
			};
		}
	}
}
