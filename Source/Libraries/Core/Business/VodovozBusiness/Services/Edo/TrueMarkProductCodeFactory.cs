using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace VodovozBusiness.Services.Edo
{
	/// <summary>
	/// Создает новые записи кодов маркировки на основе существующих.
	/// </summary>
	public static class TrueMarkProductCodeFactory
	{
		/// <summary>
		/// Создает новые записи на основе исходных кодов. Отклоненные исходные коды получают тип Resent.
		/// </summary>
		/// <param name="sourceCodes">Исходные коды маркировки</param>
		/// <returns>Список новых записей кодов маркировки</returns>
		public static List<TrueMarkProductCode> CreateCodesFromSource(
			IEnumerable<TrueMarkProductCode> sourceCodes)
		{
			if(sourceCodes is null)
			{
				throw new ArgumentNullException(nameof(sourceCodes));
			}

			var newCodes = new List<TrueMarkProductCode>();

			foreach(var sourceCode in sourceCodes)
			{
				if(sourceCode.SourceCodeStatus == SourceProductCodeStatus.Rejected
					&& sourceCode.SourceCode != null)
				{
					newCodes.Add(CreateCodeFromSource<ResentTrueMarkProductCode>(sourceCode));
				}
				else
				{
					newCodes.Add(CreateCodeFromSource<AutoTrueMarkProductCode>(sourceCode));
				}
			}

			return newCodes;
		}

		/// <summary>
		/// Создает новые записи кодов на основе кодов отмененной задачи.
		/// </summary>
		/// <param name="cancelledTask">Отменённая задача ЭДО</param>
		/// <returns>Список новых записей кодов маркировки</returns>
		public static List<TrueMarkProductCode> CreateCodesFromCancelledTask(
			OrderEdoTask cancelledTask)
		{
			if(cancelledTask is null)
			{
				throw new ArgumentNullException(nameof(cancelledTask));
			}

			return CreateCodesFromSource(cancelledTask.Items.Select(x => x.ProductCode));
		}

		private static TTrueMarkProductCode CreateCodeFromSource<TTrueMarkProductCode>(TrueMarkProductCode sourceCode)
			where TTrueMarkProductCode : TrueMarkProductCode, new()
		{
			return new TTrueMarkProductCode
			{
				CreationTime = DateTime.Now,
				LastModified = DateTime.Now,
				SourceCode = sourceCode.SourceCode,
				SourceCodeStatus = SourceProductCodeStatus.New,
				Problem = ProductCodeProblem.None
			};
		}
	}
}
