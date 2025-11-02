using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.ViewModels;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Settings.Common;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class EmailsViewModel : WidgetViewModelBase
	{
		private IUnitOfWork _uow;
		private readonly IEmailSettings _emailSettings;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;

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
			IInteractiveService interactiveService,
			PersonType personType)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			InteractiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_personType = personType;
			EmailTypes = _uow.GetAll<EmailType>();

			if(emailList is null)
			{
				throw new ArgumentNullException(nameof(emailList));
			}

			EmailsList = new GenericObservableList<Email>(emailList);
		}
		
		public IInteractiveService InteractiveService { get; }
		public GenericObservableList<Email> EmailsList { get; }
		public IEnumerable<EmailType> EmailTypes { get; }

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
