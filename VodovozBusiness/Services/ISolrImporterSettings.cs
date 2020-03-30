using System;
namespace Vodovoz.Services
{
	public interface ISolrImporterSettings
	{
		/// <summary>
		/// Имя базы данных при работе с которой возможна работа с сервисом SolrImporterService
		/// </summary>
		string WorkDatabaseName { get; }

		string ServerAddress { get; }
		string ServerPort { get; }
	}
}
