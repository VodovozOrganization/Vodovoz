namespace Pacs.Core
{
	/// <summary>
	/// Исключение , возникающее при инициализации СКУД.
	/// </summary>
	public class PacsInitException : PacsException
	{
		public PacsInitException(string message) : base(message)
		{
		}
	}
}
