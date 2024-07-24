using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog;
using Vodovoz.Controllers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Contacts;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhonesViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly IPhoneTypeSettings _phoneTypeSettings;
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWork _uow;
		private readonly IExternalCounterpartyController _externalCounterpartyController;
		private readonly IContactSettings _contactSettings;
		
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

		public event Action UpdateExternalCounterpartyAction;

		public PhonesViewModel(
			IPhoneTypeSettings phoneTypeSettings,
			IPhoneRepository phoneRepository,
			IUnitOfWork uow,
			IContactSettings contactSettings,
			ICommonServices commonServices,
			IExternalCounterpartyController externalCounterpartyController)
		{
			_phoneTypeSettings = phoneTypeSettings ?? throw new ArgumentNullException(nameof(phoneTypeSettings));
			_contactSettings = contactSettings ?? throw new ArgumentNullException(nameof(contactSettings));
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
			IPhoneTypeSettings phoneTypeSettings,
			IPhoneRepository phoneRepository,
			IUnitOfWork uow,
			IContactSettings contactSettings,
			RoboatsJournalsFactory roboatsJournalsFactory,
			ICommonServices commonServices,
			IExternalCounterpartyController externalCounterpartyController)
			: this(phoneTypeSettings, phoneRepository, uow, contactSettings, commonServices, externalCounterpartyController)
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

			viewModel.UpdateExternalCounterpartyAction += OnUpdateExternalCounterparty;
			_phoneViewModels.Add(viewModel);

			return viewModel;
		}

		private void OnUpdateExternalCounterparty()
		{
			UpdateExternalCounterpartyAction?.Invoke();
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
					if(phone.Id != 0
						&& phone.Counterparty != null
						&& !_externalCounterpartyController.CanArchiveOrDeletePhone(phone.ExternalCounterpartiesIds))
					{
						_commonServices.InteractiveService.ShowMessage(
							ImportanceLevel.Warning,
							"Недостаточно прав для удаления телефона");
						return;
					}
					
					if(phone.Id != 0
						&& phone.Counterparty != null
						&& !_externalCounterpartyController.TryDeleteExternalCounterparties(_uow, phone.ExternalCounterpartiesIds, true))
					{
						return;
					}
					
					PhonesList.Remove(phone);
					OnUpdateExternalCounterparty();
					
					var viewModel = _phoneViewModels.SingleOrDefault(x => x.GetPhone() == phone);

					if(viewModel is null)
					{
						return;
					}
					
					viewModel.UpdateExternalCounterpartyAction -= OnUpdateExternalCounterparty;
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

		public void Dispose()
		{
			foreach(var item in _phoneViewModels)
			{
				item.UpdateExternalCounterpartyAction -= OnUpdateExternalCounterparty;
			}
			
			_phoneViewModels.Clear();
		}
	}
}
