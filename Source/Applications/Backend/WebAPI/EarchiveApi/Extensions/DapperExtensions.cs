using Dapper;
using Polly.Retry;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EarchiveApi.Extensions
{
	public static class DapperExtensions
	{
		public static async Task<IEnumerable<T>> QueryAsyncWithRetry<T>(
			this IDbConnection connection,
			string query,
			AsyncRetryPolicy policy,
			object param = null) =>
			await policy.ExecuteAsync(async () => await connection.QueryAsync<T>(query, param));
	}
}
