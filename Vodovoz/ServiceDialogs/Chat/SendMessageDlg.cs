using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Chats;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Chats;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.ServiceDialogs.Chat
{
	public partial class SendMessageDlg : Gtk.Dialog
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IChatMessageRepository _chatMessageRepository = new ChatMessageRepository();
		private readonly IChatRepository _chatRepository = new ChatRepository();
		IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();
		IList<Employee> Recipients;
		
		public SendMessageDlg (int[] recipientsIds, IEmployeeRepository employeeRepository)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			
			Build ();

			Recipients = UoW.GetById<Employee> (recipientsIds);
			labelRecipients.LabelProp = String.Join (", ", Recipients.Select(x => x.ShortName));
			textviewMessage.Buffer.Changed += Buffer_Changed;
		}

		void Buffer_Changed (object sender, EventArgs e)
		{
			buttonOk.Sensitive = !String.IsNullOrWhiteSpace (textviewMessage.Buffer.Text);
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			var service = new ChannelFactory<IChatService>(
				new BasicHttpBinding(), 
				ChatMain.ChatServiceUrl).CreateChannel();
				
			var accessToLogisticChat = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
			var unreadedMessages = _chatMessageRepository.GetUnreadedChatMessages(UoW, currentEmployee, accessToLogisticChat);
			bool needCommit = false;

			foreach(var recipient in Recipients)
			{
				service.SendMessageToDriver(
					currentEmployee.Id,
					recipient.Id,
					textviewMessage.Buffer.Text
				);
				
				var unreaded = unreadedMessages.FirstOrDefault (x => x.EmployeeId == recipient.Id);
				
				if(unreaded == null)
				{
					var chat = _chatRepository.GetChatForDriver(UoW, recipient);
					if(chat != null)
					{
						chat.UpdateLastReadedTime (currentEmployee);
						UoW.Save(chat);
						needCommit = true;
					}
				}
				if (needCommit)
					UoW.Commit ();
			}

		}
	}
}
