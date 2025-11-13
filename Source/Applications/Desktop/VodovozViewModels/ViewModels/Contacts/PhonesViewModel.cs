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
using Vodovoz.Settings.Contacts;
using Vodovoz.ViewModels.Journals.JournalFactories;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhonesViewModel : WidgetViewModelBase
	{
		private readonly IUnitOfWork _uow;
		private readonly IExternalCounterpartyController _externalCounterpartyController;
		private readonly ICommonServices _commonServices;
		private readonly IContactSettings _contactSettings;
		private readonly IPhoneTypeSettings _phoneTypeSettings;

		private bool _readOnly;
		private GenericObservableList<Phone> _phonesList;
		private IList<PhoneViewModel> _phoneViewModels = new List<PhoneViewModel>();

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
			ICommonServices commonServices,
			IPhoneRepository phoneRepository,
			IUnitOfWork uow,
			IContactSettings contactSettings,
			IPhoneTypeSettings phoneTypeSettings,
			IExternalCounterpartyController externalCounterpartyController)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_contactSettings = contactSettings ?? throw new ArgumentNullException(nameof(contactSettings));
			_phoneTypeSettings = phoneTypeSettings ?? throw new ArgumentNullException(nameof(phoneTypeSettings));
			_externalCounterpartyController =
				externalCounterpartyController ?? throw new ArgumentNullException(nameof(externalCounterpartyController));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));

			var currentPermissionService = commonServices.CurrentPermissionService;
			var roboAtsCounterpartyNamePermissions = currentPermissionService.ValidateEntityPermission(typeof(RoboAtsCounterpartyName));
			CanReadCounterpartyName = roboAtsCounterpartyNamePermissions.CanRead;
			CanEditCounterpartyName = roboAtsCounterpartyNamePermissions.CanUpdate;

			var roboAtsCounterpartyPatronymicPermissions = currentPermissionService.ValidateEntityPermission(typeof(RoboAtsCounterpartyPatronymic));
			CanReadCounterpartyPatronymic = roboAtsCounterpartyPatronymicPermissions.CanRead;
			CanEditCounterpartyPatronymic = roboAtsCounterpartyPatronymicPermissions.CanUpdate;

			PhoneTypes = phoneRepository.GetPhoneTypes(uow);
			CreateCommands();
		}

		public PhonesViewModel(
			ICommonServices commonServices,
			IPhoneRepository phoneRepository,
			IUnitOfWork uow,
			IContactSettings contactSettings,
			IPhoneTypeSettings phoneTypeSettings,
			RoboatsJournalsFactory roboatsJournalsFactory,
			IExternalCounterpartyController externalCounterpartyController)
			: this(commonServices, phoneRepository, uow, contactSettings, phoneTypeSettings, externalCounterpartyController)
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
			var viewModel = new PhoneViewModel(
				phone,
				_uow,
				_commonServices,
				_phoneTypeSettings,
				_externalCounterpartyController);

			_phoneViewModels.Add(viewModel);
			viewModel.IsPhoneNumberEditable = !ReadOnly;

			return viewModel;
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
					var phone = new Phone().Init(_contactSettings);
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
					if(_externalCounterpartyController.CheckActiveExternalCounterparties(_uow, phone))
					{
						return;
					}
					
					PhonesList.Remove(phone);
					
					var viewModel = _phoneViewModels.SingleOrDefault(x => x.GetPhone() == phone);

					if(viewModel is null)
					{
						return;
					}
					
					_phoneViewModels.Remove(viewModel);
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
			PhonesList.Where(p => p.DigitsNumber.Length < _contactSettings.MinSavePhoneLength)
				.ToList().ForEach(p => PhonesList.Remove(p));
		}
	}
}
