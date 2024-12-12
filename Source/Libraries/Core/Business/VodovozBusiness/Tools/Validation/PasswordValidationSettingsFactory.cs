using System;
using System.Linq;
using MySqlConnector;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Utilities.Debug;
using QS.Validation;

namespace Vodovoz.Tools.Validation
{
	public class PasswordValidationSettingsFactory
	{
		public PasswordValidationSettingsFactory(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public DefaultPasswordValidationSettings GetPasswordValidationSettings()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var databaseSoftwareType = GetDatabaseSoftwareType(uow);

				switch(databaseSoftwareType)
				{
					case DatabaseSoftwareType.MariaDB:
						return GetSimplePasswordCheckSettings(uow);
					case DatabaseSoftwareType.Percona:
						return GetValidatePasswordComponentSettings(uow);
					default:
						throw new NotSupportedException($"Not supported database software type {databaseSoftwareType}");
				}
			}
		}

		public DefaultPasswordValidationSettings GetSimplePasswordCheckSettings(IUnitOfWork unitOfWork)
		{
			try
			{
				return new DefaultPasswordValidationSettings
				{
					MinNumberCount = unitOfWork.Session
						.CreateSQLQuery("SELECT @@GLOBAL.simple_password_check_digits AS value")
						.AddScalar("value", NHibernateUtil.Int32)
						.List<int>().First(),
					MinLength = unitOfWork.Session
						.CreateSQLQuery("SELECT @@GLOBAL.simple_password_check_minimal_length AS value")
						.AddScalar("value", NHibernateUtil.Int32)
						.List<int>().First(),
					MinLetterSameCaseCount = unitOfWork.Session
						.CreateSQLQuery("SELECT @@GLOBAL.simple_password_check_letters_same_case AS value")
						.AddScalar("value", NHibernateUtil.Int32)
						.List<int>().First(),
					MinOtherCharactersCount = unitOfWork.Session
						.CreateSQLQuery("SELECT @@GLOBAL.simple_password_check_other_characters AS value")
						.AddScalar("value", NHibernateUtil.Int32)
						.List<int>().First(),
					AllowWhitespaces = false,
					ASCIIOnly = true
				};
			}
			catch(Exception ex)
			{
				var mysqlException = ex.FindExceptionTypeInInner<MySqlException>();
				if(mysqlException == null || mysqlException.Number != 1193)
				{
					throw;
				}
				throw new InvalidOperationException(
					"Неправильно настроены глобальные переменные." +
					" Возможно не установлен MariaDB плагин simple_password_check", ex);
			}
		}

		public DefaultPasswordValidationSettings GetValidatePasswordComponentSettings(IUnitOfWork unitOfWork)
		{
			try
			{
				return new DefaultPasswordValidationSettings
				{
					MinNumberCount = unitOfWork.Session
						.CreateSQLQuery("SELECT @@GLOBAL.validate_password.number_count AS value")
						.AddScalar("value", NHibernateUtil.Int32)
						.List<int>().First(),
					MinLength = unitOfWork.Session
						.CreateSQLQuery("SELECT @@GLOBAL.validate_password.length AS value")
						.AddScalar("value", NHibernateUtil.Int32)
						.List<int>().First(),
					MinLetterSameCaseCount = unitOfWork.Session
						.CreateSQLQuery("SELECT @@GLOBAL.validate_password.mixed_case_count AS value")
						.AddScalar("value", NHibernateUtil.Int32)
						.List<int>().First(),
					MinOtherCharactersCount = unitOfWork.Session
						.CreateSQLQuery("SELECT @@GLOBAL.validate_password.special_char_count AS value")
						.AddScalar("value", NHibernateUtil.Int32)
						.List<int>().First(),
					AllowWhitespaces = false,
					ASCIIOnly = true
				};
			}
			catch(Exception ex)
			{
				var mysqlException = ex.FindExceptionTypeInInner<MySqlException>();
				if(mysqlException == null || mysqlException.Number != 1193)
				{
					throw;
				}
				throw new InvalidOperationException(
					"Неправильно настроены глобальные переменные." +
					" Возможно не установлен mysql компонент validate_password", ex);
			}
		}

		public static DatabaseSoftwareType GetDatabaseSoftwareType(IUnitOfWork unitOfWork)
		{
			var version = unitOfWork.Session.CreateSQLQuery("SELECT @@version").List<string>().First();

			if(version.ToLower().Contains("mariadb"))
			{
				return DatabaseSoftwareType.MariaDB;
			}

			var versionComment = unitOfWork.Session.CreateSQLQuery("SELECT @@version_comment").List<string>().First();

			if(versionComment.ToLower().Contains("percona"))
			{
				return DatabaseSoftwareType.Percona;
			}

			throw new NotSupportedException("Not supported database software type");
		}
	}

	public enum DatabaseSoftwareType
	{
		MariaDB,
		Percona
	}
}
