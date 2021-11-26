using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.MachineConfig;
using WhereIsTheBottle.Database;
using WhereIsTheBottle.Infrastructure;

namespace WhereIsTheBottle.Models
{
	public class EditConnectionModel : PropertyChangedBase, IEditConnectionModel
	{
		private readonly IConnectionManager _connectionManager;
		private bool _connectionsSaved;

		public EditConnectionModel(IConnectionManager connectionManager)
		{
			_connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
			_connectionsSaved = false;
		}

		public bool ConnectionsSaved
		{
			get => _connectionsSaved;
			set => SetField(ref _connectionsSaved, value);
		}

		public IList<Connection> GetConnections()
		{
			var connections = _connectionManager.LoadConnections();
			if(!connections.Any())
			{
				connections.Add(_connectionManager.CreateConnection());
			}
			return connections;
		}

		public Connection GetNewConnection()
		{
			return _connectionManager.CreateConnection("Новое подключение");
		}

		public void SaveConnections(IList<Connection> connections)
		{
			_connectionManager.SetConnections(connections);
		}
	}
}
