using System;
using UnsubscribePage.Models;
using Vodovoz.EntityRepositories;

namespace UnsubscribePage.Controllers
{
	public interface IUnsubscribeViewModelFactory
	{
		UnsubscribeViewModel CreateNewUnsubscribeViewModel(Guid guid, IEmailRepository emailRepository);
	}
}
