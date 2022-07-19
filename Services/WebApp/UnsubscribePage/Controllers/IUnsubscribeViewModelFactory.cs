using System;
using UnsubscribePage.Models;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;

namespace UnsubscribePage.Controllers
{
	public interface IUnsubscribeViewModelFactory
	{
		UnsubscribeViewModel CreateNewUnsubscribeViewModel(Guid guid, IEmailRepository emailRepository, IEmailParametersProvider emailParametersProvider);
	}
}
