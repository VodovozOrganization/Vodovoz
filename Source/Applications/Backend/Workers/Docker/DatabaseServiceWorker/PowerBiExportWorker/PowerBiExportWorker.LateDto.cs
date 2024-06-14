namespace DatabaseServiceWorker
{
	internal partial class PowerBiExportWorker
	{
		internal sealed class LateDto
		{
			internal int LessThan5Minutes { get; set; }
			internal int LessThan30Minutes { get; set; }
			internal int MoreThan30Minutes { get; set; }
		}
	}
}
