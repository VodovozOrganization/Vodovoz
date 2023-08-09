using Dapper;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Vodovoz.CachingRepositories.Common;

namespace Vodovoz.CachingRepositories.Counterparty
{
	public class CounterpartyInMemoryTitlesCacheRepository : DomainEntityNodeInMemoryCacheRepositoryBase<Domain.Client.Counterparty>
	{
		public CounterpartyInMemoryTitlesCacheRepository(
			ILogger<IDomainEntityNodeInMemoryCacheRepository<Domain.Client.Counterparty>> logger,
			IUnitOfWorkFactory unitOfWorkFactory) : base(logger, unitOfWorkFactory)
		{
		}

		protected override Domain.Client.Counterparty GetEntityById(int id)
		{
			var tableName = nameof(Domain.Client.Counterparty).ToLower();

			var sqlQuery = $"select {tableName}.id, {tableName}.name from {tableName} where {tableName}.id = @id";

			var connection = _unitOfWork.Session.Connection;

			var result = connection.Query<Domain.Client.Counterparty>(
				sqlQuery,
				new { id })
				.FirstOrDefault();

			return result;
		}

		protected override IDictionary<int, string> GetTitlesByIdsFromDatabase(ICollection<int> ids)
		{
			var tableName = nameof(Domain.Client.Counterparty).ToLower();

			var connection = _unitOfWork.Session.Connection;

			DbCommand dbCommand = connection.CreateCommand();

			var idsString = "(" + string.Join(",", ids) + ")";

			dbCommand.CommandText = $"select {tableName}.id, {tableName}.name " +
						   $"from {tableName} " +
						   $"where {tableName}.id in {idsString}";

			var result = new Dictionary<int, string>();

			DbDataReader reader = dbCommand.ExecuteReader();

			while(reader.Read())
			{
				result.Add(int.Parse(reader[0].ToString()), reader[1].ToString());
			}

			reader.Close();

			return result;
		}
	}
}
