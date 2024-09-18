using System.Collections.Generic;
using System.Linq;

namespace FrameworkSupport
{
    public class CommandLineArgsHelper : Dictionary<string, string>
    {
        public CommandLineArgsHelper(string[] args)
        {
            args.ToList().ForEach(a =>
            {
                string[] components = a.Split(new char[] { '=' }, 2);
                this.Add(components[0], components.Length > 1 ? components[1] : null);
            });
        }

        public bool IsTrue(string key) => this.ContainsKey(key) && bool.Parse(this[key]);

        public string ArgList => string.Join(" ", this.Select(kp => $"{kp.Key}={kp.Value}"));

        public bool HasFlag(string flag) => this.Keys.Any(k => k == $"-{flag}");
    }
}