namespace Vodovoz.Extensions
{
	public static class ColorExtensions
	{
		public static Gdk.Color ToGdkColor(this System.Drawing.Color drawingsColor) =>
			new Gdk.Color(drawingsColor.R, drawingsColor.G, drawingsColor.B);

		public static System.Drawing.Color ToDrawingsColor(this Gdk.Color gdkColor)

		{
			double red = (double)gdkColor.Red / ushort.MaxValue * byte.MaxValue;
			double green = (double)gdkColor.Green / ushort.MaxValue * byte.MaxValue;
			double blue = (double)gdkColor.Blue / ushort.MaxValue * byte.MaxValue;

			return System.Drawing.Color.FromArgb((int)red, (int)green, (int)blue);
		}

		public static bool IsLight(this Gtk.Style style)
		{
			var fontColor = style.Foreground(Gtk.StateType.Normal).ToDrawingsColor();
			double index = fontColor.R * 0.299 + fontColor.G * 0.587 + fontColor.B * 0.114;
			return index < 186;
		}

		public static bool IsDark(this Gtk.Style style) => !IsLight(style);
	}
}
