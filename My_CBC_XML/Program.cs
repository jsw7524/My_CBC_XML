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

    class PositionDealer : IJobDealer
    {
        public Dictionary<string, string> positions = new Dictionary<string, string>();
        public void DoJob(XElement e, XAttribute a)
        {
            if ("?"!=a.Value.ToString())
            {
                positions[e.Name.ToString()] = a.Value;
            }
            
        }
    }

    class DataToTxtDealer : IJobDealer
    {
        public Dictionary<string, string> positions;
        public Byte[] buffer= new Byte[360];
        public DataToTxtDealer(Dictionary<string, string> p)
        {
            positions = p;
            for (int i = 0; i < 360; i++)
            {
                buffer[i] = 32;
            }
        }

        public byte[] ConvertUTF8toBIG5(string strInput)
        {
            byte[] strut8 = System.Text.Encoding.UTF8.GetBytes(strInput);
            return System.Text.Encoding.Convert(System.Text.Encoding.UTF8, System.Text.Encoding.GetEncoding(950), strut8);
        }

        public void DoJob(XElement e, XAttribute a)
        {

            if (positions.ContainsKey(e.Name.ToString()))
            {
                if (""==e.Value.ToString())
                {
                    return;
                }

                var content = ConvertUTF8toBIG5(e.Value.ToString());
                int n=0;
                if (positions[e.Name.ToString()].Contains("?"))
                {

                }
                else if (positions[e.Name.ToString()].Contains("-"))
                {
                    int l=0, r=0,count=0;
                    Regex regex = new Regex(@"(?<start>\d+)-(?<end>\d+)");
                    var m = regex.Match(positions[e.Name.ToString()]);
                    int.TryParse(m.Groups["start"].Value, out l);
                    int.TryParse(m.Groups["end"].Value, out r);
                    for (int i = l-1; i <r && count < content.Length; i++)
                    {
                        buffer[i] = content[i-(l-1)];
                        count += 1;
                    }

                }
                else if(int.TryParse(positions[e.Name.ToString()], out n))
                {
                    buffer[n] = content[0];
                }
            }
        }

        public void OutputByteFile(string filename)
        {
            File.WriteAllBytes(filename, buffer);
        }
    }

    class DataToXMLDealer : IJobDealer
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
                    info.AppendFormat(" ", ss);//&#x{0:X};
                else info.Append(cc);
            }
            return info.ToString();
        }
        public void DoJob(XElement e, XAttribute a)
        {
            if (positions.ContainsKey(e.Name.ToString()))
            {
                int n = 0;
                if (positions[e.Name.ToString()].Contains("?"))
                {

                }
                else if (positions[e.Name.ToString()].Contains("-"))
                {

                    int l = 0, r = 0;
                    Regex regex = new Regex(@"(?<start>\d+)-(?<end>\d+)");
                    var m = regex.Match(positions[e.Name.ToString()]);
                    int.TryParse(m.Groups["start"].Value, out l);
                    int.TryParse(m.Groups["end"].Value, out r);
                    byte[] tmp = new byte[r-l+1];
                    for (int i = l-1; i < r ; i++)
                    {
                        tmp[i-(l-1)] = buffer[i];
                    }
                    e.Value= ReplaceLowOrderASCIICharacters(ConvertBIG5toUTF8(tmp)).Trim();
                }
                else if (int.TryParse(positions[e.Name.ToString()], out n))
                {
                    byte[] tmp = new byte[1];
                    tmp[0] = buffer[n];
                    e.Value = ReplaceLowOrderASCIICharacters(ConvertBIG5toUTF8(tmp)).Trim();
                }
            }

        }

        public void OutputXMLFile(string filename)
        {
            root.Save(filename);
        }
    }

    class Program
    {
        public static void ReceiveFile()
        {
            JswXML jswXML;
            XElement a;
            jswXML = new JswXML();
            PositionDealer positionDealer = new PositionDealer();
            jswXML.JobDealer.Add("Position", positionDealer);
            a = jswXML.Parse(File.ReadAllText("SchemaXML.txt"));
            jswXML.ProcessNodeRecursively(a);

            jswXML = new JswXML();
            DataToTxtDealer dataDealer = new DataToTxtDealer(positionDealer.positions);
            jswXML.JobDealer.Add("MustDoForAnyNode", dataDealer);
            a = jswXML.Parse(File.ReadAllText("DataXML.txt"));
            jswXML.ProcessNodeRecursively(a);
            dataDealer.OutputByteFile("test.va");
        }

        public static void MakeXMLFile()
        {
            JswXML jswXML;
            XElement b;
            jswXML = new JswXML();
            PositionDealer positionDealer = new PositionDealer();
            jswXML.JobDealer.Add("Position", positionDealer);
            b = jswXML.Parse(File.ReadAllText("SchemaXML.txt"));
            jswXML.ProcessNodeRecursively(b);

            XElement a;
            a = jswXML.Parse(File.ReadAllText("EmptyXML.txt"));
            var rawdata=File.ReadAllBytes("Sample.va");
            jswXML = new JswXML();
            DataToXMLDealer dataToXMLDealer = new DataToXMLDealer(positionDealer.positions, rawdata, a);
            jswXML.JobDealer.Add("MustDoForAnyNode", dataToXMLDealer);
            jswXML.ProcessNodeRecursively(a);
            dataToXMLDealer.OutputXMLFile("test.xml");

        }
        static void Main(string[] args)
        {
            //ReceiveFile();
            //MakeXMLFile();


        }
    }
}
