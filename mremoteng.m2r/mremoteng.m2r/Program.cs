using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;

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
            BasicConfigurator.Configure();

            log.Debug("Starting...");

            //online resource for testing only
            mremote = new mremote(Properties.Resources.onlinesource);
            


            //read m2r settings file - get xml path
            //backup the original xml source 
            //copy the new source

            //open the new xml addition
            //open the full source xml
            //replace the ONLINE section





        }
    }
}
