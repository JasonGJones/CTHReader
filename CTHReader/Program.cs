using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security;
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
            
            
            if (ConfigurationManager.AppSettings["passwordPrompt"] == "true")
            {
                Console.WriteLine("Please provide user name to connect to the site:");
                cth.UserName = Console.ReadLine();

                Console.WriteLine("Please provide the password:");
                cth.UserPassword = getPassword();

                Console.WriteLine();
               
            }
            cth.OutputFilePrefix = ConfigurationManager.AppSettings["outputFilePrefix"];
            
            string outputLocation = ConfigurationManager.AppSettings["outputLocation"];
            string siteUrl = ConfigurationManager.AppSettings["siteUrl"];
            Console.WriteLine(cth.ProcessCTH(siteUrl, outputLocation, CTHReader.CTHQueryMode.CTHAndSiteColumns));
            //doit();
            //tidyUpXMLDoc();
            //queryXMLProcess();

            

        }

        private static SecureString getPassword()
        {
            SecureString pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
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
