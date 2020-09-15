using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClientMangoService.Commands;
using ClientMangoService.DTO.Group;
using ClientMangoService.DTO.Users;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango
{
	public class SubscriberSelectionViewModel : WindowDialogViewModelBase
	{
		private Action SuccessfulForward { get; set; }

		#region InnerTypes

		public enum DialogType
		{
			AdditionalCall,
			Telephone
		}
		#endregion
		public readonly DialogType dialogType;
		private MangoManager Manager { get; }

		public List<SearchTableEntity> SearchTableEntities { get; private set; }
		public List<SearchTableEntity> LocalEntities { get; set; }

		public SubscriberSelectionViewModel(INavigationManager navigation,
			MangoManager manager,
			DialogType dialogType) : base(navigation)
		{
			this.dialogType = dialogType;
			Manager = manager;

			LocalEntities = new List<SearchTableEntity>();

			SearchTableEntities = new List<SearchTableEntity>();
			List<User> users = Manager.GetAllVPBXEmploies().ToList();
			foreach(var user in users) {
				SearchTableEntities.Add(new SearchTableEntity(user));
			}

			List<Group> groups = Manager.GetAllVPBXGroups().ToList();
			foreach(var group in groups) {
				SearchTableEntities.Add(new SearchTableEntity(group));
			}
		}

		public SubscriberSelectionViewModel(INavigationManager navigation,
			MangoManager manager,
			DialogType dialogType,
			Action exitAction) : base(navigation)
		{

			this.dialogType = dialogType;
			Manager = manager;

			LocalEntities = new List<SearchTableEntity>();

			SearchTableEntities = new List<SearchTableEntity>();
			List<User> users = Manager.GetAllVPBXEmploies().ToList();
			foreach(var user in users) {
				SearchTableEntities.Add(new SearchTableEntity(user));
			}

			List<Group> groups = Manager.GetAllVPBXGroups().ToList();
			foreach(var group in groups) {
				SearchTableEntities.Add(new SearchTableEntity(group));
			}

			this.SuccessfulForward = exitAction;
		}

		public void MakeCall(string extension)
		{
			Manager.MakeCall(extension);
		}

		public void ForwardCall(string extension,ForwardingMethod method)
		{
			Close(false, CloseSource.Self);
			Manager.ForwardCall(extension, method);
			SuccessfulForward();
		}

	}
	public class SearchTableEntity
	{
		public string Name { get; set; }
		public string Department { get; set; }
		public string Extension { get; set; }
		public bool Status { get; set; }

		public SearchTableEntity(User user)
		{
			Name = user.general.name;
			Department = user.general.department;
			Extension = user.telephony.extension;
			Status = user.telephony.numbers.Any(x => x.status == "on");
		}

		public SearchTableEntity(Group group)
		{
			Name = group.name;
			Department = "";
			Extension = group.extension;
			Status = true;
		}
	}
}
