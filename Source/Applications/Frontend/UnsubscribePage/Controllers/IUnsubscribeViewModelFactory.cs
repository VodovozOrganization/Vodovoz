using System;
using UnsubscribePage.Models;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;

namespace UnsubscribePage.Controllers
{
	/// <summary>
	/// Фабрика создания ViewModel страницы отписки.
	/// </summary>
	public interface IUnsubscribeViewModelFactory
	{
		/// <summary>
		/// Создаёт и инициализирует ViewModel страницы отписки.
		/// </summary>
		/// <param name="guid">Guid email-ссылки отписки.</param>
		/// <param name="emailRepository">Репозиторий email-данных.</param>
		/// <param name="emailSettings">Настройки email.</param>
		/// <returns>Инициализированная ViewModel.</returns>
		UnsubscribeViewModel CreateNewUnsubscribeViewModel(Guid guid, IEmailRepository emailRepository, IEmailSettings emailSettings);
	}
}
