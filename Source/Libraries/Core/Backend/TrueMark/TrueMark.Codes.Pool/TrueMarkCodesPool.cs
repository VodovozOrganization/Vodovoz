using MySqlConnector;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrueMark.Codes.Pool
{
	/// <summary>
	/// Предоставляет возможность добавлять и забирать коды из пула кодов 
	/// </summary>
	public class TrueMarkCodesPool
	{
		private const string _poolTableName = "true_mark_codes_pool";

		private readonly IUnitOfWork _uow;

		public TrueMarkCodesPool(IUnitOfWork uow)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_uow.OpenTransaction();
		}

		public virtual void PutCode(int codeId)
		{
			var query = GetPutCodeQuery(codeId);
			try
			{
				query.ExecuteUpdate();
			}
			catch(MySqlException ex) when(ex.Number == (int)MySqlErrorCode.DuplicateKeyEntry)
			{
				return;
			}
		}

		public virtual async Task PutCodeAsync(int codeId, CancellationToken cancellationToken)
		{
			var query = GetPutCodeQuery(codeId);
			try
			{
				await query.ExecuteUpdateAsync(cancellationToken);
			}
			catch(MySqlException ex) when(ex.Number == (int)MySqlErrorCode.DuplicateKeyEntry)
			{
				return;
			}
		}

		private IQuery GetPutCodeQuery(int codeId)
		{
			var sql = $@"INSERT INTO {_poolTableName} (code_id) VALUES(:code_id);";
			var query = _uow.Session.CreateSQLQuery(sql)
				.SetParameter("code_id", codeId);
			return query;
		}

		public virtual int TakeCode(string gtin)
		{
			var query = GetTakeCodeQuery(gtin);
			var codeId = query.UniqueResult<uint>();
			if(codeId == 0)
			{
				throw new EdoCodePoolException($"В пуле не найден код для gtin {gtin}");
			}
			return (int)codeId;
		}

		public virtual async Task<int> TakeCode(string gtin, CancellationToken cancellationToken)
		{
			var query = GetTakeCodeQuery(gtin);
			var codeId = await query.UniqueResultAsync<uint>(cancellationToken);
			if(codeId == 0)
			{
				throw new EdoCodePoolException($"В пуле не найден код для gtin {gtin}");
			}
			return (int)codeId;
		}

		private IQuery GetTakeCodeQuery(string gtin)
		{
			var sql = $@"
				SELECT pool.id
				FROM {_poolTableName} pool
					INNER JOIN true_mark_identification_code code ON code.id = pool.code_id 
				WHERE pool.promoted 
					AND code.gtin = :gtin
				ORDER BY pool.adding_time DESC 
				LIMIT 1
				FOR UPDATE SKIP LOCKED
				;";

			var query = _uow.Session.CreateSQLQuery(sql)
				.SetParameter("gtin", gtin);
			return query;
		}
	}
}
