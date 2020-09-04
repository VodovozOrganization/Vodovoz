using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.ViewModels.Mango
{
	public class TestMangoViewModel : DialogViewModelBase
	{
		MangoManager manager;
		public TestMangoViewModel(
			INavigationManager navigation,
			MangoManager manager) : base(navigation) { this.manager = manager; }

		public void HangUp() { }

		public void GetAllVPBXEmploies()
		{
			manager.GetAllVPBXEmploies();
		}

	}
}
