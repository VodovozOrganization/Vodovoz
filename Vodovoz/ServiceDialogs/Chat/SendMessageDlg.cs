using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Repository.Chats;
using VodovozService.Chats;

namespace Vodovoz.ServiceDialogs.Chat
{
	public partial class SendMessageDlg : Gtk.Dialog
	{
		
		IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();
		IList<Employee> Recipients;
		
		public SendMessageDlg (int[] recipientsIds)
		{
			this.Build ();

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
			var currentEmployee = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			var service = new ChannelFactory<IChatService>(
				new BasicHttpBinding(), 
				ChatMain.ChatServiceUrl).CreateChannel();
				
			var accessToLogisticChat = QSMain.User.Permissions["logistican"];
			var unreadedMessages = ChatMessageRepository.GetUnreadedChatMessages(UoW, currentEmployee, accessToLogisticChat);
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
					var chat = ChatRepository.GetChatForDriver (UoW, recipient);
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
