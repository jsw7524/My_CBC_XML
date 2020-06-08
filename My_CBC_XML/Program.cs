using Jsw;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
namespace MySample
{
    class PositionDealer : JobDealerPrototype
    {
        public Dictionary<string, string> positions = new Dictionary<string, string>();
        public override void DoJob(XElement e, XAttribute a)
        {
            positions[e.Name.ToString()] = a.Value;
        }
    }
    class DataToXMLDealer : JobDealerPrototype
    {
        public Dictionary<string, string> positions;
        public Byte[] buffer;
        public XElement root = null;
        public DataToXMLDealer(Dictionary<string, string> p, Byte[] rawdata , XElement r)
        {
            positions = p;
            buffer = rawdata;
            root = r;
        }
        public string ConvertBIG5toUTF8(byte[] big5Array)
        {
            byte[] utf8Array = System.Text.Encoding.Convert(System.Text.Encoding.GetEncoding(950), System.Text.Encoding.UTF8, big5Array);
            return System.Text.Encoding.UTF8.GetString(utf8Array);
        }
        private string ReplaceLowOrderASCIICharacters(string tmp)
        {
            StringBuilder info = new StringBuilder();
            foreach (char cc in tmp)
            {
                int ss = (int)cc;
                if (((ss >= 0) && (ss <= 8)) || ((ss >= 11) && (ss <= 12)) || ((ss >= 14) && (ss <= 32)))
                    info.AppendFormat(" ", ss);
                else 
                    info.Append(cc);
            }
            return info.ToString();
        }
        public override void DoJob(XElement e, XAttribute a)
        {
            if (positions.ContainsKey(e.Name.ToString()))
            {
                int l = 0, r = 0;
                GetRange(positions[e.Name.ToString()], out l, out r);
                byte[] tmp = new byte[r - l + 1];
                for (int i = l - 1; i < r; i++)
                {
                    tmp[i - (l - 1)] = buffer[i];
                }
                e.Value = ReplaceLowOrderASCIICharacters(ConvertBIG5toUTF8(tmp)).Trim();
            }
        }
    }
    class Program
    {
        public static XElement MakeXML(string tableNumber, Byte[] rawdata)
        {
            JswXML jswXML1= new JswXML();
            PositionDealer positionDealer = new PositionDealer();
            jswXML1.JobDealer.Add("Position", positionDealer);
            XElement b = jswXML1.Parse(File.ReadAllText(tableNumber));
            jswXML1.ProcessNodeRecursively(b);

            JswXML jswXML2 = new JswXML();
            XElement a= jswXML2.Parse(File.ReadAllText(tableNumber));
            DataToXMLDealer dataToXMLDealer = new DataToXMLDealer(positionDealer.positions, rawdata, a);
            jswXML2.JobDealer.Add("MustDoForAnyNode", dataToXMLDealer);
            jswXML2.ProcessNodeRecursively(a);
            jswXML2.RemoveAllAttributesRecursively(a);
            return dataToXMLDealer.root;
        }
        static void Main(string[] args)
        {
            Byte[] buffer = new Byte[360];
            XDocument doc = XDocument.Parse(File.ReadAllText("Frame.xml"));
            ASCIIEncoding ascii = new ASCIIEncoding();
            using (var fs = File.OpenRead("Sample.va"))
            {
                while (fs.Read(buffer, 0, 360) == 360)
                {
                    string n = ascii.GetString(buffer, 32, 2);
                    var node = MakeXML("T"+ n + ".txt", buffer);
                    node.Element("SEX").Value = "1";
                    doc.Root.Add(node);
                }
            }
            doc.Save("Final.xml");
        }
    }
}