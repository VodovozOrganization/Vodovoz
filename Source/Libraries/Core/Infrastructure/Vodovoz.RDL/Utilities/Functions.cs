using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Vodovoz.RDL.Utilities
{
	public static class Functions
	{		
		public static XElement ToXElement<T>(this object element, string @namespace = null)
		{
			var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
			var serializer = new XmlSerializer(typeof(T), @namespace);

			using(var memoryStream = new MemoryStream())
			using(var streamWriter = new StreamWriter(memoryStream))
			{
				serializer.Serialize(streamWriter, element, namespaces);
				var bytes = memoryStream.ToArray();
				var stringContent = Encoding.UTF8.GetString(bytes);
				var result = XElement.Parse(stringContent);
				return result;
			}
		}

		public static T FromXElement<T>(this XElement xElement, string @namespace = null)
		{
			if(@namespace == null)
			{
				@namespace = xElement.Document.Root.Attribute("xmlns").Value;
			}
			var serializer = new XmlSerializer(typeof(T), @namespace);
			using(var reader = xElement.CreateReader())
			{
				reader.MoveToContent();
				var result = serializer.Deserialize(reader);
				return (T)result;
			}
		}

		public static T CloneElement<T>(this T element, string @namespace = null)
		{
			var xElement = ToXElement<T>(element, @namespace);
			var result = FromXElement<T>(xElement, @namespace);
			return result;
		}

		public static PropertyInfo GetPropertyInfo<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
		{
			MemberExpression memberExpr = propertyRefExpr.Body as MemberExpression;
			if(memberExpr == null)
			{
				UnaryExpression unaryExpr = propertyRefExpr.Body as UnaryExpression;
				if(unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
				{
					memberExpr = unaryExpr.Operand as MemberExpression;
				}
			}

			if(memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
			{
				return memberExpr.Member as PropertyInfo;
			}
			else
			{
				return null;
			}
		}
	}
}
