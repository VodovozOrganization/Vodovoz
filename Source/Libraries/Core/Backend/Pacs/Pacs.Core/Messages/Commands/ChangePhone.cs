namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Команда смены номера внутреннего телефона
	/// </summary>
	public class ChangePhone : OperatorCommand
	{
		/// <summary>
		/// Номер телефона
		/// </summary>
		public string PhoneNumber { get; set; }
	}
}
