using System;
using System.Collections.Generic;
using System.Linq;
using MangoService;
using MangoService.DTO.Group;
using MangoService.DTO.Users;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango
{
	public class SubscriberSelectionViewModel : WindowDialogViewModelBase
	{
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

		public SubscriberSelectionViewModel(INavigationManager navigation,
			MangoManager manager,
			DialogType dialogType) : base(navigation)
		{
			this.dialogType = dialogType;
			Manager = manager;

			SearchTableEntities = new List<SearchTableEntity>();
			List<User> users = Manager.GetAllVPBXEmploies().Where(x => !String.IsNullOrEmpty(x.telephony.extension)).ToList();
			foreach(var user in users) {
				SearchTableEntities.Add(new SearchTableEntity(user));
			}

			List<Group> groups = Manager.GetAllVPBXGroups().Where(x => !String.IsNullOrEmpty(x.extension)).ToList();
			foreach(var group in groups) {
				SearchTableEntities.Add(new SearchTableEntity(group));
			}

			Title = dialogType == DialogType.Telephone ? "Вызов абонента" : "Перевод звонка на абонента";
		}

		public void MakeCall(SearchTableEntity extension)
		{
			Manager.MakeCall(extension.Extension);
		}

		public void ForwardCall(SearchTableEntity extension, ForwardingMethod method)
		{
			NavigationManager.AskClosePage(NavigationManager.FindPage(this));
			Manager.ForwardCall(extension.Extension, method);

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
