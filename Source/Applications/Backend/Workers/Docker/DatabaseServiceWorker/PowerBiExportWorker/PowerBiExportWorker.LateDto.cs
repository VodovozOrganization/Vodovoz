namespace DatabaseServiceWorker
{
	internal partial class PowerBiExportWorker
	{
		internal sealed class LateDto
		{
			internal decimal LessThan5Minutes { get; set; }
			internal decimal LessThan30Minutes { get; set; }
			internal decimal MoreThan30Minutes { get; set; }
		}
	}
}
