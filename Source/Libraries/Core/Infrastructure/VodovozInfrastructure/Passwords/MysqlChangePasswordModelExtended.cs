using MySqlConnector;
using QS.Project.DB;
using QS.Project.DB.Passwords;
using QS.Project.Repositories;
using QS.Utilities.Text;
using System;
using System.Security;
using VodovozInfrastructure.Configuration;

namespace VodovozInfrastructure.Passwords
{
	public class MysqlChangePasswordModelExtended : MySqlChangePasswordModel
    {
        public MysqlChangePasswordModelExtended(
            IApplicationConfigurator applicationConfigurator,
            MySqlConnection connection,
            IMySqlPasswordRepository mySqlPasswordRepository)
            : base(connection, mySqlPasswordRepository)
        {
            this.applicationConfigurator = applicationConfigurator ?? throw new ArgumentNullException(nameof(applicationConfigurator));
        }

        private readonly IApplicationConfigurator applicationConfigurator;

        public override void ChangePassword(SecureString newPassword)
        {
            base.ChangePassword(newPassword);

            var dbConnectionStringBuilder = new MySqlConnectionStringBuilder {
                ConnectionString = Connection.ConnectionString,
                Password = NewPassword.ToPlainString()
            };

            Connection.ChangeDbConnectionString(dbConnectionStringBuilder.ConnectionString);
            //applicationConfigurator.ConfigureOrm();
        }
    }
}
