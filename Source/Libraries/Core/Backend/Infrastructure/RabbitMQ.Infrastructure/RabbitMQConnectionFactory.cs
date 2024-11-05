﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Threading.Tasks;

namespace RabbitMQ.Infrastructure
{
	public class RabbitMQConnectionFactory
	{
		private readonly ILogger<RabbitMQConnectionFactory> _logger;
		private readonly int _secondsDelayToReconnect;

		public RabbitMQConnectionFactory(ILogger<RabbitMQConnectionFactory> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_secondsDelayToReconnect = 10;
		}

		public IConnection CreateConnection(IConfiguration configuration)
		{
			var section = configuration.GetSection("MessageBroker");

			var hostname = section.GetValue<string>("Hostname");

			var connectionFactory = new ConnectionFactory
			{
				HostName = hostname,
				UserName = section.GetValue<string>("Username"),
				Password = section.GetValue<string>("Password"),
				VirtualHost = section.GetValue<string>("VirtualHost"),
				DispatchConsumersAsync = true,
				Port = section.GetValue<int>("Port"),
				Ssl =
				{
					ServerName = hostname,
					Enabled = true
				}
			};

			bool waitingForRabbit = true;

			IConnection connection = null;

			_logger.LogInformation("Trying to establish connection...");

			while(waitingForRabbit)
			{
				try
				{
					connection = connectionFactory.CreateConnection();
				}
				catch(BrokerUnreachableException ex)
				{
					if(ex.InnerException is AuthenticationFailureException authException)
					{
						_logger.LogInformation("RabbitMQ credentials is wrong...");
						throw authException;
					}
					_logger.LogInformation($"RabbitMQ instance \"{ hostname }\" is unreacheable...") ;
					Task.Delay(_secondsDelayToReconnect * 1000).Wait();
					continue;
				}

				waitingForRabbit = false;
			}

			if(connection == null)
			{
				throw new Exception("Connection not established");
			}

			return connection;
		}

		public IConnection CreateConnection(string hostname, string username, string password, string virtualhost, int port)
		{
			var connectionFactory = new ConnectionFactory
			{
				HostName = hostname,
				UserName = username,
				Password = password,
				VirtualHost = virtualhost,
				DispatchConsumersAsync = true,
				Port = port,
				Ssl =
				{
					ServerName = hostname,
					Enabled = true
				}
			};

			IConnection connection;

			_logger.LogInformation("Trying to establish connection...");

			connection = connectionFactory.CreateConnection();

			if(connection == null)
			{
				throw new Exception("Connection not established");
			}

			return connection;
		}
	}
}
