using NHibernate.Persister.Entity;
using NLog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;

namespace Vodovoz.Parameters
{
    public class ParametersProvider : IParametersProvider
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        Dictionary<string, string> parameters = new Dictionary<string, string>();

        public bool ContainsParameter(string parameterName)
        {
            RefreshParameter(parameterName);
            return parameters.ContainsKey(parameterName);
        }

        public string GetParameterValue(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                string freshParameterValue = uow.Session.QueryOver<BaseParameter>()
                    .Where(x => x.Name == parameterName)
                    .Select(x => x.StrValue)
                    .SingleOrDefault<string>();
                if (parameters.ContainsKey(parameterName))
                {
                    parameters[parameterName] = freshParameterValue;
                    return freshParameterValue;
                }
                if (!parameters.ContainsKey(parameterName) && !string.IsNullOrWhiteSpace(freshParameterValue))
                {
                    parameters.Add(parameterName, freshParameterValue);
                    return freshParameterValue;
                }
                throw new InvalidProgramException($"В параметрах базы не найден параметр ({parameterName})");
            }
        }

        private void RefreshParameter(string parameterName)
        {
            using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                BaseParameter parameter = uow.Session.QueryOver<BaseParameter>()
                    .Where(x => x.Name == parameterName)
                    .SingleOrDefault<BaseParameter>();
                if (parameter == null)
                {
                    return;
                }

                if (parameters.ContainsKey(parameterName))
                {
                    parameters[parameterName] = parameter.StrValue;
                    return;
                }
                if (!parameters.ContainsKey(parameterName))
                {
                    parameters.Add(parameter.Name, parameter.StrValue);
                    return;
                }
            }
        }

        public void RefreshParameters()
        {
            using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                var allParameters = uow.Session.QueryOver<BaseParameter>().List();
                parameters.Clear();
                foreach (var parameter in allParameters)
                {
                    if (parameters.ContainsKey(parameter.Name))
                    {
                        continue;
                    }
                    parameters.Add(parameter.Name, parameter.StrValue);
                }
            }
        }

        public int GetIntValue(string parameterId)
        {
            if (!ContainsParameter(parameterId))
            {
                throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})");
            }

            string value = GetParameterValue(parameterId);

            if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
            {
                throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
            }

            return result;
        }

        public char GetCharValue(string parameterId)
        {
            if (!ContainsParameter(parameterId))
            {
                throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})");
            }

            string value = GetParameterValue(parameterId);

            if (string.IsNullOrWhiteSpace(value) || !char.TryParse(value, out char result))
            {
                throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
            }

            return result;
        }

        public decimal GetDecimalValue(string parameterId)
        {
            if (!ContainsParameter(parameterId))
            {
                throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})");
            }

            string value = GetParameterValue(parameterId);

            if (string.IsNullOrWhiteSpace(value) || !decimal.TryParse(value, out decimal result))
            {
                throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
            }

            return result;
        }

        public string GetStringValue(string parameterId)
        {
            if (!ContainsParameter(parameterId))
            {
                throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})");
            }

            string value = GetParameterValue(parameterId);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
            }

            return value;
        }

        public void CreateOrUpdateParameter(string name, string value)
        {
            bool isInsert = false;
            if (parameters.TryGetValue(name, out string oldValue))
            {
                if (oldValue == value)
                {
                    return;
                }
            }
            else
            {
                isInsert = true;
            }
            using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                var bpPersister = (AbstractEntityPersister)uow.Session.SessionFactory.GetClassMetadata(typeof(BaseParameter));
                var tableName = bpPersister.TableName;
                var nameColumnName = bpPersister.GetPropertyColumnNames(nameof(BaseParameter.Name)).First();
                var strValueColumnName = bpPersister.GetPropertyColumnNames(nameof(BaseParameter.StrValue)).First();

                string sql;
                if (isInsert)
                {
                    sql = $"INSERT INTO {tableName} ({nameColumnName}, {strValueColumnName}) VALUES ('{name}', '{value}')";
                    logger.Debug($"Добавляем новый параметр базы {name}='{value}'");
                }
                else
                {
                    sql = $"UPDATE {tableName} SET {strValueColumnName} = '{value}' WHERE {nameColumnName} = '{name}'";
                    logger.Debug($"Изменяем параметр базы {name}='{value}'");
                }
                uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
            }

            logger.Debug("Ок");
        }
    }
}
