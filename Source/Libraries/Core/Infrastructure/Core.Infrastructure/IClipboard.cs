namespace Core.Infrastructure
{
	public interface IClipboard
	{
		void Clear();
		string GetText();
		void SetText(string text);
	}
}
