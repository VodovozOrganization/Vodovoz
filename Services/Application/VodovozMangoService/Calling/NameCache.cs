using System;
using Newtonsoft.Json;

namespace VodovozMangoService.Calling
{
    public class NameCache
    {
        public string Name { get; private set; }
        public string Number{ get; private set; }
        private DateTime created = DateTime.Now;

        public NameCache(string number, string name)
        {
            Name = name;
            Number = number;
        }

        public TimeSpan LiveTime => DateTime.Now - created;
    }
}