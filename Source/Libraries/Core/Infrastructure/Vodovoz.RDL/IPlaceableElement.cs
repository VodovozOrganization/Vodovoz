namespace Vodovoz.RDL
{
	public interface IPlaceableElement
	{
		string Top { get; set; }
		decimal TopSize { get; set; }
		string TopDimension { get; set; }
		string Left { get; set; }
		decimal LeftSize { get; set; }
		string LeftDimension { get; set; }
		void ParseTopValue(string value);
		void ParseLeftValue(string value);
	}
}
