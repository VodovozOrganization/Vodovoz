using Mango.Client;
using MangoService;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.Dialogs.Mango
{
	public class SubscriberSelectionViewModel : WindowDialogViewModelBase
	{
		#region InnerTypes

		public enum DialogType
		{
			Transfer,
			Telephone
		}
		#endregion
		public readonly DialogType dialogType;
		private MangoManager Manager { get; }

		public List<SearchTableEntity> SearchTableEntities { get; private set; }

		public SubscriberSelectionViewModel(INavigationManager navigation,
			MangoManager manager,
			DialogType dialogType) : base(navigation)
		{
			this.dialogType = dialogType;
			Manager = manager;

			SearchTableEntities = Manager.GetPhoneBook().Select(x => new SearchTableEntity(x)).ToList();

			Title = dialogType == DialogType.Telephone ? "Вызов абонента" : "Перевод звонка на абонента";
		}

		public void MakeCall(SearchTableEntity extension)
		{
			Manager.MakeCall(extension.Extension);
			Close(true, CloseSource.Self);
		}

		public void MakeCall(string number)
		{
			Manager.MakeCall(number);
			Close(true, CloseSource.Self);
		}

		public void ForwardCall(SearchTableEntity extension, ForwardingMethod method)
		{
			Manager.ForwardCall(extension.Extension, method);
			Close(true, CloseSource.Self);
		}
	}
	public class SearchTableEntity
	{
		public string Name { get; set; }
		public string Department { get; set; }
		public string Extension { get; set; }
		public bool IsReady { get; set; }
		public bool IsGroup { get; set; }

		public SearchTableEntity(PhoneEntry phoneEntry)
		{
			Name = phoneEntry.Name;
			Department = phoneEntry.Department;
			Extension = phoneEntry.Extension.ToString();
			IsReady = phoneEntry.PhoneState == PhoneState.Ready;
			IsGroup = phoneEntry.PhoneType == PhoneEntryType.Group;
		}
	}
}
