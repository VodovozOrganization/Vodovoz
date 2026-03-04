using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Contacts;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class EmailsViewModel : WidgetViewModelBase
	{
		private readonly IUnitOfWork _uow;
		private readonly IEmailSettings _emailSettings;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IEmailTypeSettings _emailTypeSettings;
		private readonly ICommonServices _commonServices;

		private PersonType _personType;
		private DelegateCommand _addEmailCommand;
		private DelegateCommand<Email> _removeEmailCommand;
		private DelegateCommand<Email> _removeEmailWithAllReferencesCommand;
		private IList<ExternalCounterparty> _externalCounterparties;

		public EmailsViewModel(
			IUnitOfWork uow,
			IList<Email> emailList,
			IEmailSettings emailSettings,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			ICommonServices commonServices,
			PersonType personType,
			IEmailTypeSettings emailTypeSettings
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_emailTypeSettings = emailTypeSettings ?? throw new ArgumentNullException(nameof(emailTypeSettings));
			_personType = personType;

			InteractiveService = _commonServices.InteractiveService;
			EmailTypes = _uow.GetAll<EmailType>();

			if(emailList is null)
			{
				throw new ArgumentNullException(nameof(emailList));
			}

			EmailsList = new GenericObservableList<Email>(emailList);
			ActiveEmails = new List<Email>(emailList.Where(x => x.EmailType == null || x.EmailType.Id != _emailTypeSettings.ArchiveId));
		}

		public readonly IInteractiveService InteractiveService;
		public GenericObservableList<Email> EmailsList { get; }
		public List<Email> ActiveEmails { get; }
		public IEnumerable<EmailType> EmailTypes { get; }

		public void OnEmailTypeChanged(Email email)
		{
			if(email.EmailType != null && email.EmailType.Id == _emailTypeSettings.ArchiveId)
			{
				_commonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Info,
						"Email будет переведен в архив и пропадет в списке активных");
			}
		}


		public DelegateCommand AddEmailCommand => _addEmailCommand ?? (_addEmailCommand = new DelegateCommand(
			() =>
			{
				if(EmailsList.Any())
				{
					AddEmptyEmail();
				}
				else
				{
					if(_personType == PersonType.natural)
					{
						EmailsList.Add(new Email
						{
							EmailType = _uow.GetById<EmailType>(_emailSettings.EmailTypeForReceiptsId)
						});
					}
					else
					{
						AddEmptyEmail();
					}
				}
			}));

		public DelegateCommand<Email> RemoveEmailCommand => _removeEmailCommand ?? (_removeEmailCommand = new DelegateCommand<Email>(
			email =>
			{
				EmailsList.Remove(email);
			}));
		
		public DelegateCommand<Email> RemoveEmailWithAllReferencesCommand => _removeEmailWithAllReferencesCommand ?? (
			_removeEmailWithAllReferencesCommand = new DelegateCommand<Email>(
				email =>
				{
					foreach(var externalCounterparty in _externalCounterparties)
					{
						externalCounterparty.Email = null;
						_uow.Save(externalCounterparty);
					}
					EmailsList.Remove(email);
					_externalCounterparties.Clear();
				}));
		
		/// <summary>
		/// Необходимо выполнить перед сохранением или в геттере HasChanges
		/// </summary>
		public void RemoveEmpty()
		{
			EmailsList.Where(p => string.IsNullOrWhiteSpace(p.Address))
				.ToList()
				.ForEach(p => EmailsList.Remove(p));
		}

		public void UpdatePersonType(PersonType newPersonType)
		{
			_personType = newPersonType;
		}

		public bool HasExternalCounterpartiesWithEmail(int emailId)
		{
			_externalCounterparties = _externalCounterpartyRepository.GetExternalCounterpartyByEmail(_uow, emailId);
			return _externalCounterparties.Any();
		}
		
		private void AddEmptyEmail()
		{
			EmailsList.Add(new Email());
		}
	}
}
