﻿using System;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Services;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CallTaskPanelView : Gtk.Bin, IPanelView
	{
		private readonly IPersonProvider _personProvider;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IPermissionResult _callTaskPermissionResult;
		private Order _order;

		public CallTaskPanelView(IPersonProvider personProvider, IEmployeeRepository employeeRepository, ICommonServices commonServices)
		{
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}
			_personProvider = personProvider ?? throw new ArgumentNullException(nameof(personProvider));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			Build();
			_callTaskPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(CallTask));
		}

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => true;

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			if(InfoProvider is ICallTaskProvider callTaskProvider)
			{
				_order = callTaskProvider.Order;
			}
			if(_order == null)
			{
				return;
			}

			ytextview.Sensitive = buttonCreateTask.Sensitive = _callTaskPermissionResult.CanCreate;
		}

		#endregion

		protected void OnButtonCreateTaskClicked(object sender, EventArgs e)
		{
			if(_order.DeliveryPoint == null)
			{
				MessageDialogHelper.RunInfoDialog("Необходимо выбрать точку доставки");
				return;
			}
			if(String.IsNullOrWhiteSpace(ytextview.Buffer.Text))
			{
				MessageDialogHelper.RunInfoDialog("Необходимо оставить комментарий");
				return;
			}

			using(var uow = UnitOfWorkFactory.CreateWithNewRoot<CallTask>("Кнопка «Создать задачу» на панели \"Постановка задачи\""))
			{
				CallTaskSingletonFactory.GetInstance()
					.CreateTask(uow, _employeeRepository, _personProvider, uow.Root, _order, ytextview.Buffer.Text);
				uow.Root.Source = TaskSource.OrderPanel;
				uow.Save();
			}
			ytextview.Buffer.Text = String.Empty;
		}
	}
}
