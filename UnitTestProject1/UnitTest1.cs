using Jsw;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private string testData = @"<T01><IDNO Position=""011-020"" >A222222222</IDNO><NAME Position = ""021-032"" >陳○○</NAME><ALIAS Position = ""071-074"" /></T01>";
        [TestMethod]
        public void GetValueXML()
        {
            JswXML jswXML = new JswXML();
            XElement data = jswXML.Parse(testData);
            Assert.AreEqual("陳○○", data.Element("NAME").Value);
        }
        [TestMethod]
        public void GetPositionXML()
        {
            JswXML jswXML = new JswXML();
            XElement data = jswXML.Parse(testData);
            Assert.AreEqual("021-032", data.Element("NAME").Attribute("Position").Value);
        }

        [TestMethod]
        public void ModifyXML()
        {
            JswXML jswXML = new JswXML();
            XElement a = jswXML.Parse(testData);
            a.Element("NAME").Value = "abc";
            Assert.AreEqual("abc", a.Element("NAME").Value);
        }
        [TestMethod]
        public void MakeNewXML()
        {
            JswXML jswXML = new JswXML();
            XElement a = jswXML.Parse(testData);
            a.Element("NAME").Value = "abc";
            a.Element("IDNO").Value = "A123456789";
            a.Element("ALIAS").Value = "";
            Assert.AreEqual("<T01>\r\n  <IDNO Position=\"011-020\">A123456789</IDNO>\r\n  <NAME Position=\"021-032\">abc</NAME>\r\n  <ALIAS Position=\"071-074\"></ALIAS>\r\n</T01>", a.ToString());
        }

        [TestMethod]
        public void RemoveXMLAllAttributes()
        {
            JswXML jswXML = new JswXML();
            XElement a = jswXML.Parse(testData);
            a.Element("NAME").Value = "abc";
            jswXML.RemoveAllAttributesRecursively(a);
            Assert.AreEqual("<T01>\r\n  <IDNO>A222222222</IDNO>\r\n  <NAME>abc</NAME>\r\n  <ALIAS />\r\n</T01>", a.ToString());
        }

        [TestMethod]
        public void TravelXMLTree()
        {
            JswXML jswXML = new JswXML();
            XElement a = jswXML.Parse(testData);
            jswXML.ProcessNodeRecursively(a);
        }
        class TestJob1 : JobDealerPrototype
        {
            public int count = 0;
            public override void DoJob(XElement e, XAttribute a)
            {
                Debug.Print(e.Name + "_" + a.Name + "_" + a.Value);
                count++;
            }
        }

        [TestMethod]
        public void DoJobXML1()
        {
            JswXML jswXML = new JswXML();
            TestJob1 testJob = new TestJob1();
            jswXML.JobDealer.Add("Position", testJob);
            XElement a = jswXML.Parse(testData);
            jswXML.ProcessNodeRecursively(a);
            Assert.AreEqual(3, testJob.count);
        }

        class TestJob2 : JobDealerPrototype
        {
            public int count = 0;
            public override void DoJob(XElement e, XAttribute a)
            {
                Debug.Print(e.Name.ToString());
                count++;
            }
        }

        [TestMethod]
        public void DoJobXML2()
        {
            JswXML jswXML = new JswXML();
            TestJob2 testJob = new TestJob2();
            jswXML.JobDealer.Add("T01", testJob);
            XElement a = jswXML.Parse(testData);
            jswXML.ProcessNodeRecursively(a);
            Assert.AreEqual(1, testJob.count);
        }

        [TestMethod]
        public void DoJobXML3()
        {
            JswXML jswXML = new JswXML();
            TestJob2 testJob = new TestJob2();
            jswXML.JobDealer.Add("MustDoForAnyNode", testJob);
            XElement a = jswXML.Parse(testData);
            jswXML.ProcessNodeRecursively(a);
            Assert.AreEqual(4, testJob.count);
        }
    }
}
