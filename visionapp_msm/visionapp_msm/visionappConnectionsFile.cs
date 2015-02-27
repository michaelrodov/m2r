using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace visionapp_msm
{
    class visionappConnectionsFile
    {
        private String owner;

        private int currentConnectionFolderIdCount;
        private int currentCredFolderIdCount;
        private int currentAppFolderIdCount;

        //"Workaround variables" to speed up the adding process!
        // this system only works if the input is sorted!
        private int currentProductFolderId;
        private String currentProductFolderName;
        private int currentDatacenterFolderId;
        private String currentDatacenterFolderName;
        private int currentFarmFolderId;
        private String currentFarmFolderName;
        
        private XElement credentials;
        private XElement connections;
        private XElement folders;
        private XElement credFolders;
        private XElement credPlaces;
        private XElement indexTable;
        private XElement history;
        private XElement appFolders;
        private XElement externalApps;
        private XElement connStates;

        public visionappConnectionsFile(String owner)
        {
            this.owner = owner;
            currentConnectionFolderIdCount = 0;
            currentCredFolderIdCount = 0;
            currentAppFolderIdCount = 0;

            currentProductFolderId = 0;
            currentProductFolderName = "";
            currentDatacenterFolderId = 0;
            currentDatacenterFolderName = "";
            currentFarmFolderId = 0;
            currentFarmFolderName = "";

            credentials = new XElement("Credentials");
            connections = new XElement("Connections");
            folders = new XElement("Folders");
            credFolders = new XElement("CredFolders");
            credPlaces = new XElement("CredPlaces");
            indexTable = new XElement("IndexTable");
            history = new XElement("History");
            appFolders = new XElement("AppFolders");
            externalApps = new XElement("ExternalApps");
            connStates = new XElement("ConnStates");

            addFolder("Connections");
            addCredFolder("Credentials");
            addCredPlace(owner, "0", "0", "false");
            addAppFolder("External Applications");

        }

        public void addServer(String serverName, 
            String serverIp,
            String serverRole,
            String serverOperatingSystem,
            String product, 
            String datacenter, 
            String farm)
        {
            //We can have the same farm in different datacenters: (empty farm)
            // so we need a way to track when we're in a different datacenter
            // but in the same farm!
            bool newDatacenterFlag = false;

            //First add the product folder
            if(product!=currentProductFolderName)
            {
                addFolder(product);
                addCredPlace(owner, (currentConnectionFolderIdCount - 1).ToString(), "0", "false");
                addIndexTableItem((currentConnectionFolderIdCount - 1).ToString(), "Folder", "0");
                currentProductFolderId = currentConnectionFolderIdCount - 1;
                currentProductFolderName = product;
            }
            //String folderId1 = addProduct(product);

            //...then the datacenter folder
            if(datacenter!=currentDatacenterFolderName)
            {
                addFolder(datacenter);
                addCredPlace(owner, (currentConnectionFolderIdCount - 1).ToString(), "0", "true");
                addIndexTableItem((currentConnectionFolderIdCount - 1).ToString(), "Folder", currentProductFolderId.ToString());
                currentDatacenterFolderId = currentConnectionFolderIdCount - 1;
                currentDatacenterFolderName = datacenter;

                newDatacenterFlag = true;
            }
            //String folderId2 = addDatacenter(folderId1, datacenter);

            //...then the farm folder
            if (farm != currentFarmFolderName || newDatacenterFlag)
            {
                addFolder(farm);
                addCredPlace(owner, (currentConnectionFolderIdCount - 1).ToString(), "0", "true");
                addIndexTableItem((currentConnectionFolderIdCount - 1).ToString(), "Folder", currentDatacenterFolderId.ToString());
                currentFarmFolderId = currentConnectionFolderIdCount - 1;
                currentFarmFolderName = farm;
            }
            //String folderId3 = addFarm(folderId2, farm);

            //... and then finally the server!
            addConnection(serverName, serverIp, serverRole, serverOperatingSystem, currentFarmFolderId.ToString());

        }

        public XElement exportConnectionsFile()
        {
            XElement connectionsFile = new XElement("vRDConfig");
            connectionsFile.Add(new XAttribute("Version", "3.0"));
            connectionsFile.Add(credentials);
            connectionsFile.Add(connections);
            connectionsFile.Add(folders);
            connectionsFile.Add(credFolders);
            connectionsFile.Add(credPlaces);
            connectionsFile.Add(indexTable);
            connectionsFile.Add(history);
            connectionsFile.Add(appFolders);
            connectionsFile.Add(externalApps);
            connectionsFile.Add(connStates);
            connectionsFile.Add(new XElement("TreeViewSettings", "PD94bWwgdmVyc2lvbj0iMS4wIj8+DQo8QXJyYXlPZlRyZWVTdGF0ZUl0ZW0geG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeG1sbnM6eHNkPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSI+DQogIDxUcmVlU3RhdGVJdGVtPg0KICAgIDxEYXRhSXRlbUlkPjA8L0RhdGFJdGVtSWQ+DQogICAgPE9iamVjdFR5cGU+dlJEX0Nvbm5lY3Rpb25zX1Jvb3Q8L09iamVjdFR5cGU+DQogICAgPENhdGVnb3J5PmZhbHNlPC9DYXRlZ29yeT4NCiAgICA8U2VsZWN0ZWQ+ZmFsc2U8L1NlbGVjdGVkPg0KICA8L1RyZWVTdGF0ZUl0ZW0+DQogIDxUcmVlU3RhdGVJdGVtPg0KICAgIDxEYXRhSXRlbUlkPjYzNDc8L0RhdGFJdGVtSWQ+DQogICAgPE9iamVjdFR5cGU+dlJEX0ZvbGRlcjwvT2JqZWN0VHlwZT4NCiAgICA8Q2F0ZWdvcnk+ZmFsc2U8L0NhdGVnb3J5Pg0KICAgIDxTZWxlY3RlZD50cnVlPC9TZWxlY3RlZD4NCiAgPC9UcmVlU3RhdGVJdGVtPg0KPC9BcnJheU9mVHJlZVN0YXRlSXRlbT4="));


            return connectionsFile;
        }

/*******************************************************************************************/

        /* These functions were the "old" way to add product, datacenter and farm folders.
         * These were really slow since each function would loop through ALL folders to
         * verify if they existed or not.
         * Now we assume the input "servers" list is sorted (by product, datacenter, farm, and server
         * name... in that order). This way we don't have to search for existing folders and it
         * speeds up the performance by a lot! :)
         * 
        private String addProduct(String product)
        {
            if(!doesProductExist(product))
            {
                addFolder(product);
                addCredPlace(owner, (currentConnectionFolderIdCount - 1).ToString(), "0", "false");
                addIndexTableItem((currentConnectionFolderIdCount - 1).ToString(), "Folder", "0");

                return (currentConnectionFolderIdCount - 1).ToString();
            }
            else
            {
                foreach (XElement e in folders.Elements())
                {
                    if (e.Element("Name").Value == product)
                    {
                        return (e.Attribute("Id").Value);
                    }

                }

                return "0";
            }
        }

        private String addDatacenter(String parentFolderId, String datacenter)
        {
            //Does the datacenter exist under the product?
            foreach (XElement e in folders.Elements())
            {
                if ((e.Element("Name").Value == datacenter) && (getParentItemId(e.Attribute("Id").Value) == parentFolderId))
                {
                    return e.Attribute("Id").Value;
                }

            }

            //If not, add it
            addFolder(datacenter);
            addCredPlace(owner, (currentConnectionFolderIdCount - 1).ToString(), "0", "true");
            addIndexTableItem((currentConnectionFolderIdCount - 1).ToString(), "Folder", parentFolderId);

            return (currentConnectionFolderIdCount - 1).ToString();
        }

        private String addFarm(String parentFolderId, String farm)
        {
            //Does the farm exist under product/datacenter?
            foreach (XElement e in folders.Elements())
            {
                if ((e.Element("Name").Value == farm) && (getParentItemId(e.Attribute("Id").Value) == parentFolderId))
                {
                    return e.Attribute("Id").Value;
                }

            }

            //If not, add it
            addFolder(farm);
            addCredPlace(owner, (currentConnectionFolderIdCount - 1).ToString(), "0", "true");
            addIndexTableItem((currentConnectionFolderIdCount - 1).ToString(), "Folder", parentFolderId);

            return (currentConnectionFolderIdCount - 1).ToString();
        }



        private bool doesProductExist(String product)
        {
            foreach (XElement e in folders.Elements())
            {
                if((e.Element("Name").Value == product) && (getParentItemId(e.Attribute("Id").Value)=="0"))
                {
                    return true;
                }
                
            }

            return false;
        }

        private String getParentItemId(String id)
        {
            foreach( var e in indexTable.Elements())
            {
                if (e.Attribute("Id").Value == id)
                    return e.Attribute("ParentItemId").Value;
            }

            return "";
        }
                
         */

        private void addConnection(String serverName,
            String serverIp,
            String serverRole,
            String serverOperatingSystem,
            String parentFolderId)
        {
            if(serverOperatingSystem.Contains("Windows"))
            {
                //Windows! (RDP)
                XElement newConnection = new XElement("Connection");
                newConnection.Add(new XAttribute("Id", currentConnectionFolderIdCount));
                newConnection.Add(new XElement("LastUpdate", "-8587767832102012914"));
                newConnection.Add(new XElement("LastUpdateBy", "TOOL"));
                newConnection.Add(new XElement("ConnectionName", serverName));
                newConnection.Add(new XElement("ServerName", serverIp));
                newConnection.Add(new XElement("MACAddress", ""));
                newConnection.Add(new XElement("MgmtBoardUrl", ""));
                newConnection.Add(new XElement("Description", serverRole));
                newConnection.Add(new XElement("IconId", "00000000-0000-0000-0000-000000000000"));
                newConnection.Add(new XElement("PreExtAppId", 0));
                newConnection.Add(new XElement("PostExtAppId", 0));
                newConnection.Add(new XElement("Private", "true"));
                newConnection.Add(new XElement("OwnerName", owner));
                newConnection.Add(new XElement("ConnectionPlace", 2));
                newConnection.Add(new XElement("AutoSize", "true"));
                newConnection.Add(new XElement("SeparateResolutionX", 1024));
                newConnection.Add(new XElement("SeparateResolutionY", 768));
                newConnection.Add(new XElement("TabResolutionX", 1024));
                newConnection.Add(new XElement("TabResolutionY", 768));
                newConnection.Add(new XElement("DefaultProtocol", 0));
                newConnection.Add(new XElement("Port", 3389));
                newConnection.Add(new XElement("Console", "false"));
                newConnection.Add(new XElement("ClipBoard", "true"));
                newConnection.Add(new XElement("Printer", "false"));
                newConnection.Add(new XElement("Serial", "false"));
                newConnection.Add(new XElement("LocalDrives", "false"));
                newConnection.Add(new XElement("SmartCard", "false"));
                newConnection.Add(new XElement("EnableNLA", "false"));
                newConnection.Add(new XElement("DesktopBackground", "true"));
                newConnection.Add(new XElement("WindowDrag", "true"));
                newConnection.Add(new XElement("Themes", "true"));
                newConnection.Add(new XElement("BitmapCaching", "true"));
                newConnection.Add(new XElement("SmartSizing", "false"));
                newConnection.Add(new XElement("FontSmoothing", "false"));
                newConnection.Add(new XElement("RemoteAudio", 0));
                newConnection.Add(new XElement("KeyboardMode", 2));
                newConnection.Add(new XElement("RDPColorDepth", 16));
                newConnection.Add(new XElement("TSGUsageMethod", 2));
                newConnection.Add(new XElement("TSGServerName", ""));
                newConnection.Add(new XElement("TSGBypass", "false"));
                newConnection.Add(new XElement("TSGCredId", 0));
                newConnection.Add(new XElement("AuthenticationLevel", 0));
                newConnection.Add(new XElement("UseStartProgram", "false"));
                newConnection.Add(new XElement("StartProgram", ""));
                newConnection.Add(new XElement("WorkDir", ""));
                newConnection.Add(new XElement("ICAPort", 1494));
                newConnection.Add(new XElement("ICAEncryption", 0));
                newConnection.Add(new XElement("ICALocalDrives", "false"));
                newConnection.Add(new XElement("ICAPrinter", "false"));
                newConnection.Add(new XElement("ICALPTPort", "false"));
                newConnection.Add(new XElement("ICACOMPort", "false"));
                newConnection.Add(new XElement("ICACompression", "false"));
                newConnection.Add(new XElement("ICABitmapCaching", "false"));
                newConnection.Add(new XElement("ICAQueue", "false"));
                newConnection.Add(new XElement("ICASessionReliability", "false"));
                newConnection.Add(new XElement("ICARemoteAudio", 0));
                newConnection.Add(new XElement("ICAColorDepth", 2));
                newConnection.Add(new XElement("VNCPort", 5900));
                newConnection.Add(new XElement("VNCAuthentication", 0));
                newConnection.Add(new XElement("VNCEncoding", 3));
                newConnection.Add(new XElement("VNCCopyRect", "false"));
                newConnection.Add(new XElement("VNCSharedConnection", "false"));
                newConnection.Add(new XElement("VNCViewOnly", "false"));
                newConnection.Add(new XElement("VNCCursor", 0));
                newConnection.Add(new XElement("VNCCursorShape", 2));
                newConnection.Add(new XElement("VNCAutoSizeMode", 0));
                newConnection.Add(new XElement("VNCCustomCompression", "false"));
                newConnection.Add(new XElement("VNCCustomCompressionValue", 1));
                newConnection.Add(new XElement("VNCJPEGCompression", "false"));
                newConnection.Add(new XElement("VNCJPEGCompressionValue", 0));
                newConnection.Add(new XElement("VNCEnable8BitColor", "false"));
                newConnection.Add(new XElement("VNCEmulate3Button", "false"));
                newConnection.Add(new XElement("VNCSwapButtons", "false"));
                newConnection.Add(new XElement("VNCClipboard", "true"));
                newConnection.Add(new XElement("VNCProxyType", 0));
                newConnection.Add(new XElement("VNCProxyPort", 0));
                newConnection.Add(new XElement("VNCProxyIP", ""));
                newConnection.Add(new XElement("VNCProxyCredId", 0));
                newConnection.Add(new XElement("VNCDSMKeyStorage", 0));
                newConnection.Add(new XElement("VNCDSMKeyPath", ""));
                newConnection.Add(new XElement("VNCDSMKeyHex", ""));
                newConnection.Add(new XElement("SSHPuTTYSession", ""));
                newConnection.Add(new XElement("SSHPort", 22));
                newConnection.Add(new XElement("SSHCompression", "false"));
                newConnection.Add(new XElement("TelnetPuTTYSession", ""));
                newConnection.Add(new XElement("TelnetPort", 23));
                newConnection.Add(new XElement("ExtAppId", 0));
                newConnection.Add(new XElement("ExtAppPort", 0));
                connections.Add(newConnection);
            }
            else
            {
                //We'll assume Linux here (SSH)
                XElement newConnection = new XElement("Connection");
                newConnection.Add(new XAttribute("Id", currentConnectionFolderIdCount));
                newConnection.Add(new XElement("LastUpdate", "-8587767832102012914"));
                newConnection.Add(new XElement("LastUpdateBy", "TOOL"));
                newConnection.Add(new XElement("ConnectionName", serverName));
                newConnection.Add(new XElement("ServerName", serverIp));
                newConnection.Add(new XElement("MACAddress", ""));
                newConnection.Add(new XElement("MgmtBoardUrl", ""));
                newConnection.Add(new XElement("Description", serverRole));
                newConnection.Add(new XElement("IconId", "00000000-0000-0000-0000-000000000000"));
                newConnection.Add(new XElement("PreExtAppId", 0));
                newConnection.Add(new XElement("PostExtAppId", 0));
                newConnection.Add(new XElement("Private", "true"));
                newConnection.Add(new XElement("OwnerName", owner));
                newConnection.Add(new XElement("ConnectionPlace", 2));
                newConnection.Add(new XElement("AutoSize", "true"));
                newConnection.Add(new XElement("SeparateResolutionX", 1024));
                newConnection.Add(new XElement("SeparateResolutionY", 768));
                newConnection.Add(new XElement("TabResolutionX", 1024));
                newConnection.Add(new XElement("TabResolutionY", 768));
                newConnection.Add(new XElement("DefaultProtocol", 3));
                newConnection.Add(new XElement("Port", 3389));
                newConnection.Add(new XElement("Console", "false"));
                newConnection.Add(new XElement("ClipBoard", "true"));
                newConnection.Add(new XElement("Printer", "false"));
                newConnection.Add(new XElement("Serial", "false"));
                newConnection.Add(new XElement("LocalDrives", "false"));
                newConnection.Add(new XElement("SmartCard", "false"));
                newConnection.Add(new XElement("EnableNLA", "false"));
                newConnection.Add(new XElement("DesktopBackground", "true"));
                newConnection.Add(new XElement("WindowDrag", "true"));
                newConnection.Add(new XElement("Themes", "true"));
                newConnection.Add(new XElement("BitmapCaching", "true"));
                newConnection.Add(new XElement("SmartSizing", "false"));
                newConnection.Add(new XElement("FontSmoothing", "false"));
                newConnection.Add(new XElement("RemoteAudio", 0));
                newConnection.Add(new XElement("KeyboardMode", 2));
                newConnection.Add(new XElement("RDPColorDepth", 16));
                newConnection.Add(new XElement("TSGUsageMethod", 2));
                newConnection.Add(new XElement("TSGServerName", ""));
                newConnection.Add(new XElement("TSGBypass", "false"));
                newConnection.Add(new XElement("TSGCredId", 0));
                newConnection.Add(new XElement("AuthenticationLevel", 0));
                newConnection.Add(new XElement("UseStartProgram", "false"));
                newConnection.Add(new XElement("StartProgram", ""));
                newConnection.Add(new XElement("WorkDir", ""));
                newConnection.Add(new XElement("ICAPort", 1494));
                newConnection.Add(new XElement("ICAEncryption", 0));
                newConnection.Add(new XElement("ICALocalDrives", "false"));
                newConnection.Add(new XElement("ICAPrinter", "false"));
                newConnection.Add(new XElement("ICALPTPort", "false"));
                newConnection.Add(new XElement("ICACOMPort", "false"));
                newConnection.Add(new XElement("ICACompression", "false"));
                newConnection.Add(new XElement("ICABitmapCaching", "false"));
                newConnection.Add(new XElement("ICAQueue", "false"));
                newConnection.Add(new XElement("ICASessionReliability", "false"));
                newConnection.Add(new XElement("ICARemoteAudio", 0));
                newConnection.Add(new XElement("ICAColorDepth", 2));
                newConnection.Add(new XElement("VNCPort", 5900));
                newConnection.Add(new XElement("VNCAuthentication", 0));
                newConnection.Add(new XElement("VNCEncoding", 3));
                newConnection.Add(new XElement("VNCCopyRect", "false"));
                newConnection.Add(new XElement("VNCSharedConnection", "false"));
                newConnection.Add(new XElement("VNCViewOnly", "false"));
                newConnection.Add(new XElement("VNCCursor", 0));
                newConnection.Add(new XElement("VNCCursorShape", 2));
                newConnection.Add(new XElement("VNCAutoSizeMode", 0));
                newConnection.Add(new XElement("VNCCustomCompression", "false"));
                newConnection.Add(new XElement("VNCCustomCompressionValue", 1));
                newConnection.Add(new XElement("VNCJPEGCompression", "false"));
                newConnection.Add(new XElement("VNCJPEGCompressionValue", 0));
                newConnection.Add(new XElement("VNCEnable8BitColor", "false"));
                newConnection.Add(new XElement("VNCEmulate3Button", "false"));
                newConnection.Add(new XElement("VNCSwapButtons", "false"));
                newConnection.Add(new XElement("VNCClipboard", "true"));
                newConnection.Add(new XElement("VNCProxyType", 0));
                newConnection.Add(new XElement("VNCProxyPort", 0));
                newConnection.Add(new XElement("VNCProxyIP", ""));
                newConnection.Add(new XElement("VNCProxyCredId", 0));
                newConnection.Add(new XElement("VNCDSMKeyStorage", 0));
                newConnection.Add(new XElement("VNCDSMKeyPath", ""));
                newConnection.Add(new XElement("VNCDSMKeyHex", ""));
                newConnection.Add(new XElement("SSHPuTTYSession", ""));
                newConnection.Add(new XElement("SSHPort", 22));
                newConnection.Add(new XElement("SSHCompression", "false"));
                newConnection.Add(new XElement("TelnetPuTTYSession", ""));
                newConnection.Add(new XElement("TelnetPort", 23));
                newConnection.Add(new XElement("ExtAppId", 0));
                newConnection.Add(new XElement("ExtAppPort", 0));
                connections.Add(newConnection);

            }
            addCredPlace(owner, currentConnectionFolderIdCount.ToString(), "0", "true");
            addIndexTableItem(currentConnectionFolderIdCount.ToString(), "Connection", parentFolderId);
            
            currentConnectionFolderIdCount++;
        }
        private void addFolder(String folderName)
        {

            XElement newFolder = new XElement("Folder");
            newFolder.Add(new XAttribute("Id", currentConnectionFolderIdCount));
            newFolder.Add(new XElement("LastUpdate", "-8587767833400112711"));
            newFolder.Add(new XElement("LastUpdateBy", "User"));
            newFolder.Add(new XElement("Name", folderName));
            newFolder.Add(new XElement("Description", ""));
            newFolder.Add(new XElement("Sorted", "true"));
            folders.Add(newFolder);

            currentConnectionFolderIdCount++;
        }
        private void addCredFolder(String credFolderName)
        {

            XElement newCredFolder = new XElement("CredFolder");
            newCredFolder.Add(new XAttribute("Id", currentCredFolderIdCount));
            newCredFolder.Add(new XElement("LastUpdate", "-8587767833400112711"));
            newCredFolder.Add(new XElement("LastUpdateBy", "User"));
            newCredFolder.Add(new XElement("Name", credFolderName));
            newCredFolder.Add(new XElement("Description", ""));
            newCredFolder.Add(new XElement("Sorted", "true"));
            credFolders.Add(newCredFolder);

            currentCredFolderIdCount++;
        }
        private void addCredPlace(String user, String hostId, String credId, String inheritCred)
        {
            XElement newCredPlace = new XElement("CredPlace");
            newCredPlace.Add(new XAttribute("User", user));
            newCredPlace.Add(new XAttribute("HostId", hostId));
            newCredPlace.Add(new XAttribute("CredId", credId));
            newCredPlace.Add(new XAttribute("InheritCred", inheritCred));
            credPlaces.Add(newCredPlace);
        }
        private void addIndexTableItem(String id, String type, String parentItemId)
        {
            //get order num
            int orderNum = 1;

            XElement newItem = new XElement("Item");
            newItem.Add(new XAttribute("Id", id));
            newItem.Add(new XAttribute("Type", type));
            newItem.Add(new XAttribute("ParentItemId", parentItemId));
            newItem.Add(new XAttribute("OrderNum", orderNum));
            indexTable.Add(newItem);
        }

        private void addAppFolder(String appFolderName)
        {
            XElement newappFolder = new XElement("AppFolder");
            newappFolder.Add(new XAttribute("Id", currentAppFolderIdCount));
            newappFolder.Add(new XElement("LastUpdate", "-8587767833400102710"));
            newappFolder.Add(new XElement("LastUpdateBy", "User"));
            newappFolder.Add(new XElement("Name", appFolderName));
            newappFolder.Add(new XElement("Description", ""));
            newappFolder.Add(new XElement("Sorted", "true"));
            appFolders.Add(newappFolder);

            currentAppFolderIdCount++;
        }

    }
}
