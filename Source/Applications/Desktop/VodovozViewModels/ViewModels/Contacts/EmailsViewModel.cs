using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Parameters;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class EmailsViewModel : WidgetViewModelBase
	{
		private IUnitOfWork _uow;
		private readonly IEmailParametersProvider _emailParametersProvider;
		
		private PersonType _personType;
		private DelegateCommand _addEmailCommand;
		private DelegateCommand<Email> _removeEmailCommand;

		public EmailsViewModel(
			IUnitOfWork uow,
			IList<Email> emailList,
			IEmailParametersProvider emailParametersProvider,
			PersonType personType)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
			_personType = personType;
			EmailTypes = _uow.GetAll<EmailType>();

			if(emailList is null)
			{
				throw new ArgumentNullException(nameof(emailList));
			}

			EmailsList = new GenericObservableList<Email>(emailList);
		}
		
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
							EmailType = _uow.GetById<EmailType>(_emailParametersProvider.EmailTypeForReceiptsId)
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
		
		private void AddEmptyEmail()
		{
			EmailsList.Add(new Email());
		}
	}
}
