using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Utilities.Numeric;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Counterparties;
using VodovozBusiness.Services.Clients;

namespace Vodovoz.Core.Application.Clients
{
	/// <inheritdoc/>
	public class ExternalCounterpartyHandler : IExternalCounterpartyHandler
	{
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IExternalCounterpartyMatchingRepository _matchingRepository;
		private readonly IExternalCounterpartyAssignNotificationRepository _notificationRepository;
		private readonly ICurrentPermissionService _permissionService;

		public ExternalCounterpartyHandler(
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IExternalCounterpartyMatchingRepository matchingRepository,
			IExternalCounterpartyAssignNotificationRepository notificationRepository,
			ICurrentPermissionService permissionService)
		{
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_matchingRepository = matchingRepository ?? throw new ArgumentNullException(nameof(matchingRepository));
			_notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
			_permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
		}

		/// <inheritdoc/>
		public bool HasExternalCounterparties(IUnitOfWork uow, Phone phone)
		{
			if(phone is null || phone.Id == 0 || phone.Counterparty is null)
			{
				return false;
			}
			
			if(_externalCounterpartyRepository.HasExternalCounterparties(uow, phone.Id))
			{
				return true;
			}
			
			return false;
		}

		/// <inheritdoc/>
		public void DeleteExternalCounterpartiesForArchivedPhone(IUnitOfWork uow, Phone phone)
		{
			if(phone == null || phone.Id == 0 || phone.Counterparty == null || !phone.IsArchive)
			{
				return;
			}

			var externalCounterparties = _externalCounterpartyRepository.GetByPhoneId(uow, phone.Id);
			if(!externalCounterparties.Any())
			{
				return;
			}

			if(!_permissionService.ValidatePresetPermission(CounterpartyPermissions.CanArchivePhoneWithExternalCounterparties)
				|| !_permissionService.ValidateEntityPermission(typeof(Phone)).CanUpdate)
			{
				throw new InvalidOperationException("Недостаточно прав для архивации телефона, привязанного к пользователям ИПЗ.");
			}

			var formatter = new PhoneFormatter(PhoneFormat.DigitsTen);
			var externalCounterpartyIds = externalCounterparties.Select(ec => ec.Id).ToArray();
			var matchings = _matchingRepository.GetForExternalCounterparties(uow, externalCounterpartyIds)
				.Where(m => m.AssignedExternalCounterparty != null
					|| formatter.FormatString(m.PhoneNumber ?? string.Empty) == phone.DigitsNumber)
				.ToList();
			var notifications = _notificationRepository.GetByExternalCounterpartyIds(
				uow, externalCounterpartyIds);

			foreach(var notification in notifications)
			{
				uow.Delete(notification);
			}

			foreach(var matching in matchings)
			{
				uow.Delete(matching);
			}

			foreach(var externalCounterparty in externalCounterparties)
			{
				uow.Delete(externalCounterparty);
			}
		}
	}
}
