using System.IO;
using System.IO.Compression;
using System.Xml;

namespace TaxcomEdo.Client
{
	/// <summary>
	/// Итог приёмки в титуле покупателя, атрибут КодИтога.
	/// Код транзакции при приёмке, расхождении и отказе один и тот же.
	/// </summary>
	public static class BuyerTitleAcceptance
	{
		public const string AcceptedWithoutDiscrepancies = "1";
		public const string AcceptedWithDiscrepancies = "2";
		public const string NotAccepted = "3";

		private const int MaxEntryBytes = 20 * 1024 * 1024;
		private const string ResultCodeAttribute = "КодИтога";

		private static readonly XmlReaderSettings XmlSettings = new XmlReaderSettings
		{
			DtdProcessing = DtdProcessing.Prohibit
		};

		public static string ReadResultCode(byte[] raw)
		{
			if(raw == null || raw.Length == 0)
			{
				return null;
			}

			if(IsZip(raw))
			{
				return ReadArchiveResultCode(raw, 0);
			}

			return ReadXmlResultCode(raw);
		}

		private static string ReadArchiveResultCode(byte[] raw, int depth)
		{
			if(depth > 1)
			{
				return null;
			}

			string resultCode = null;
			using(var stream = new MemoryStream(raw, false))
			using(var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
			{
				foreach(var entry in archive.Entries)
				{
					if(string.IsNullOrEmpty(entry.Name) || entry.Length > MaxEntryBytes)
					{
						continue;
					}

					var entryBytes = ReadEntry(entry);
					if(entryBytes == null)
					{
						continue;
					}

					resultCode = Stronger(resultCode, ReadResultCode(entryBytes, depth + 1));
					if(resultCode == NotAccepted)
					{
						return resultCode;
					}
				}
			}

			return resultCode;
		}

		private static string ReadResultCode(byte[] raw, int depth)
		{
			if(IsZip(raw))
			{
				return ReadArchiveResultCode(raw, depth);
			}

			return ReadXmlResultCode(raw);
		}

		private static string ReadXmlResultCode(byte[] content)
		{
			string resultCode = null;
			try
			{
				using(var stream = new MemoryStream(content, false))
				using(var reader = XmlReader.Create(stream, XmlSettings))
				{
					while(reader.Read())
					{
						if(reader.NodeType != XmlNodeType.Element)
						{
							continue;
						}

						resultCode = Stronger(resultCode, reader.GetAttribute(ResultCodeAttribute));
						if(resultCode == NotAccepted)
						{
							return resultCode;
						}
					}
				}
			}
			catch(XmlException)
			{
				return resultCode;
			}

			return resultCode;
		}

		private static string Stronger(string current, string candidate)
		{
			return Rank(candidate) > Rank(current) ? candidate : current;
		}

		private static int Rank(string resultCode)
		{
			if(resultCode == NotAccepted)
			{
				return 3;
			}

			if(resultCode == AcceptedWithDiscrepancies)
			{
				return 2;
			}

			if(resultCode == AcceptedWithoutDiscrepancies)
			{
				return 1;
			}

			return 0;
		}

		private static byte[] ReadEntry(ZipArchiveEntry entry)
		{
			using(var entryStream = entry.Open())
			using(var buffer = new MemoryStream())
			{
				var chunk = new byte[8192];
				int read;
				while((read = entryStream.Read(chunk, 0, chunk.Length)) > 0)
				{
					if(buffer.Length + read > MaxEntryBytes)
					{
						return null;
					}

					buffer.Write(chunk, 0, read);
				}

				return buffer.ToArray();
			}
		}

		private static bool IsZip(byte[] raw)
		{
			return raw.Length >= 2 && raw[0] == 0x50 && raw[1] == 0x4B;
		}
	}
}
