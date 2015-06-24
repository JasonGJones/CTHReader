using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CTHReader
{
    class Program
    {
        static void Main(string[] args)
        {
            CTHReader cth = new CTHReader();
            string outputLocation = ConfigurationManager.AppSettings["outputLocation"];
            string siteUrl = ConfigurationManager.AppSettings["siteUrl"];
            Console.WriteLine(cth.ProcessCTH(siteUrl, outputLocation, CTHReader.CTHQueryMode.CTHAndSiteColumns));
            //doit();
            //tidyUpXMLDoc();
            //queryXMLProcess();
            

        }

        private static void queryXMLProcess()
        {
            XDocument doc = new XDocument(
                new XElement("Processes",
                    from p in Process.GetProcesses()
                    select new XElement("Process",
                        new XAttribute("PID", p.Id),
                        new XAttribute("PName", p.ProcessName)
                        )

                    ));
        }

        //private static void tidyUpXMLDoc()
        //{
        //    XDocument xd = XDocument.Load(@"C:\temp\allcts.xml");
        //    XDocument xdOut = removeUnwantedAttributes(xd);
        //    xdOut.Save(@"C:\temp\allctsmod.xml");
        //}

       

        
    }
}
