using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Tdi;
using Vodovoz.Controllers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Contacts;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhonesViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWork _uow;
		private readonly IExternalCounterpartyController _externalCounterpartyController;
		private readonly IContactSettings _contactsSettings;
		private readonly IList<PhoneViewModel> _phoneViewModels = new List<PhoneViewModel>();
		private readonly ILifetimeScope _scope;
		private ITdiTab _parentTab;
		private DialogTabViewModelBase _parentViewModel;
		private bool _readOnly;
		private IList<Phone> _phonesList;

		#region Properties

		public IList<PhoneType> PhoneTypes;
		public DeliveryPoint DeliveryPoint { get; set; }
		public Domain.Client.Counterparty Counterparty { get; set; }

		public IList<Phone> PhonesList
		{
			get => _phonesList;
			private set => SetField(ref _phonesList, value);
		}

		public IReadOnlyList<PhoneViewModel> PhoneViewModels => (IReadOnlyList<PhoneViewModel>)_phoneViewModels;

		public bool ReadOnly
		{
			get => _readOnly;
			set => SetField(ref _readOnly, value);
		}

		public event Action UpdateExternalCounterpartyAction;
		public event Action<int> DeletedPhoneAction;
		public event Action<Phone, int> AddedPhoneAction;

		public PhonesViewModel(
			ILifetimeScope scope,
			IPhoneRepository phoneRepository,
			IUnitOfWork uow,
			IContactSettings contactsSettings,
			ICommonServices commonServices,
			IExternalCounterpartyController externalCounterpartyController)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_contactsSettings = contactsSettings ?? throw new ArgumentNullException(nameof(contactsSettings));
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

		public bool CanReadCounterpartyName { get; }
		public bool CanEditCounterpartyName { get; }
		public bool CanReadCounterpartyPatronymic { get; }
		public bool CanEditCounterpartyPatronymic { get; }

		#endregion Prorerties

		#region Methods
		
		//Для телефонов, где не надо отображать имена и отчества для roboats
		public void Initialize(IList<Phone> phones)
		{
			SetPhones(phones);
			InitializeViewModels();
		}
		
		public void Initialize(IList<Phone> phones, ITdiTab parentTab)
		{
			SetPhones(phones);
			_parentTab = parentTab;
			InitializeViewModels();
		}
		
		public void Initialize(IList<Phone> phones, DialogTabViewModelBase parentViewModel)
		{
			SetPhones(phones);
			_parentViewModel = parentViewModel;
			InitializeViewModels();
		}

		private void SetPhones(IList<Phone> phones)
		{
			PhonesList = phones;
		}
		
		private void InitializeViewModels()
		{
			if(PhonesList is null && _phoneViewModels.Any())
			{
				RemoveViewModels();
				return;
			}

			if(PhonesList is null)
			{
				return;
			}

			RemoveViewModels();
			AddNewViewModels();
		}

		private void RemoveViewModels()
		{
			const int i = 0;
				
			while(i < _phoneViewModels.Count)
			{
				RemoveViewModel(_phoneViewModels[i], i);
			}
		}

		private void AddNewViewModels()
		{
			var externalCounterparties =
				_externalCounterpartyController.GetActiveExternalCounterpartiesByPhones(_uow, PhonesList.Select(x => x.Id).ToArray())
					.ToLookup(x => x.PhoneId);
			
			for(var i = 0; i< PhonesList.Count; i++)
			{
				var phone = PhonesList[i];
				var canEditPhone = !externalCounterparties.Contains(phone.Id);
				AddNewViewModel(phone, i, canEditPhone);
			}
		}

		private void AddNewViewModel(Phone phone, int phoneIndex, bool canEditPhone = true)
		{
			var viewModel = _scope.Resolve<PhoneViewModel>(new TypedParameter(typeof(IUnitOfWork), _uow));

			if(_parentViewModel != null)
			{
				viewModel.Initialize(phone, canEditPhone, _parentViewModel);
			}
			else
			{
				viewModel.Initialize(phone, canEditPhone, _parentTab);
			}
			
			viewModel.UpdateExternalCounterpartyAction += OnUpdateExternalCounterparty;
			_phoneViewModels.Add(viewModel);
			AddedPhoneAction?.Invoke(phone, phoneIndex);
		}
		
		private void RemoveViewModel(PhoneViewModel viewModel, int index)
		{
			if(viewModel is null)
			{
				return;
			}
					
			viewModel.UpdateExternalCounterpartyAction -= OnUpdateExternalCounterparty;
			viewModel.Dispose();
			_phoneViewModels.Remove(viewModel);
			DeletedPhoneAction?.Invoke(index);
		}

		private void OnUpdateExternalCounterparty()
		{
			UpdateExternalCounterpartyAction?.Invoke();
		}

		#endregion

		#region Commands

		public DelegateCommand AddItemCommand { get; private set; }
		public DelegateCommand<int> DeleteItemCommand { get; private set; }

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand(
				() =>
				{
					var phone = new Phone().Init(_contactsSettings);
					phone.DeliveryPoint = DeliveryPoint;
					phone.Counterparty = Counterparty;
					
					if(PhonesList == null)
					{
						PhonesList = new GenericObservableList<Phone>();
					}

					PhonesList.Add(phone);
					var phoneIndex = PhonesList.Count - 1;
					AddNewViewModel(phone, phoneIndex);
				},
				() => !ReadOnly
			);

			DeleteItemCommand = new DelegateCommand<int>(
				index =>
				{
					var phone = PhonesList[index];
					
					if(phone.Id != 0
						&& phone.Counterparty != null
						&& !_externalCounterpartyController.DeleteExternalCounterparties(_uow, phone.Id))
					{
						return;
					}
					
					PhonesList.Remove(phone);
					OnUpdateExternalCounterparty();
					
					var viewModel = _phoneViewModels[index];
					RemoveViewModel(viewModel, index);
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
			PhonesList.Where(p => p.DigitsNumber.Length < _contactsSettings.MinSavePhoneLength)
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
