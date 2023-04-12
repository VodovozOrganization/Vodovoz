using QS.DomainModel.UoW;
using System.Collections.Generic;

namespace Vodovoz.Models.TrueMark
{
	// После обновления базы данных до версии 10.6 
	// необходимо поменять работу пула кодов на логику 
	// использующую SELECT SKIP LOCKED
	public class TrueMarkTransactionalCodesPool : TrueMarkCodesPool
	{
		private IList<int> _codesToPut = new List<int>();
		private IList<int> _codesToTake = new List<int>();
		private IList<int> _defectiveCodesToPut = new List<int>();
		private IList<int> _defectiveCodesToTake = new List<int>();

		public TrueMarkTransactionalCodesPool(IUnitOfWorkFactory uowFactory) : base(uowFactory)
		{
			RefreshCache();
		}

		private void RefreshCache()
		{
			_codesToPut.Clear();
			_codesToTake.Clear();
			_defectiveCodesToPut.Clear();
			_defectiveCodesToTake.Clear();
		}

		public override void PutCode(int codeId)
		{
			_codesToPut.Add(codeId);
		}

		public override int TakeCode()
		{
			var codeId = base.TakeCode();
			if(codeId == 0)
			{
				throw new TrueMarkException("В пуле отсутствуют коды для получения.");
			}
			_codesToTake.Add(codeId);
			return codeId;
		}

		public override int TakeCode(string gtin)
		{
			var codeId = base.TakeCode(gtin);
			if(codeId == 0)
			{
				throw new TrueMarkException($"В пуле отсутствуют коды по gtin {gtin} для получения.");
			}
			_codesToTake.Add(codeId);
			return codeId;
		}

		public override void PutDefectiveCode(int codeId)
		{
			_defectiveCodesToPut.Add(codeId);
		}

		public override int TakeDefectiveCode()
		{
			var codeId = base.TakeDefectiveCode();
			if(codeId == 0)
			{
				throw new TrueMarkException("В пуле отсутствуют коды для получения.");
			}
			_defectiveCodesToTake.Add(codeId);
			return codeId;
		}

		public void Commit()
		{
			try
			{
				foreach(var codeToPut in _codesToPut)
				{
					base.PutCode(codeToPut);
				}

				foreach(var defectiveCodeToPut in _defectiveCodesToPut)
				{
					base.PutDefectiveCode(defectiveCodeToPut);
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
					base.PutCode(codeToTake);
				}

				foreach(var defectiveCodeToTake in _defectiveCodesToTake)
				{
					base.PutDefectiveCode(defectiveCodeToTake);
				}
			}
			finally
			{
				RefreshCache();
			}
		}
	}
}
