using QS.DomainModel.UoW;
using System;
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

		public UnsubscribeViewModel CreateNewUnsubscribeViewModel(Guid guid, IEmailRepository emailRepository, IEmailSettings emailSettings) =>
			new UnsubscribeViewModel(_uowFactory, guid, emailRepository, emailSettings);
	}
}
