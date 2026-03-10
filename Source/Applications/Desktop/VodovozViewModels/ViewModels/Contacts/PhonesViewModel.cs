using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Tdi;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Contacts;
using VodovozBusiness.Domain.Contacts;
using VodovozBusiness.Services.Clients;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhonesViewModel : WidgetViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private IObservableList<Phone> _phonesList;
		private readonly IContactSettings _contactsParameters;
		private IPhoneRepository phoneRepository;

		public PhonesViewModel(
			IUnitOfWork uow,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			IPhoneTypeSettings phoneTypeSettings,
			IPhoneRepository phoneRepository,
			IContactSettings contactsParameters,
			ICommonServices commonServices,
			IExternalCounterpartyHandler externalCounterpartyHandler)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			ExternalCounterpartyHandler = externalCounterpartyHandler ?? throw new ArgumentNullException(nameof(externalCounterpartyHandler));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			this.phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
			_contactsParameters = contactsParameters ?? throw new ArgumentNullException(nameof(contactsParameters));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_phoneTypeSettings = phoneTypeSettings ?? throw new ArgumentNullException(nameof(phoneTypeSettings));

			var roboAtsCounterpartyNamePermissions = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(RoboAtsCounterpartyName));
			CanReadCounterpartyName = roboAtsCounterpartyNamePermissions.CanRead;
			CanEditCounterpartyName = roboAtsCounterpartyNamePermissions.CanUpdate;

			var roboAtsCounterpartyPatronymicPermissions = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(RoboAtsCounterpartyPatronymic));
			CanReadCounterpartyPatronymic = roboAtsCounterpartyPatronymicPermissions.CanRead;
			CanEditCounterpartyPatronymic = roboAtsCounterpartyPatronymicPermissions.CanUpdate;

			PhoneTypes = phoneRepository.GetPhoneTypes(uow);
			CreateCommands();
		}

		#region Properties

		public INavigationManager NavigationManager { get; }
		public ILifetimeScope Scope { get; }
		public IExternalCounterpartyHandler ExternalCounterpartyHandler { get; }
		public IUnitOfWork UoW { get; }
		public ITdiTab ParentTab { get; private set; }
		public DialogViewModelBase ParentViewModel { get; private set; }
		public bool WithRoboatsWidgets { get; private set; }

		public IList<PhoneType> PhoneTypes;
		public event Action PhonesListReplaced; //Убрать
		public event Action<IObservableList<Phone>> PhonesListChangedAction;
		public DeliveryPoint DeliveryPoint { get; set; }
		public Domain.Client.Counterparty Counterparty { get; set; }

		public virtual IObservableList<Phone> PhonesList
		{
			get => _phonesList;
			set
			{
				var oldList = _phonesList;
				
				if(value is null)
				{
					value = new ObservableList<Phone>();
				}

				if(SetField(ref _phonesList, value))
				{
					PhonesListChangedAction?.Invoke(oldList);
				};
			}
		}

		private bool _readOnly = false;
		private readonly IPhoneTypeSettings _phoneTypeSettings;

		public virtual bool ReadOnly
		{
			get => _readOnly;
			set => SetField(ref _readOnly, value, () => ReadOnly);
		}

		public bool CanReadCounterpartyName { get; }
		public bool CanEditCounterpartyName { get; }
		public bool CanReadCounterpartyPatronymic { get; }
		public bool CanEditCounterpartyPatronymic { get; }
		
		#endregion Prorerties

		#region Methods
		
		public PhoneViewModel GetPhoneViewModel(Phone phone)
		{
			return new PhoneViewModel(
				UoW,
				phone,
				_commonServices,
				_phoneTypeSettings,
				ExternalCounterpartyHandler);
		}
		
		public void Initialize(
			ITdiTab parentTab,
			bool readOnly,
			IObservableList<Phone> phoneList = null,
			IDomainObject phoneType = null,
			bool withRoboatsWidgets = false)
		{
			ParentTab = parentTab;
			Initialize(phoneList, readOnly, phoneType, withRoboatsWidgets);
		}
		
		public void Initialize(
			DialogViewModelBase parentViewModel,
			bool readOnly,
			IObservableList<Phone> phoneList = null,
			IDomainObject phoneType = null,
			bool withRoboatsWidgets = false)
		{
			ParentViewModel = parentViewModel;
			Initialize(phoneList, readOnly, phoneType, withRoboatsWidgets);
		}
		
		private void Initialize(
			IObservableList<Phone> phoneList,
			bool readOnly,
			IDomainObject phoneType = null,
			bool withRoboatsWidgets = false)
		{
			switch (phoneType)
			{
				case Domain.Client.Counterparty clientPhone:
					Counterparty = clientPhone;
					break;
				case DeliveryPoint deliveryPointPhone:
					DeliveryPoint = deliveryPointPhone;
					break;
			}

			_phonesList = phoneList;
			ReadOnly = readOnly;
			WithRoboatsWidgets = withRoboatsWidgets;
		}
		
		#endregion

		#region Commands

		public DelegateCommand AddItemCommand { get; private set; }
		public DelegateCommand<Phone> DeleteItemCommand { get; private set; }

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand(
				() =>
				{
					var phone = new Phone().Init(_contactsParameters);
					phone.DeliveryPoint = DeliveryPoint;
					phone.Counterparty = Counterparty;
					
					PhonesList.Add(phone);
				},
				() => !ReadOnly);

			DeleteItemCommand = new DelegateCommand<Phone>(
				phone =>
				{
					if(ExternalCounterpartyHandler.HasExternalCounterparties(UoW, phone))
					{
						_commonServices.InteractiveService
							.ShowMessage(
								ImportanceLevel.Warning,
								"По данному номеру привязан пользователь ИПЗ(МП, сайта и т.д.) удаление невозможно. Обратитесь в отдель разработки");
						
						return;
					}
					
					PhonesList.Remove(phone);
				},
				phone => !ReadOnly);
		}

		#endregion Commands

		/// <summary>
		/// Необходимо выполнить перед сохранением или в геттере HasChanges
		/// </summary>
		public void RemoveEmpty()
		{
			PhonesList.Where(p => p.DigitsNumber.Length < _contactsParameters.MinSavePhoneLength)
					.ToList().ForEach(p => PhonesList.Remove(p));
		}
	}
}
