using System;
using System.Security;
using MySql.Data.MySqlClient;
using QS.Project.DB;
using QS.Project.DB.Passwords;
using QS.Project.Repositories;
using QS.Utilities.Text;
using VodovozInfrastructure.Database;

namespace VodovozInfrastructure.Passwords
{
    public class MysqlChangePasswordModelExtended : MySqlChangePasswordModel
    {
        public MysqlChangePasswordModelExtended(
            IDatabaseConfigurator databaseConfigurator,
            MySqlConnection connection,
            IMySqlPasswordRepository mySqlPasswordRepository)
            : base(connection, mySqlPasswordRepository)
        {
            this.databaseConfigurator = databaseConfigurator ?? throw new ArgumentNullException(nameof(databaseConfigurator));
        }

        private readonly IDatabaseConfigurator databaseConfigurator;

        public override void ChangePassword(SecureString newPassword)
        {
            base.ChangePassword(newPassword);

            var dbConnectionStringBuilder = new MySqlConnectionStringBuilder {
                ConnectionString = Connection.ConnectionString,
                Password = NewPassword.ToPlainString()
            };

            Connection.ChangeDbConnectionString(dbConnectionStringBuilder.ConnectionString);
            databaseConfigurator.ConfigureOrm();
        }
    }
}
