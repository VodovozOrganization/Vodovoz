using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Nodes;

namespace Vodovoz.Controllers
{
	public class ExternalCounterpartyController : IExternalCounterpartyController
	{
		private readonly IDeleteEntityService _deleteEntityService;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IInteractiveService _interactiveService;

		public string PhoneAssignedExternalCounterpartyMessage =>
			"Данный номер телефона привязан к внешнему пользователю сайта/приложения\n" +
			"При удалении/архивации телефона будут таже удалены все связанные с пользователем данные и он потеряет доступ к сайту/МП\n";

		public ExternalCounterpartyController(
			IDeleteEntityService deleteEntityService,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IInteractiveService interactiveService)
		{
			_deleteEntityService = deleteEntityService ?? throw new ArgumentNullException(nameof(deleteEntityService));
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}
		
		public bool DeleteExternalCounterparties(IUnitOfWork uow, int phoneId)
		{
			if(!HasActiveExternalCounterparties(uow, phoneId, out var externalCounterparties))
			{
				return true;
			}

			if(!_interactiveService.Question(PhoneAssignedExternalCounterpartyMessage + "Вы действительно хотите продолжить?"))
			{
				return false;
			}

			DeleteExternalCounterparties(uow, externalCounterparties);

			return true;
		}
		
		public void DeleteExternalCounterparties(IUnitOfWork uow, IEnumerable<ExternalCounterparty> externalCounterparties)
		{
			foreach(var externalCounterparty in externalCounterparties)
			{
				_deleteEntityService.DeleteEntity<ExternalCounterparty>(externalCounterparty.Id, uow, forceDelete: true);
			}
		}

		public bool HasActiveExternalCounterparties(
			IUnitOfWork uow,
			int phoneId,
			out IEnumerable<ExternalCounterparty> externalCounterparties)
		{
			externalCounterparties = _externalCounterpartyRepository.GetActiveExternalCounterpartiesByPhone(uow, phoneId);
			return externalCounterparties.Any();
		}
		
		public IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByCounterparty(
			IUnitOfWork uow,
			int counterpartyId)
		{
			return _externalCounterpartyRepository.GetActiveExternalCounterpartiesByCounterparty(uow, counterpartyId);
		}
		
		public IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByPhones(
			IUnitOfWork uow,
			IEnumerable<int> phonesIds)
		{
			return _externalCounterpartyRepository.GetActiveExternalCounterpartiesByPhones(uow, phonesIds);
		}
	}
}
