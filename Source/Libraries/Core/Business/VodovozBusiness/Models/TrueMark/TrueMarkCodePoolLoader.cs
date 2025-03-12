using ClosedXML.Excel;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Settings.Edo;
using NewPool = TrueMark.Codes.Pool.TrueMarkCodesPool;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCodePoolLoader
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly TrueMarkWaterCodeParser _trueMarkCodeParser;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly NewPool _newTrueMarkCodesPool;
		private readonly IInteractiveMessage _interactiveMessage;
		private readonly IEdoSettings _edoSettings;
		private readonly bool _usingNewPool;

		public TrueMarkCodePoolLoader(
			IUnitOfWorkFactory uowFactory, 
			TrueMarkWaterCodeParser trueMarkCodeParser,
			TrueMarkCodesPool trueMarkCodesPool,
			NewPool newTrueMarkCodesPool,
			IInteractiveMessage interactiveMessage,
			IEdoSettings edoSettings
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkCodeParser = trueMarkCodeParser ?? throw new ArgumentNullException(nameof(trueMarkCodeParser));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_newTrueMarkCodesPool = newTrueMarkCodesPool ?? throw new ArgumentNullException(nameof(newTrueMarkCodesPool));
			_interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_usingNewPool = _edoSettings.CodePoolLoaderToNewPool;
		}

		public CodeLoadingResult LoadFromFile(string path)
		{
			if(_usingNewPool)
			{
				_interactiveMessage.ShowMessage(ImportanceLevel.Info, 
					"Используется загрузка в НОВЫЙ пул кодов, рассчитанный на работу " +
					"с поэкземплярным учетом кодов честного знака");
			}
			else
			{
				_interactiveMessage.ShowMessage(ImportanceLevel.Info,
					"Используется загрузка в СТАРЫЙ пул кодов, рассчитанный на работу " +
					"с объемно-сортовым учетом кодов честного знака");
			}

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

			var saveTasks = new List<Task>();
			int successfulLoaded = 0;
			int totalFound = 0;

			foreach(var cell in cells)
			{
				string content = "";
				try
				{
					content = cell.GetValue<string>();
				}
				catch(Exception)
				{
					continue;
				}

				saveTasks.Add(
					Task.Run(() => {
						var code = TryParseCode(content);
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

		private CodeLoadingResult LoadFromText(string excelFilePath)
		{
			var lines = File.ReadLines(excelFilePath);

			var saveTasks = new List<Task>();
			int successfulLoaded = 0;
			int totalFound = 0;

			foreach(var line in lines)
			{
				saveTasks.Add(
					Task.Run(() => {
						var code = TryParseCode(line);
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
				GTIN = code.GTIN,
				SerialNumber = code.SerialNumber,
				CheckCode = code.CheckCode
			};

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				try
				{
					uow.Save(codeEntity);
					uow.Commit();
				}
				catch(Exception)
				{
					return false;
				}
			}

			if(_usingNewPool)
			{
				_newTrueMarkCodesPool.PutCode(codeEntity.Id);
			}
			else
			{
				_trueMarkCodesPool.PutCode(codeEntity.Id);
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
