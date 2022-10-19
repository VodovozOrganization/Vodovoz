using System.IO;
using System.Text;

namespace VodovozInfrastructure
{
	public sealed class StringWriterWithEncoding : StringWriter
	{
		public StringWriterWithEncoding(Encoding encoding)
		{
			Encoding = encoding;
		}

		public override Encoding Encoding { get; }
	}
}
