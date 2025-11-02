using ClosedXML.Excel;
using MoreLinq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCodePoolLoader
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly TrueMarkWaterCodeParser _trueMarkCodeParser;
		private readonly TrueMarkCodesPoolFactory _trueMarkCodesPoolFactory;

		public TrueMarkCodePoolLoader(
			IUnitOfWorkFactory uowFactory,
			TrueMarkWaterCodeParser trueMarkCodeParser,
			TrueMarkCodesPoolFactory trueMarkCodesPoolFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkCodeParser = trueMarkCodeParser ?? throw new ArgumentNullException(nameof(trueMarkCodeParser));
			_trueMarkCodesPoolFactory = trueMarkCodesPoolFactory ?? throw new ArgumentNullException(nameof(trueMarkCodesPoolFactory));
		}

		public CodeLoadingResult LoadFromFile(string path)
		{
			var extension = Path.GetExtension(path);
			switch(extension)
			{
				case ".xlsx":
					return LoadFromExcel(path);
				default:
					return LoadFromText(path);
			}
		}

		private CodeLoadingResult LoadFromExcel(string excelFilePath)
		{
			var workbook = new XLWorkbook(excelFilePath);
			var workSheet = workbook.Worksheets.FirstOrDefault();
			if(workSheet == null)
			{
				return new CodeLoadingResult();
			}

			var cells = workSheet.FirstColumn().Cells();

			var cellsValues = new List<string>();

			foreach(var cell in cells)
			{
				try
				{
					cellsValues.Add(cell.GetValue<string>());
				}
				catch(Exception)
				{
					continue;
				}
			}

			return SaveCodes(cellsValues.Where(x => !string.IsNullOrWhiteSpace(x)));
		}

		private CodeLoadingResult LoadFromText(string excelFilePath)
		{
			var lines = File.ReadLines(excelFilePath);

			return SaveCodes(lines.Where(x => !string.IsNullOrWhiteSpace(x)));
		}

		private CodeLoadingResult SaveCodes(IEnumerable<string> codeStrings)
		{
			var saveTasks = new List<Task>();
			int successfulLoaded = 0;
			int totalFound = 0;

			foreach(var codeString in codeStrings)
			{
				saveTasks.Add(
						Task.Run(() =>
						{
							var code = TryParseCode(codeString);
							if(code == null)
							{
								return;
							}

							Interlocked.Increment(ref totalFound);

							if(TrySaveCode(code))
							{
								Interlocked.Increment(ref successfulLoaded);
							}
						})
					);

				if(saveTasks.Count == 5)
				{
					Task.WaitAll(saveTasks.ToArray());
					saveTasks.Clear();
				}
			}

			Task.WaitAll(saveTasks.ToArray());

			return new CodeLoadingResult { SuccessfulLoaded = successfulLoaded, TotalFound = totalFound };
		}

		private TrueMarkWaterCode TryParseCode(string content)
		{
			try
			{
				return _trueMarkCodeParser.ParseCodeFrom1c(content);
			}
			catch(Exception)
			{
				return null;
			}
		}

		private bool TrySaveCode(TrueMarkWaterCode code)
		{
			var codeEntity = new TrueMarkWaterIdentificationCode
			{
				IsInvalid = false,
				RawCode = code.SourceCode.Substring(0, Math.Min(255, code.SourceCode.Length)),
				Gtin = code.Gtin,
				SerialNumber = code.SerialNumber,
				CheckCode = code.CheckCode
			};
			
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				try
				{
					var savedCode = uow.Session.QueryOver<TrueMarkWaterIdentificationCode>()
						.Where(x => x.RawCode == codeEntity.RawCode)
						.Take(1)
						.SingleOrDefault<TrueMarkWaterIdentificationCode>();

					if(savedCode != null)
					{
						return false;
					}

					uow.Save(codeEntity);

					var pool = _trueMarkCodesPoolFactory.Create(uow);
					pool.PutCode(codeEntity.Id);

					uow.Commit();
				}
				catch(Exception ex)
				{
					return false;
				}
			}

			return true;
		}

		public struct CodeLoadingResult
		{
			public int TotalFound { get; set; }
			public int SuccessfulLoaded { get; set; }
		}
	}
}
