using System;
using QSContacts;
using System.Collections.Generic;
using QS.Services;
using QS.ViewModels;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.ViewModelBased;
using System.Linq;

namespace Vodovoz.Dialogs.Phones
{
	public class PhonesViewModel : WidgetViewModelBase
	{

		#region Properties

		public IList<PhoneType> PhoneTypes;
		public event Action PhonesListReplaced;

		private GenericObservableList<Phone> phonesList ;
		public virtual GenericObservableList<Phone> PhonesList {
			get => phonesList;
			set {
				SetField(ref phonesList, value, () => PhonesList);
				PhonesListReplaced?.Invoke();
			} 
		}

		private bool readOnly = false;
		public virtual bool ReadOnly {
			get => readOnly;
			set => SetField(ref readOnly, value, () => ReadOnly);
		}

		public PhonesViewModel(IInteractiveService interactiveService,IUnitOfWork uow):base(interactiveService)
		{
			PhoneTypes = PhoneTypeRepository.GetPhoneTypes(uow);
			CreateCommands();
		}

		#endregion Prorerties

		#region Commands

		public DelegateCommand AddItemCommand { get; private set; }
		public DelegateCommand<Phone> DeleteItemCommand { get; private set; }

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand(
				() => {
					var phone = new Phone();
					if(PhonesList == null)
						PhonesList = new GenericObservableList<Phone>();
					PhonesList.Add(phone);
				},
				() => { return !ReadOnly; }
			);

			DeleteItemCommand = new DelegateCommand<Phone>(
				(phone) => {
					PhonesList.Remove(phone);
				},
				(phone) => { return !ReadOnly;}
			);
		}

		#endregion Commands


		/// <summary>
		/// Необходимо выполнить перед сохранением или в геттере HasChanges
		/// </summary>
		public void RemoveEmpty()
		{
			PhonesList.Where(p => p.Number.Length < QSContactsMain.MinSavePhoneLength)
					.ToList().ForEach(p => PhonesList.Remove(p));
		}

	}
}
