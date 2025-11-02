using System;
using UnsubscribePage.Models;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;

namespace UnsubscribePage.Controllers
{
	public interface IUnsubscribeViewModelFactory
	{
		UnsubscribeViewModel CreateNewUnsubscribeViewModel(Guid guid, IEmailRepository emailRepository, IEmailSettings emailSettings);
	}
}
