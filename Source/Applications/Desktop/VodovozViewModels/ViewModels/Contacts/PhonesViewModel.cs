using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.Services;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhonesViewModel : WidgetViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWork _uow;
		private readonly IExternalCounterpartyController _externalCounterpartyController;
		private readonly IContactParametersProvider _contactsParameters;
		
		private bool _readOnly;
		private GenericObservableList<Phone> _phonesList;

		#region Properties

		public IList<PhoneType> PhoneTypes;
		public DeliveryPoint DeliveryPoint { get; set; }
		public Domain.Client.Counterparty Counterparty { get; set; }
		
		public virtual GenericObservableList<Phone> PhonesList
		{
			get => _phonesList;
			set => SetField(ref _phonesList, value);
		}

		public virtual bool ReadOnly
		{
			get => _readOnly;
			set => SetField(ref _readOnly, value);
		}

		public PhonesViewModel(
			IPhoneRepository phoneRepository,
			IUnitOfWork uow,
			IContactParametersProvider contactsParameters,
			ICommonServices commonServices,
			IExternalCounterpartyController externalCounterpartyController)
		{
			_contactsParameters = contactsParameters ?? throw new ArgumentNullException(nameof(contactsParameters));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_externalCounterpartyController =
				externalCounterpartyController ?? throw new ArgumentNullException(nameof(externalCounterpartyController));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));

			var roboAtsCounterpartyNamePermissions = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(RoboAtsCounterpartyName));
			CanReadCounterpartyName = roboAtsCounterpartyNamePermissions.CanRead;
			CanEditCounterpartyName = roboAtsCounterpartyNamePermissions.CanUpdate;

			var roboAtsCounterpartyPatronymicPermissions = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(RoboAtsCounterpartyPatronymic));
			CanReadCounterpartyPatronymic = roboAtsCounterpartyPatronymicPermissions.CanRead;
			CanEditCounterpartyPatronymic = roboAtsCounterpartyPatronymicPermissions.CanUpdate;

			PhoneTypes = phoneRepository.GetPhoneTypes(uow);
			CreateCommands();
		}

		public PhonesViewModel(
			IPhoneRepository phoneRepository,
			IUnitOfWork uow,
			IContactParametersProvider contactsParameters,
			RoboatsJournalsFactory roboatsJournalsFactory,
			ICommonServices commonServices,
			IExternalCounterpartyController externalCounterpartyController)
			: this(phoneRepository, uow, contactsParameters, commonServices, externalCounterpartyController)
		{
			if(roboatsJournalsFactory == null)
			{
				throw new ArgumentNullException(nameof(roboatsJournalsFactory));
			}

			RoboAtsCounterpartyNameSelectorFactory = roboatsJournalsFactory.CreateCounterpartyNameSelectorFactory();
			RoboAtsCounterpartyPatronymicSelectorFactory = roboatsJournalsFactory.CreateCounterpartyPatronymicSelectorFactory();
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
			return new PhoneViewModel(
				phone,
				_uow,
				_commonServices,
				new PhoneTypeSettings(new ParametersProvider()),
				_externalCounterpartyController);
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
					
					if(PhonesList == null)
					{
						PhonesList = new GenericObservableList<Phone>();
					}

					PhonesList.Add(phone);
				},
				() => !ReadOnly
			);

			DeleteItemCommand = new DelegateCommand<Phone>(
				(phone) =>
				{
					if(phone.Id != 0
						&& phone.Counterparty != null
						&& !_externalCounterpartyController.ArchiveExternalCounterparties(_uow, phone.Id))
					{
						return;
					}
					
					PhonesList.Remove(phone);
				},
				phone => !ReadOnly
			);
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
