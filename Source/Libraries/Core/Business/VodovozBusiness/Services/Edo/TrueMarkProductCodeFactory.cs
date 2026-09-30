using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace VodovozBusiness.Services.Edo
{
	public static class TrueMarkProductCodeFactory
	{
		/// <summary>
		/// Создает новые коды маркировки типа Auto на основе существующих кодов
		/// </summary>
		/// <param name="sourceCodes">Исходные коды маркировки</param>
		/// <returns>Список новых кодов маркировки типа Auto</returns>
		public static List<TrueMarkProductCode> CreateAutoCodesFromSource(
			IEnumerable<TrueMarkProductCode> sourceCodes)
		{
			if(sourceCodes is null)
			{
				throw new ArgumentNullException(nameof(sourceCodes));
			}

			var newCodes = new List<TrueMarkProductCode>();

			foreach(var sourceCode in sourceCodes)
			{
				var autoCode = new AutoTrueMarkProductCode
				{
					CreationTime = DateTime.Now,
					LastModified = DateTime.Now,
					SourceCode = sourceCode.SourceCode,
					SourceCodeStatus = SourceProductCodeStatus.New,
					Problem = ProductCodeProblem.None
				};

				newCodes.Add(autoCode);
			}

			return newCodes;
		}

		/// <summary>
		/// Создает коды маркировки типа Auto на основе кодов из отменённой задачи
		/// </summary>
		/// <param name="cancelledTask">Отменённая задача ЭДО</param>
		/// <returns>Список новых кодов маркировки типа Auto</returns>
		public static List<TrueMarkProductCode> CreateAutoCodesFromCancelledTask(
			OrderEdoTask cancelledTask)
		{
			if(cancelledTask is null)
			{
				throw new ArgumentNullException(nameof(cancelledTask));
			}

			return CreateAutoCodesFromSource(cancelledTask.Items.Select(x => x.ProductCode));
		}
	}
}
