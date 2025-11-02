using System;

namespace Vodovoz.Core
{
    public class WidgetResolveException : Exception
    {
        public WidgetResolveException()
        {
        }
        
        public WidgetResolveException(string message) : base(message)
        {
        }
        
        public WidgetResolveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}