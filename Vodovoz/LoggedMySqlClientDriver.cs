using System;
using System.Data.Common;
using NHibernate.Driver;
using NLog;

namespace Vodovoz
{
	public class LoggedMySqlClientDriver : MySqlDataDriver
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		
		protected override void OnBeforePrepare(DbCommand command)
		{
			base.OnBeforePrepare(command);
			try {
				logger.Debug(GetResultQueryText(command));
			}
			catch (Exception ex) {
				logger.Error(ex, "Ошибка при формировании текста SQL запроса для логгера");
			}
		}
		
		public string GetResultQueryText(DbCommand dbCommand)
		{
			string parametersText = "";
			foreach (DbParameter parameter in dbCommand.Parameters) {
				parametersText += $"SET @{parameter.ParameterName.TrimStart('?')} = {parameter.Value};{Environment.NewLine}";
			}
			return $"{parametersText}{dbCommand.CommandText.Replace("?p", "@p")};";
		}
	}
}