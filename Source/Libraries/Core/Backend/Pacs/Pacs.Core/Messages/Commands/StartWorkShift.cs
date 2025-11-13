namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Команда начала смены
	/// </summary>
	public class StartWorkShift : OperatorCommand
	{
		/// <summary>
		/// Внутренний номер телефона
		/// </summary>
		public string PhoneNumber { get; set; }
	}
}
