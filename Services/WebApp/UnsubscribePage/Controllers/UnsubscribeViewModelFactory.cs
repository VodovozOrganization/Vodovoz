using System;
using UnsubscribePage.Models;
using Vodovoz.EntityRepositories;

namespace UnsubscribePage.Controllers
{
	public class UnsubscribeViewModelFactory : IUnsubscribeViewModelFactory
	{
		public UnsubscribeViewModel CreateNewUnsubscribeViewModel(Guid guid, IEmailRepository emailRepository) =>
			new UnsubscribeViewModel(guid, emailRepository);
	}
}
