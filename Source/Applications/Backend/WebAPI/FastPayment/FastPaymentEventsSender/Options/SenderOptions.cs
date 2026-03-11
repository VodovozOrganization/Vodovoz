using QS.DomainModel.UoW;

namespace FastPaymentEventsSender.Options
{
	/// <summary>
	/// Настройки воркера
	/// </summary>
	public class SenderOptions
	{
		public const string Path = "SenderOptions";
		/// <summary>
		/// Задержка между запусками в секундах
		/// </summary>
		public int DelayInSeconds { get; set; }
	}
}
