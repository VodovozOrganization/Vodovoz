using QS.Project.DB;
using System.Data.Common;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Драйвер как на проде, который запоминает каждую команду прямо перед выполнением.
	/// </summary>
	public class CountingDriver : LoggedMySqlClientDriver
	{
		protected override void OnBeforePrepare(DbCommand command)
		{
			base.OnBeforePrepare(command);
			SqlLog.Record(command);
		}
	}
}
