using Dapper;
using Polly;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace VodovozMangoService.Extensions
{
	public static class DapperExtensions
	{
		public static async Task<IEnumerable<T>> QueryAsyncWithPolicy<T>(
			this IDbConnection connection,
			string query,
			IAsyncPolicy policy,
			object param = null) =>
			await policy.ExecuteAsync(async () => await connection.QueryAsync<T>(query, param));
	}
}
