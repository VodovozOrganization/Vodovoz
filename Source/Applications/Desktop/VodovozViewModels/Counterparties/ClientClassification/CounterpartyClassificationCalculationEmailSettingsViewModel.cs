﻿using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Counterparties.ClientClassification
{
	public partial class CounterpartyClassificationCalculationEmailSettingsViewModel : WindowDialogViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private string _currentUserEmail;
		private string _additionalEmail;

		public event EventHandler<StartClassificationCalculationEventArgs> StartClassificationCalculationClicked;

		public CounterpartyClassificationCalculationEmailSettingsViewModel(
			INavigationManager navigation,
			IInteractiveService interactiveService,
			string userEmail
			) : base(navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			Title = "Пересчёт классификации";
			_currentUserEmail = 
				string.IsNullOrWhiteSpace(userEmail)
				? string.Empty
				: userEmail;

			StartCalculationCommand = new DelegateCommand(StartCalculation);
		}

		#region Properties

		public DelegateCommand StartCalculationCommand { get; }

		[PropertyChangedAlso(nameof(IsCurrentUserEmailValid))]
		public string CurrentUserEmail
		{
			get => _currentUserEmail;
			set
			{
				value.Trim();

				SetField(ref _currentUserEmail, value);
			}
		}

		[PropertyChangedAlso(nameof(IsAdditionalEmailValid))]
		public string AdditionalEmail
		{
			get => _additionalEmail;
			set
			{
				value.Trim();

				SetField(ref _additionalEmail, value);
			}
		}

		public bool IsCurrentUserEmailValid =>
			string.IsNullOrEmpty(CurrentUserEmail) || new EmailAddressAttribute().IsValid(CurrentUserEmail);

		public bool IsAdditionalEmailValid =>
			string.IsNullOrEmpty(AdditionalEmail) || new EmailAddressAttribute().IsValid(AdditionalEmail);

		#endregion Properties

		private void StartCalculation()
		{
			if(!string.IsNullOrEmpty(CurrentUserEmail)
				&& !new EmailAddressAttribute().IsValid(CurrentUserEmail))
			{
				if(!_interactiveService.Question(
					 $"Введенный основной адрес электронной почты '{CurrentUserEmail}' имеет неправильный формат." +
					 "\nОтчет на данную почту не может быть отправлен." +
					 "\n\nПродолжить?"))
				{
					return;
				}

				CurrentUserEmail = string.Empty;
			}

			if(!string.IsNullOrEmpty(AdditionalEmail)
				&& !new EmailAddressAttribute().IsValid(AdditionalEmail))
			{
				if(!_interactiveService.Question(
					 $"Введенный дополнитель адрес электронной почты '{AdditionalEmail}' имеет неправильный формат." +
					 "\nОтчет на данную почту не может быть отправлен." +
					 "\n\nПродолжить?"))
				{
					return;
				}

				AdditionalEmail = string.Empty;
			}

			if(string.IsNullOrEmpty(CurrentUserEmail) && string.IsNullOrEmpty(AdditionalEmail))
			{
				if(!_interactiveService.Question(
					 $"Вы не ввели ни одного адреса электронной почты." +
					 "\n\nПродолжить?"))
				{
					return;
				}
			}

			var clickedEventArgs = new StartClassificationCalculationEventArgs(CurrentUserEmail, AdditionalEmail);

			StartClassificationCalculationClicked?.Invoke(this, clickedEventArgs);

			this.Close(false, CloseSource.Self);
		}
	}
}
