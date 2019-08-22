using System;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Services;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CallTaskPanelView : Gtk.Bin , IPanelView
	{
		Order Order{ get; set; }

		private IPersonProvider personProvider { get; set; }
		private IEmployeeRepository employeeRepository { get; set; }

		public CallTaskPanelView(IPersonProvider personProvider, IEmployeeRepository employeeRepository)
		{
			this.Build();
			this.personProvider = personProvider;
			this.employeeRepository = employeeRepository;
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
			if(Order.DeliveryPoint == null) {
				MessageDialogHelper.RunInfoDialog("Необходимо выбрать точку доставки");
				return;
			}
			if(String.IsNullOrWhiteSpace(ytextview.Buffer.Text)) {
				MessageDialogHelper.RunInfoDialog("Необходимо оставить комментарий");
				return;
			}

			using(var uow = UnitOfWorkFactory.CreateWithNewRoot<CallTask>("Кнопка «Создать задачу» на панели \"Постановка задачи\"")) 
			{
				CallTaskFactory.GetInstance().CreateTask(uow, employeeRepository, personProvider, uow.Root, Order, ytextview.Buffer.Text);
				uow.Save();
			}
			ytextview.Buffer.Text = String.Empty;
		}

	}
}
