using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.ViewModels.TrueMark
{
	public class OrderCodeItemViewModel : QS.ViewModels.ViewModelBase
	{
		private OrderCodeItemViewModel _parent;
		private List<OrderCodeItemViewModel> _children = new List<OrderCodeItemViewModel>();
		private TrueMarkTransportCode _transportCode;
		private TrueMarkWaterGroupCode _groupCode;
		private string _type;
		private TrueMarkWaterIdentificationCode _sourceCode;
		private string _sourceIdentificationCode;
		private TrueMarkWaterIdentificationCode _resultCode;
		private string _resultIdentificationCode;
		private bool _replacedFromPool;
		private ProductCodeProblem _problem;
		private int? _sourceDocumentId;
		private int? _codeAuthorId;
		private string _codeAuthor;
		private string _unscannedCodesReason;

		public virtual OrderCodeItemViewModel Parent
		{
			get => _parent;
			set => SetField(ref _parent, value);
		}

		public virtual List<OrderCodeItemViewModel> Children
		{
			get => _children;
			set => SetField(ref _children, value);
		}

		public virtual TrueMarkTransportCode TransportCode
		{
			get => _transportCode;
			set
			{
				SetField(ref _transportCode, value);
				if(value != null)
				{
					SourceIdentificationCode = value.RawCode;
					Type = "Транспортный";
				}
			}
		}

		public virtual TrueMarkWaterGroupCode GroupCode
		{
			get => _groupCode;
			set
			{
				SetField(ref _groupCode, value);
				if(value != null)
				{
					SourceIdentificationCode = value.IdentificationCode;
					Type = "Групповой";
				}
			}
		}

		public virtual TrueMarkWaterIdentificationCode SourceCode
		{
			get => _sourceCode;
			set
			{
				SetField(ref _sourceCode, value);
				if(value != null)
				{
					SourceIdentificationCode = value.IdentificationCode;
					Type = "Экземплярный";
				}
			}
		}

		public virtual string Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		public virtual string SourceIdentificationCode
		{
			get => _sourceIdentificationCode;
			set => SetField(ref _sourceIdentificationCode, value);
		}

		public virtual TrueMarkWaterIdentificationCode ResultCode
		{
			get => _resultCode;
			set
			{
				SetField(ref _resultCode, value);
				if(value != null)
				{
					ResultIdentificationCode = value.IdentificationCode;
				}
			}
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

		/// <summary>
		/// Причина не отсканированных кодов
		/// </summary>
		public string UnscannedCodesReason
		{
			get => _unscannedCodesReason;
			set => SetField(ref _unscannedCodesReason, value);
		}
	}
}
