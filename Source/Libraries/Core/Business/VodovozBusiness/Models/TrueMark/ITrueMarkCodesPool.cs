namespace Vodovoz.Models.TrueMark
{
	public interface ITrueMarkCodesPool
	{
		void PutCode(string code);
		void PutDefectiveCode(string code);
		string TakeCode();
		string TakeDefectiveCode();
	}
}