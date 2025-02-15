﻿using Dapper;
using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabaseServiceWorker.Helpers
{
	internal static class DapperExtensions
	{
		internal static async Task<List<T>> GetDataAsync<T>(this MySqlConnection connection, string sql, object param = null)
		{
			var result = new List<T>();

			using(var multi = await connection.QueryMultipleAsync(sql, param, commandTimeout: 120))
			{
				while(!multi.IsConsumed)
				{
					var rows = await multi.ReadAsync<T>();
					result.AddRange(rows);
				}
			}

			return result;
		}
	}
}
