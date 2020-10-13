using System;
using MySql.Data.MySqlClient;
using Nini.Config;

namespace VodovozMangoService
{
    public class VodovozMangoConfiguration
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly string configFile = "/etc/vodovoz-mango-service.conf";
	    
        //MangoService
        public readonly int NotficationServicePort;
        public readonly int MangoServiceHttpPort; 
        public readonly int MangoServiceHttpsPort; 
	    
        //Mysql
        private static string mysqlServerHostName;
        private static string mysqlServerPort;
        private static string mysqlUser;
        private static string mysqlPassword;
        private static string mysqlDatabase;

        public MySqlConnectionStringBuilder ConnectionStringBuilder;
	    
        //Mango
        public readonly string VpbxApiKey;
        public readonly string VpbxApiSalt;

        public VodovozMangoConfiguration()
        {
            #region Читаем конфигурацию
            try {
                IniConfigSource confFile = new IniConfigSource(configFile);
                confFile.Reload();

                IConfig serviceConfig = confFile.Configs["MangoService"];
                NotficationServicePort = serviceConfig.GetInt("grps_client_port");
                MangoServiceHttpPort = serviceConfig.GetInt("http_port");
                MangoServiceHttpsPort = serviceConfig.GetInt("https_port");
		        
                IConfig mysqlConfig = confFile.Configs["Mysql"];
                mysqlServerHostName = mysqlConfig.GetString("mysql_server_host_name");
                mysqlServerPort = mysqlConfig.GetString("mysql_server_port", "3306");
                mysqlUser = mysqlConfig.GetString("mysql_user");
                mysqlPassword = mysqlConfig.GetString("mysql_password");
                mysqlDatabase = mysqlConfig.GetString("mysql_database");
				
                IConfig mangoConfig = confFile.Configs["Mango"];
                VpbxApiKey = mangoConfig.GetString("vpbx_api_key");
                VpbxApiSalt = mangoConfig.GetString("vpbx_api_salt");
            }
            catch(Exception ex) {
                logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
                return;
            }
            #endregion

            logger.Info("Настраиваем соединение с базой данных.");
            try {
                ConnectionStringBuilder = new MySqlConnectionStringBuilder();
                ConnectionStringBuilder.Server = mysqlServerHostName;
                ConnectionStringBuilder.Port = UInt32.Parse(mysqlServerPort);
                ConnectionStringBuilder.Database = mysqlDatabase;
                ConnectionStringBuilder.UserID = mysqlUser;
                ConnectionStringBuilder.Password = mysqlPassword;
                ConnectionStringBuilder.SslMode = MySqlSslMode.None;
            }
            catch(Exception ex) {
                logger.Fatal(ex, "Ошибка в настройке подключения к БД.");
            }
        }
    }
}