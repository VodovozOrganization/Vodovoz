using System;

namespace TrueMark.Contracts
{
	public class TrueMarkApiDocumentDto
	{
		public Guid Guid { get; set; }
		public bool IsSuccess { get; set; }
		public string ErrorMessage { get; set; }
	}
}
