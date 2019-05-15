using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CallTaskPanelView : Gtk.Bin , IPanelView
	{
		Order Order{ get; set; }

		public CallTaskPanelView()
		{
			this.Build();
		}

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel {get {return true;} }

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			if(InfoProvider is ICallTaskProvider callTaskProvider) 
				Order = callTaskProvider.Order;
			if(Order == null)
				return;

			buttonCreateTask.Sensitive = true;
		}

		#endregion

		protected void OnButtonCreateTaskClicked(object sender, EventArgs e)
		{
			using(var uow = UnitOfWorkFactory.CreateWithNewRoot<CallTask>("Кнопка «Создать задачу» на панели \"Постановка задачи\"")) 
			{
				uow.Root.DeliveryPoint = Order?.DeliveryPoint;
				uow.Root.Counterparty = Order?.Client;
				uow.Root.CreationDate = DateTime.Now;
				uow.Root.TaskCreator = EmployeeRepository.GetEmployeeForCurrentUser(InfoProvider.UoW);
				uow.Root.EndActivePeriod = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
				uow.Root.AddComment(uow, ytextview.Buffer.Text);
				uow.Save();
			}
			ytextview.Buffer.Text = String.Empty;
		}

	}
}
