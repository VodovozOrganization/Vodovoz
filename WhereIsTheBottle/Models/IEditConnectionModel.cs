using System.Collections.Generic;
using QS.MachineConfig;

namespace WhereIsTheBottle.Models
{
	public interface IEditConnectionModel
	{
		public bool ConnectionsSaved { get; set; }
		IList<Connection> GetConnections();
		Connection GetNewConnection();
		void SaveConnections(IList<Connection> connections);
	}
}
