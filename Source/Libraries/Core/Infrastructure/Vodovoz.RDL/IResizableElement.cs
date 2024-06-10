namespace Vodovoz.RDL
{
	public interface IResizableElement
	{
		string Height { get; set; }
		decimal HeightSize { get; set; }
		string HeightDimension { get; set; }
		string Width { get; set; }
		decimal WidthSize { get; set; }
		string WidthDimension { get; set; }
		void ParseWidthValue(string value);
		void ParseHeightValue(string value);
	}
}
