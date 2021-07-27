using System;
using System.Linq;
using MySql.Data.MySqlClient;
using NHibernate;
using QS.DomainModel.UoW;
using QS.ErrorReporting;
using QS.Validation;

namespace Vodovoz.Tools.Validation
{
    public class PasswordValidationSettingsFactory
    {
        public PasswordValidationSettingsFactory(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public DefaultPasswordValidationSettings GetPasswordValidationSettings()
        {
            using(var uow = unitOfWorkFactory.CreateWithoutRoot()) {
                try {
                    return new DefaultPasswordValidationSettings {
                        MinNumberCount = uow.Session.CreateSQLQuery("SELECT @@GLOBAL.simple_password_check_digits AS value")
                            .AddScalar("value", NHibernateUtil.Int32)
                            .List<int>().First(),
                        MinLength = uow.Session.CreateSQLQuery("SELECT @@GLOBAL.simple_password_check_minimal_length AS value")
                            .AddScalar("value", NHibernateUtil.Int32)
                            .List<int>().First(),
                        MinLetterSameCaseCount = uow.Session
                            .CreateSQLQuery("SELECT @@GLOBAL.simple_password_check_letters_same_case AS value")
                            .AddScalar("value", NHibernateUtil.Int32)
                            .List<int>().First(),
                        MinOtherCharactersCount = uow.Session
                            .CreateSQLQuery("SELECT @@GLOBAL.simple_password_check_other_characters AS value")
                            .AddScalar("value", NHibernateUtil.Int32)
                            .List<int>().First(),
                        AllowWhitespaces = false,
                        ASCIIOnly = true
                    };
                }
                catch(Exception ex) {
                    var mysqlException = ex.FindExceptionTypeInInner<MySqlException>();
                    if(mysqlException == null || mysqlException.Number != 1193) {
                        throw;
                    }
                    throw new InvalidOperationException(
                        "Неправильно настроены глобальные переменные. Возможно не установлен mysql плагин simple_password_check", ex);
                }
            }
        }
    }
}
