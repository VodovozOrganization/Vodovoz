﻿using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Vodovoz.Models.TrueMark
{

	// После обновления базы данных до версии 10.6 
	// необходимо поменять работу пула кодов на логику 
	// использующую SELECT SKIP LOCKED
	public class TrueMarkCodesPool
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public TrueMarkCodesPool(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public virtual void PutCode(int codeId)
		{
			if(ContainsCode(codeId))
			{
				return;
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				var sql = $@"INSERT INTO true_mark_codes_pool (code_id) VALUES(:code_id);";
				uow.Session.CreateSQLQuery(sql)
					.SetParameter("code_id", codeId)
					.ExecuteUpdate();
				transaction.Commit();
			}
		}

		public virtual int TakeCode()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				var sql = $@"
					DELETE FROM true_mark_codes_pool
					WHERE promoted
					ORDER BY adding_time DESC 
					LIMIT 1
					RETURNING code_id
					;";
				var query = uow.Session.CreateSQLQuery(sql);
				var result = (int)query.UniqueResult<uint>();
				transaction.Commit();
				return result;
			}
		}

		public virtual int TakeCode(string gtin)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				var deletingCodeIdQuery = @"
						SELECT 
							pool.id
						FROM 
							true_mark_codes_pool pool
							INNER JOIN true_mark_identification_code code ON code.id = pool.code_id 
						WHERE 
							pool.promoted 
							AND code.gtin = :gtin
						ORDER BY pool.adding_time DESC 
						LIMIT 1
					;";
				
				var query = @"DELETE FROM true_mark_codes_pool
					WHERE id = :deletingCodeId
					RETURNING code_id
					;";

				var deletingCodeId = uow.Session.CreateSQLQuery(deletingCodeIdQuery)
					.SetParameter("gtin", gtin)
					.UniqueResult<uint>();

				if(deletingCodeId == 0)
				{
					return 0;
				}
				
				var result = (int)uow.Session.CreateSQLQuery(query)
					.SetParameter("deletingCodeId", deletingCodeId)
					.UniqueResult<uint>();
				
				transaction.Commit();
				return result;
			}
		}

		public virtual void PutDefectiveCode(int codeId)
		{
			if(ContainsDefectiveCode(codeId))
			{
				return;
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				var sql = $@"INSERT INTO true_mark_defective_codes_pool (code_id) VALUES(:code_id);";
				uow.Session.CreateSQLQuery(sql)
					.SetParameter("code_id", codeId)
					.ExecuteUpdate();
				transaction.Commit();
			}
		}

		public virtual int TakeDefectiveCode()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				var sql = $@"
					DELETE FROM true_mark_defective_codes_pool
					ORDER BY adding_time DESC 
					LIMIT 1
					RETURNING code_id
					;";
				var query = uow.Session.CreateSQLQuery(sql);
				var result = (int)query.UniqueResult<uint>();
				transaction.Commit();
				return result;
			}
		}

		public bool ContainsCode(int codeId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var sql = $@"SELECT code_id FROM true_mark_codes_pool WHERE code_id = :code_id;";
				var query = uow.Session.CreateSQLQuery(sql)
					.SetParameter("code_id", codeId);
				var result = (int)query.UniqueResult<uint>();
				return result != 0;
			}
		}

		public bool ContainsDefectiveCode(int codeId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var sql = $@"SELECT code_id FROM true_mark_defective_codes_pool WHERE code_id = :code_id;";
				var query = uow.Session.CreateSQLQuery(sql)
					.SetParameter("code_id", codeId);
				var result = (int)query.UniqueResult<uint>();
				return result != 0;
			}
		}

		public IEnumerable<int> SelectCodes(int count, bool promoted)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var sql = $@"
					SELECT code_id FROM true_mark_codes_pool
					WHERE promoted = :promoted 
					ORDER BY adding_time DESC
					LIMIT :count
					;";
				var query = uow.Session.CreateSQLQuery(sql)
					.SetParameter("count", count)
					.SetParameter("promoted", promoted);
				var result = query.List<uint>().Select(x => (int)x);
				return result;
			}
		}

		public void PromoteCodes(IEnumerable<int> codeIds, int extraSecond)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				var sql = $@"
					UPDATE true_mark_codes_pool SET
						adding_time = ADDDATE(current_timestamp(), INTERVAL :extra_second SECOND),
						promoted = 1
					WHERE code_id in (:code_ids)
					;";
				var query = uow.Session.CreateSQLQuery(sql)
					.SetParameterList("code_ids", codeIds.ToArray())
					.SetParameter("extra_second", extraSecond);
				query.ExecuteUpdate();
				transaction.Commit();
			}
		}

		public void DeleteCodes(IEnumerable<int> codeIds)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				var sql = $@"
					DELETE FROM true_mark_codes_pool
					WHERE code_id in (:code_ids)
					;";
				var query = uow.Session.CreateSQLQuery(sql)
					.SetParameterList("code_ids", codeIds.ToArray());
				query.ExecuteUpdate();
				transaction.Commit();
			}
		}

		public int GetTotalCount()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var sql = $@"
					SELECT Count(id) FROM true_mark_codes_pool;";
				var query = uow.Session.CreateSQLQuery(sql);
				var result = (int)query.UniqueResult<long>();
				return result;
			}
		}

		public int GetDefectiveTotalCount()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var sql = $@"
					SELECT Count(id) FROM true_mark_defective_codes_pool;";
				var query = uow.Session.CreateSQLQuery(sql);
				var result = (int)query.UniqueResult<long>();
				return result;
			}
		}
	}
}
