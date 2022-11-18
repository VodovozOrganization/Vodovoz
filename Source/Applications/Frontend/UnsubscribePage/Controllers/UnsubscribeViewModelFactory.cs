using System;
using UnsubscribePage.Models;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;

namespace UnsubscribePage.Controllers
{
	public class UnsubscribeViewModelFactory : IUnsubscribeViewModelFactory
	{
		public UnsubscribeViewModel CreateNewUnsubscribeViewModel(Guid guid, IEmailRepository emailRepository, IEmailParametersProvider emailParametersProvider) =>
			new UnsubscribeViewModel(guid, emailRepository, emailParametersProvider);
	}
}
