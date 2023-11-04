using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Controllers
{
	public class ExternalCounterpartyController : IExternalCounterpartyController
	{
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IInteractiveService _interactiveService;

		public ExternalCounterpartyController(
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IInteractiveService interactiveService)
		{
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}
		
		public bool ArchiveExternalCounterparty(IUnitOfWork uow, int phoneId)
		{
			if(!HasActiveExternalCounterparties(uow, phoneId, out var externalCounterparties))
			{
				return true;
			}

			if(!_interactiveService.Question(
				"Данный номер телефона привязан к внешнему пользователю сайта/приложения\n" +
				"Вы действительно хотите его удалить?"))
			{
				return false;
			}

			ArchiveExternalCounterparty(externalCounterparties);

			return true;
		}

		public void ArchiveExternalCounterparty(IList<ExternalCounterparty> externalCounterparties)
		{
			foreach(var extCounterparty in externalCounterparties)
			{
				extCounterparty.Archive();
			}
		}

		public bool HasActiveExternalCounterparties(IUnitOfWork uow, int phoneId, out IList<ExternalCounterparty> externalCounterparties)
		{
			externalCounterparties = _externalCounterpartyRepository.GetActiveExternalCounterpartiesByPhone(uow, phoneId);
			return externalCounterparties.Any();
		}
	}
}
