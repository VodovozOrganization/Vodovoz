using QS.DomainModel.UoW;
using System;
using UnsubscribePage.Models;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;

namespace UnsubscribePage.Controllers
{
	public class UnsubscribeViewModelFactory : IUnsubscribeViewModelFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public UnsubscribeViewModelFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public UnsubscribeViewModel CreateNewUnsubscribeViewModel(Guid guid, IEmailRepository emailRepository, IEmailParametersProvider emailParametersProvider) =>
			new UnsubscribeViewModel(_uowFactory, guid, emailRepository, emailParametersProvider);
	}
}
