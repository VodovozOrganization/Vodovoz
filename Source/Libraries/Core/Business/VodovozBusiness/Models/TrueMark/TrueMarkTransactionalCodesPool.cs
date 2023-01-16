using QS.DomainModel.UoW;
using System.Collections.Generic;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkTransactionalCodesPool : TrueMarkCodesPool
	{
		private IList<string> _codesToPut;
		private IList<string> _codesToTake;
		private IList<string> _defectiveCodesToPut;
		private IList<string> _defectiveCodesToTake;

		public TrueMarkTransactionalCodesPool(IUnitOfWorkFactory uowFactory) : base(uowFactory)
		{
			RefreshCache();
		}

		private void RefreshCache()
		{
			_codesToPut = new List<string>();
			_codesToTake = new List<string>();
			_defectiveCodesToPut = new List<string>();
			_defectiveCodesToTake = new List<string>();
		}

		public override void PutCode(string code)
		{
			_codesToPut.Add(code);
		}

		public override string TakeCode()
		{
			var code = base.TakeCode();
			_codesToTake.Add(code);
			return code;
		}

		public override void PutDefectiveCode(string code)
		{
			_defectiveCodesToPut.Add(code);
		}

		public override string TakeDefectiveCode()
		{
			var code = base.TakeDefectiveCode();
			_defectiveCodesToTake.Add(code);
			return code;
		}

		public void Commit()
		{
			try
			{
				foreach(var codeToPut in _codesToPut)
				{
					PutCode(codeToPut);
				}

				foreach(var defectiveCodeToPut in _defectiveCodesToPut)
				{
					PutCode(defectiveCodeToPut);
				}
			}
			finally
			{
				RefreshCache();
			}
		}

		public void Rollback()
		{
			try
			{
				foreach(var codeToTake in _codesToTake)
				{
					PutCode(codeToTake);
				}

				foreach(var defectiveCodeToTake in _defectiveCodesToTake)
				{
					PutCode(defectiveCodeToTake);
				}
			}
			finally
			{
				RefreshCache();
			}
		}
	}
}
