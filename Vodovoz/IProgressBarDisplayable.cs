namespace Vodovoz
{
	public interface IProgressBarDisplayable
	{
		void ProgressStart(double maxValue, double minValue = 0, string text = null, double startValue = 0);

		void ProgressUpdate(double curValue);

		void ProgressUpdate(string curText);

		void ProgressAdd(double addValue = 1, string text = null);

		void ProgressClose();
	}
}
