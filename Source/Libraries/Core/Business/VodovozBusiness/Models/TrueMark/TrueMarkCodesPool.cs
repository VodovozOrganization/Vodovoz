using QS.DomainModel.UoW;
using System;
using System.Data;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCodesPool
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public TrueMarkCodesPool(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public void PutCode(string code)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				var sql = $@"INSERT INTO true_mark_codes_pool (code) VALUES(':code');";
				uow.Session.CreateSQLQuery(sql)
					.SetParameter("code", code)
					.ExecuteUpdate();
				uow.Commit();
			}
		}

		public string TakeCode()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				var sql = $@"
					DELETE FROM true_mark_codes_pool
					ORDER BY adding_time DESC 
					LIMIT 1
					RETURNING code
					;";
				var query = uow.Session.CreateSQLQuery(sql);
				var result = query.UniqueResult<string>();
				uow.Commit();
				return result;
			}
		}
	}
}
