using Core.Infrastructure;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{

	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров",
			Nominative = "код ЧЗ товара")]
	[HistoryTrace]
	public abstract class TrueMarkProductCode : PropertyChangedBase, IDomainObject
	{
		private DateTime _creationTime;
		private SourceProductCodeStatus _sourceCodeStatus;
		private TrueMarkWaterIdentificationCode _sourceCode;
		private TrueMarkWaterIdentificationCode _resultCode;
		private ProductCodeProblem _problem;
		private int _duplicatesCount;


		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}

		[Display(Name = "Стадия")]
		public virtual SourceProductCodeStatus SourceCodeStatus
		{
			get => _sourceCodeStatus;
			set => SetField(ref _sourceCodeStatus, value);
		}

		public virtual int Id { get; set; }

		[Display(Name = "Исходный код")]
		public virtual TrueMarkWaterIdentificationCode SourceCode
		{
			get => _sourceCode;
			set => SetField(ref _sourceCode, value);
		}

		[Display(Name = "Результирующий код")]
		public virtual TrueMarkWaterIdentificationCode ResultCode
		{
			get => _resultCode;
			set => SetField(ref _resultCode, value);
		}

		[Display(Name = "Проблема")]
		public virtual ProductCodeProblem Problem
		{
			get => _problem;
			set => SetField(ref _problem, value);
		}

		[Display(Name = "Кол-во дубликатов")]
		public virtual int DuplicatesCount
		{
			get => _duplicatesCount;
			set => SetField(ref _duplicatesCount, value);
		}
	}
}
