using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Jsw
{
    public interface IJobDealer
    {
        void DoJob(XElement e, XAttribute a);
    }
    public class JobDealerPrototype: IJobDealer
    {
        public virtual void DoJob(XElement e, XAttribute a) { }
        public virtual void GetRange(string range, out int l, out int r)
        {
            if (range.Contains("-"))
            {
                Regex regex = new Regex(@"(?<start>\d+)-(?<end>\d+)");
                var m = regex.Match(range);
                int.TryParse(m.Groups["start"].Value, out l);
                int.TryParse(m.Groups["end"].Value, out r);
                return;
            }
            int.TryParse(range, out l);
            int.TryParse(range, out r);
            return;
        }
    }

    public class JswXML 
    {
        public  Dictionary<string, IJobDealer> JobDealer = new Dictionary<string, IJobDealer>();

        public JswXML() 
        {

        }

        public XElement Parse(string data)
        {
            XDocument doc = XDocument.Parse(data);
            return doc.Root;
        }

        public  void ProcessNodeRecursively(XElement e)
        {
            if (JobDealer.ContainsKey("MustDoForAnyNode"))
            {
                JobDealer["MustDoForAnyNode"].DoJob(e, null);
            }

            if (JobDealer.ContainsKey(e.Name.ToString()))
            {
                JobDealer[e.Name.ToString()].DoJob(e, null);
            }

            if (e.HasAttributes)
            {
                var atrr = e.Attributes();
                foreach (var a in atrr)
                {
                    if (JobDealer.ContainsKey(a.Name.ToString()))
                    {
                        JobDealer[a.Name.ToString()].DoJob(e,a);
                    }

                }
            }

            if (e.HasElements)
            {
                var children = e.Elements();
                foreach (var child in children)
                {
                    ProcessNodeRecursively(child);
                }
            }
        }


        public  void RemoveAllAttributesRecursively(XElement e)
        {
            if (e.HasAttributes)
            {
                e.RemoveAttributes();
            }
            if (e.HasElements)
            {
                var children=e.Elements();
                foreach (var child in children)
                {
                    RemoveAllAttributesRecursively(child);
                }
            }
        }


    }

}
