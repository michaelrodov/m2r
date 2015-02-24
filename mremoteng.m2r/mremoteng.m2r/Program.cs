using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;
using System.Configuration;
using System.Xml;

namespace mremoteng.m2r
{
    class Program
    {
        private static ILog log;
        private static mremote mremote; 
        static void Main(string[] args)
        {
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            //This method initializes the log4net system to use a simple Console appender
            //BasicConfigurator.Configure();

            //load config file
            XmlDocument configs = new XmlDocument();
            configs.Load("m2r.xml");

            //config data
            string msmfile = configs.SelectSingleNode("/mremote/msmfile").InnerText;
            string msmnode = configs.SelectSingleNode("/mremote/msmnode").InnerText;
            
            //open mremote launcher and connections file configurator
            mremote = new mremote(msmfile, msmnode);
            

        }
    }
}
