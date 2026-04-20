namespace Mango.Contracts.V1.Options
{
	public class DatabaseOptions
	{
		public const string SectionName = "ClickHouse";

		public string ConnectionString { get; set; } = string.Empty;
		
		public string CallsTableName { get; set; } = "default.mango_calls_analytics";
		
		public string SyncStateTableName { get; set; } = "default.mango_sync_state";
		
		public int InsertBatchSize { get; set; } = 1000;
	}
}
