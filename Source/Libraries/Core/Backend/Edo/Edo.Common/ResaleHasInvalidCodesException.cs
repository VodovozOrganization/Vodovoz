namespace Edo.Common
{
	public class ResaleHasInvalidCodesException : System.Exception
	{
		public override string Message => "Обнаружены не валидные коды в перепродаже";
	}
}
