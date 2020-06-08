using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Jsw
{
    public interface IJobDealer
    {
        void DoJob(XElement e, XAttribute a);
    }

    public class JswXML : DynamicXml
    {
        public  Dictionary<string, IJobDealer> JobDealer = new Dictionary<string, IJobDealer>();

        public JswXML(XElement root) :base( root)
        {

        }

        public JswXML() 
        {

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
