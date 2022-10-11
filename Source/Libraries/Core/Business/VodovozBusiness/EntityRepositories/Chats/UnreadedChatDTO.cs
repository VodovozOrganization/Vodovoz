using Vodovoz.Domain.Chats;

namespace Vodovoz.EntityRepositories.Chats
{
	public class UnreadedChatDTO
	{	
		public int ChatId { get; set; }
		public int UnreadedMessagesTotal { get; set; }
		public int UnreadedMessagesAuto { get; set; }
		public int UnreadedMessages => UnreadedMessagesTotal - UnreadedMessagesAuto;
		public ChatType ChatType { get; set; }
		public int EmployeeId { get; set; }
		public string EmployeeLastName { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }
	}
}