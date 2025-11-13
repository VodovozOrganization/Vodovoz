namespace Edo.Problems.Exception.EdoExceptions
{
	public class ResaleMissingCodesOnFillInventPositionsException : System.Exception
	{
		public override string Message => "При перепродаже обнаружено отсутствие кодов при создании позиций для УПД";
	}
}
