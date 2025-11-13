using Mango.Client;
using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.Dialogs.Mango
{
	public partial class SubscriberSelectionViewModel : WindowDialogViewModelBase
	{
		public DialogType DialogType { get; }

		private MangoManager Manager { get; }

		public List<SearchTableEntity> SearchTableEntities { get; private set; }

		public SubscriberSelectionViewModel(INavigationManager navigation,
			MangoManager manager,
			DialogType dialogType) : base(navigation)
		{
			DialogType = dialogType;
			Manager = manager;

			WindowPosition = WindowGravity.None;

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
}
