namespace Edo.Problems.Exception.EdoExceptions
{
	public class ResaleMissingCodesException : System.Exception
	{
		public ResaleMissingCodesException() : base("При перепродаже обнаружено отсутствие кодов")
		{
		}

		public ResaleMissingCodesException(string message) : base(message)
		{
		}
	}
}
