using System.Collections.Generic;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango
{
	public class SubscriberSelectionViewModel : WindowDialogViewModelBase
	{
		#region InnerTypes
		public enum ForwardingMethod
		{
			hold,
			blind
		}

		public enum DialogType
		{
			AdditionalCall,
			Telephone
		}
		#endregion
		public readonly DialogType dialogType;
		private MangoManager Manager { get; }

		public List<ClientMangoService.DTO.Users.User> Users { get; private set; }


		public SubscriberSelectionViewModel(INavigationManager navigation,
			MangoManager manager,
			DialogType dialogType) : base(navigation)
		{
			this.dialogType = dialogType;
			Manager = manager;
			Users = new List<ClientMangoService.DTO.Users.User>();
			Users.AddRange(manager.GetAllVPBXEmploies());
		}

		public void MakeCall(string extension)
		{
			Manager.MakeCall(extension);
		}

		public void ForwardCall(string extension,ForwardingMethod method)
		{
			NavigationManager.AskClosePage(NavigationManager.FindPage(this));
			if(method == ForwardingMethod.blind)
				Manager.ForwardCall(extension, "blind");
			else if(method == ForwardingMethod.hold)
				Manager.ForwardCall(extension, "hold");

		}

	}
}
