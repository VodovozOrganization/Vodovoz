using System;

namespace Vodovoz.Settings.Warehouse
{
	public interface ICarLoadDocumentLoadingProcessSettings
	{
		/// <summary>
		/// Таймаут бездействия сборщика талона погрузки
		/// </summary>
		TimeSpan NoLoadingActionsTimeout { get; }
	}
}
