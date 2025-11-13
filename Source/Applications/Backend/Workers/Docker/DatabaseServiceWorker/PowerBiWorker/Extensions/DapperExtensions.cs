using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;

namespace DatabaseServiceWorker.PowerBiWorker.Extensions
{
	internal static class DapperExtensions
	{
		internal static async Task<List<T>> GetDataAsync<T>(this MySqlConnection connection, string sql, object param = null, IDbTransaction transaction = null)
		{
			var result = new List<T>();

			using var multi = await connection.QueryMultipleAsync(sql, param, transaction: transaction, commandTimeout: 120);
			
			while(!multi.IsConsumed)
			{
				var rows = await multi.ReadAsync<T>();
				result.AddRange(rows);
			}

			return result;
		}
	}
}
