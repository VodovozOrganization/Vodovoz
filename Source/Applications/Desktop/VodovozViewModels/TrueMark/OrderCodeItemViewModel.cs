using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.ViewModels.TrueMark
{
	public class OrderCodeItemViewModel : QS.ViewModels.ViewModelBase
	{
		private string _sourceIdentificationCode;
		private string _resultIdentificationCode;
		private bool _replacedFromPool;
		private ProductCodeProblem _problem;
		private int? _sourceDocumentId;
		private int? _codeAuthorId;
		private string _codeAuthor;

		public virtual string SourceIdentificationCode
		{
			get => _sourceIdentificationCode;
			set => SetField(ref _sourceIdentificationCode, value);
		}

		public virtual string ResultIdentificationCode
		{
			get => _resultIdentificationCode;
			set => SetField(ref _resultIdentificationCode, value);
		}

		public virtual bool ReplacedFromPool
		{
			get => _replacedFromPool;
			set => SetField(ref _replacedFromPool, value);
		}

		public virtual ProductCodeProblem Problem
		{
			get => _problem;
			set => SetField(ref _problem, value);
		}

		public virtual int? SourceDocumentId
		{
			get => _sourceDocumentId;
			set => SetField(ref _sourceDocumentId, value);
		}

		public virtual int? CodeAuthorId
		{
			get => _codeAuthorId;
			set => SetField(ref _codeAuthorId, value);
		}

		public virtual string CodeAuthor
		{
			get => _codeAuthor;
			set => SetField(ref _codeAuthor, value);
		}
	}
}
