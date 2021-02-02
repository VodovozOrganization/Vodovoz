using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace VodovozInfrastructure.Utils {
    public class NamesUtils {
        
        public static string GetSecondNameFromFullName(string fullName)
        {
            var a = fullName.Split(' ');
            if (a.Length == 3){
                return a[1].Trim();
            }
            else{
                throw new ArgumentException("The full name must have 2 spaces(3 names)");
            }
        }
    }
}