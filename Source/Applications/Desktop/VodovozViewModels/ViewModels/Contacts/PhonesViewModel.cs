using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Contacts;
using Vodovoz.ViewModels.Journals.JournalFactories;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhonesViewModel : WidgetViewModelBase
	{
		private ICommonServices commonServices;
		private GenericObservableList<Phone> phonesList;
		private IContactSettings contactsParameters;
		private IPhoneRepository phoneRepository;

		public PhonesViewModel(IPhoneTypeSettings phoneTypeSettings, IPhoneRepository phoneRepository, IUnitOfWork uow, IContactSettings contactsParameters, ICommonServices commonServices)
		{
			this.phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
			this.contactsParameters = contactsParameters ?? throw new ArgumentNullException(nameof(contactsParameters));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
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

		public PhonesViewModel(IPhoneTypeSettings phoneTypeSettings, IPhoneRepository phoneRepository, IUnitOfWork uow, IContactSettings contactsParameters, RoboatsJournalsFactory roboatsJournalsFactory,
			ICommonServices commonServices) : this(phoneTypeSettings, phoneRepository, uow, contactsParameters, commonServices)
		{
			if(roboatsJournalsFactory == null)
			{
				throw new ArgumentNullException(nameof(roboatsJournalsFactory));
			}

			RoboAtsCounterpartyNameSelectorFactory = roboatsJournalsFactory.CreateCounterpartyNameSelectorFactory();
			RoboAtsCounterpartyPatronymicSelectorFactory = roboatsJournalsFactory.CreateCounterpartyPatronymicSelectorFactory();
		}


		#region Properties

		public IList<PhoneType> PhoneTypes;
		public event Action PhonesListReplaced; //Убрать
		public DeliveryPoint DeliveryPoint { get; set; }
		public Domain.Client.Counterparty Counterparty { get; set; }

		public virtual GenericObservableList<Phone> PhonesList
		{
			get => phonesList;
			set
			{
				SetField(ref phonesList, value, () => PhonesList);
				PhonesListReplaced?.Invoke();
			}
		}

		private bool readOnly = false;
		private readonly IPhoneTypeSettings _phoneTypeSettings;

		public virtual bool ReadOnly
		{
			get => readOnly;
			set => SetField(ref readOnly, value, () => ReadOnly);
		}

		


		public IEntityAutocompleteSelectorFactory RoboAtsCounterpartyNameSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory RoboAtsCounterpartyPatronymicSelectorFactory { get; }
		public bool CanReadCounterpartyName { get; }
		public bool CanEditCounterpartyName { get; }
		public bool CanReadCounterpartyPatronymic { get; }
		public bool CanEditCounterpartyPatronymic { get; }


		#endregion Prorerties

		#region Methods
		public PhoneViewModel GetPhoneViewModel(Phone phone)
		{
			return new PhoneViewModel(phone,
				commonServices,
				_phoneTypeSettings);
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
					var phone = new Phone().Init(contactsParameters);
					phone.DeliveryPoint = DeliveryPoint;
					phone.Counterparty = Counterparty;
					if(PhonesList == null)
						PhonesList = new GenericObservableList<Phone>();
					PhonesList.Add(phone);
				},
				() => { return !ReadOnly; }
			);

			DeleteItemCommand = new DelegateCommand<Phone>(
				(phone) =>
				{
					PhonesList.Remove(phone);
				},
				(phone) => { return !ReadOnly; }
			);
		}

		#endregion Commands


		/// <summary>
		/// Необходимо выполнить перед сохранением или в геттере HasChanges
		/// </summary>
		public void RemoveEmpty()
		{
			PhonesList.Where(p => p.DigitsNumber.Length < contactsParameters.MinSavePhoneLength)
					.ToList().ForEach(p => PhonesList.Remove(p));
		}

	}
}
