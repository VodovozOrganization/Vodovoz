using Microsoft.Extensions.Logging;
using MySqlConnector;
using QS.Project.DB;
using QS.Report;
using System;
using System.Collections.Generic;
using Vodovoz.Settings.Common;

namespace VodovozInfrastructure.Report
{
	public class SlaveDbPreferredReportInfoFactory : IReportInfoFactory
	{
		private readonly ILogger<SlaveDbPreferredReportInfoFactory> _logger;
		private readonly ISlaveDbConnectionSettings _connectionSettings;
		private readonly IDataBaseInfo _dataBaseInfo;
		private readonly MySqlConnectionStringBuilder _masterConnectionStringBuilder;

		public SlaveDbPreferredReportInfoFactory(
			ILogger<SlaveDbPreferredReportInfoFactory> logger,
			ISlaveDbConnectionSettings connectionSettings,
			IDataBaseInfo dataBaseInfo,
			MySqlConnectionStringBuilder masterConnectionStringBuilder
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
			_dataBaseInfo = dataBaseInfo ?? throw new ArgumentNullException(nameof(dataBaseInfo));
			_masterConnectionStringBuilder = masterConnectionStringBuilder ?? throw new ArgumentNullException(nameof(masterConnectionStringBuilder));
		}

		public ReportInfo Create()
		{
			return new ReportInfo(GetConnectonString());
		}

		public ReportInfo Create(string identifier, string title, Dictionary<string, object> parameters)
		{
			return new ReportInfo(GetConnectonString())
			{
				Identifier = identifier,
				Title = title,
				Parameters = parameters
			};
		}

		private string GetConnectonString()
		{
			var slaveEnabled = _connectionSettings.SlaveConnectionEnabled
					&& _dataBaseInfo.Name == _connectionSettings.SlaveConnectionEnabledForThisDatabase;

			var slaveHost = _connectionSettings.SlaveConnectionHost;
			var slavePort = _connectionSettings.SlaveConnectionPort;
			var hasSlaveHost = !string.IsNullOrWhiteSpace(slaveHost) && slavePort > 0;

			if(!slaveEnabled || !hasSlaveHost)
			{
				_logger.LogWarning("Для отчетов будет использовано Master подключение, так как Slave " +
					"подключение настроено не правильно или не настроено: {slaveHost}:{slavePort} (Host:port)", slaveHost, slavePort);
				return _masterConnectionStringBuilder.ConnectionString;
			}

			if(slavePort != (int)_masterConnectionStringBuilder.Port)
			{
				_logger.LogWarning("Для отчетов будет использовано Master подключение, так как " +
					"различные порты master ({slavePort}) и slave ({masterPort}) базы данных не поддерживаются." +
					"Подробнее: https://mysqlconnector.net/connection-options/", slavePort, _masterConnectionStringBuilder.Port);
				return _masterConnectionStringBuilder.ConnectionString;
			}

			var slaveConnectionStringBuilder = new MySqlConnectionStringBuilder(_masterConnectionStringBuilder.ConnectionString);
			slaveConnectionStringBuilder.Server = $"{slaveHost},{_masterConnectionStringBuilder.Server}";
			slaveConnectionStringBuilder.LoadBalance = MySqlLoadBalance.FailOver;

			return slaveConnectionStringBuilder.ConnectionString;
		}
	}
}
