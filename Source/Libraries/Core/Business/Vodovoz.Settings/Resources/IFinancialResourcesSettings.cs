namespace Vodovoz.Settings.Resources
{
	/// <summary>
	/// Настройки для финансовых ресурсов
	/// </summary>
	public interface IFinancialResourcesSettings
	{
		/// <summary>
		/// Директория для хранения выписок из банка
		/// </summary>
		string BankStatementsDirectory { get; }
	}
}
