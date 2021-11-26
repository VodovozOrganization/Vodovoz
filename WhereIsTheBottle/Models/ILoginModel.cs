using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using QS.MachineConfig;
using QS.Project.Versioning;

namespace WhereIsTheBottle.Models
{
	public interface ILoginModel : INotifyPropertyChanged
	{
		public bool ConnectionInProgress { get; }
		public IList<Connection> Connections { get; }
		public bool SuccessfullyLoggedIn { get; set; }
		/// <summary>
		///     Возвращает активное соединение после успешной настройки базы и логина
		/// </summary>
		public Connection ActiveConnection { get; }
		public IApplicationInfo ApplicationInfo { get; }
		public void Connect(Connection connection, SecureString password);
		public void SaveConnection(Connection connection);
		public Connection GetOrCreateConnection(string server, string baseName, string user);
		public Connection GetDefaultConnection();
		public void ReloadConnections();
		public void SetOverwriteConnectionFromArgs(string[] args);
	}
}
