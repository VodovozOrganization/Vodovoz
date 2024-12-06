namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	public enum SourceProductCodeStatus
	{
		/// <summary>
		/// Есть не решаемая проблема с кодом <br/>
		/// Дальнейшее использование кода невозможно
		/// </summary>
		Problem,

		/// <summary>
		/// Была проблема с кодом, код был заменен <br/>
		/// Можно продолжить работу с замененным кодом в ResultCode
		/// </summary>
		Changed,

		/// <summary>
		/// Код принят без изменений
		/// </summary>
		Accepted
	}
}
