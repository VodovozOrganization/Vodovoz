using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using QS.Views.Dialog;
using Gtk;
using Vodovoz.Views.Mango;
using System.Collections.Generic;
using Vodovoz.Dialogs;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Employees;
using Vodovoz.JournalViewModels;
using Vodovoz.Filters.ViewModels;
using QS.Project.Services;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Representations;
using Vodovoz.Reports;
using Vodovoz.Services.Permissions;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.EntityRepositories.Store;
using QS.Project.Journal;
using QSReport;
using Vodovoz.Domain.Contacts;
using Vodovoz.Dialogs.Sale;
using Vodovoz.JournalNodes;
using QS.Dialog;

using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango
{
	public class SubscriberSelectionViewModel : WindowDialogViewModelBase
	{
		private MangoManager Manager { get; }

		public List<ClientMangoService.DTO.Users.User> Users { get;private set; }

		public SubscriberSelectionViewModel(
		INavigationManager navigation,
			MangoManager manager) : base(navigation)
		{
			Manager = manager;
			Users = new List<ClientMangoService.DTO.Users.User>();
			Users.AddRange(manager.GetAllVPBXEmploies());
	}
	}
}
