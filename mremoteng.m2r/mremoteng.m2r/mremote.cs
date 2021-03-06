﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Diagnostics;

namespace mremoteng.m2r
{
    class mremote
    {
        private static log4net.ILog log;
        private XmlDocument mainSourceXml;
        private XmlDocument inventoryXml;
        

        public mremote(String onlineSourcePath,
                       String inventoryNodeName) {
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("initializing mremote connector");
            loadXmls(onlineSourcePath, inventoryNodeName);
            log.Info("Launching mremoteng application");
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("ProgramFiles(x86)") + "\\mremoteng\\mRemoteNGo.exe");
            }
            catch (Exception e) {
                log.Error("Can't launch mremote application",e);
            }
            //kill current process
            //Process.GetCurrentProcess().Kill();
        }

        private void loadXmls(String inventorySourcePath,
                              String inventoryNodeName)
        {
            String XPATH_SAASINVENTORY = "//Node[@Name='"+inventoryNodeName+"']";
            String XPATH_ROOT = "Connections";
            XmlNode mainConnections;            
            XmlNode saaSInventory;
            mainSourceXml = new XmlDocument();
            inventoryXml = new XmlDocument();

            
            //main xml source located at %APPDATA%\mremoteng
            string sourceDataPath = Environment.GetEnvironmentVariable("APPDATA") + "\\mremoteng";
            //load the main source
            log.Info("loading mremote source: " + sourceDataPath + "\\confCons.xml");
            mainSourceXml.Load(sourceDataPath + "\\confCons.xml");

            //new source is located at path passed to the constructor
            log.Info("loading xml received from MSM: onlineSourcePath");
            inventoryXml.Load(inventorySourcePath);
            
            //replace existing "online" element with new copy
            log.Info("Replacing " + XPATH_SAASINVENTORY);
            mainConnections = mainSourceXml.SelectSingleNode(XPATH_ROOT);
            saaSInventory = inventoryXml.SelectSingleNode(XPATH_SAASINVENTORY);
            
            saaSInventory = mainSourceXml.ImportNode(saaSInventory, true);
            mainConnections.RemoveChild(mainConnections.SelectSingleNode(XPATH_SAASINVENTORY));
            mainConnections.AppendChild(saaSInventory);
            mainSourceXml.ReplaceChild(mainConnections, mainSourceXml.ChildNodes[1]);
            log.Info("successfully updated mremote source");

            mainSourceXml.Save(sourceDataPath + "\\confCons.xml");
            log.Info(sourceDataPath + "\\confCons.xml - Saved");

        }
    }
}
