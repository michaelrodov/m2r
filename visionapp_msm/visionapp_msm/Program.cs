using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.DirectoryServices.ActiveDirectory;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;

namespace visionapp_msm
{
    class Program
    {


        static void Main(string[] args)
        {
            String owner = Environment.UserName + "@" + System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().Name;
            /*
            String installDir = "";
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\visionapp\\vRD2010");
            if (key != null)
            {
                Object o = key.GetValue("InstallDir");
                if (o != null)
                {
                    installDir = o.ToString();
                }
            }
            String vRD_path = installDir + "vRD.exe";
            String connectionsFilePath = Environment.GetEnvironmentVariable("APPDATA") + "\visionapp\vRD 2010\settings.xml";
            */

            
            String[][] servers = new String[7][];
            String[][] products = new String[4][];
            String[][] datacenters = new String [2][];
            String[][] farms = new String[2][];

            getServers(ref servers);
            getProducts(ref products);
            getDatacenters(ref datacenters);
            getFarms(ref farms);

            

            Console.WriteLine("Back-filling product, datacenter, and farm names...");
            backFillValuesToServersArray(ref servers, ref products, ref datacenters, ref farms);


            //Sort servers array (important for quick adding to the connections file!)
            // One other thing to point out is that the sort should be done
            // "just BEFORE" adding the values to the visionapp connections file
            // and AFTER any changes to the servers array.
            Console.WriteLine("\rSorting servers array (quickSort)...");
            quickSort(ref servers, 0, servers[0].Length - 1);

            //Add to the visionapp connection file...
            Console.WriteLine("Adding to visionapp connections file...");
            visionappConnectionsFile vConnFile = new visionappConnectionsFile(owner);

            for (int i = 0; i < servers[0].Length; i++ )
            {
                String serverProduct = servers[0][i];
                String serverName = servers[1][i];
                String serverIp = servers[2][i];
                String serverDatacenter = servers[3][i];
                String serverFarm = servers[4][i];
                String serverRole = servers[5][i];
                String serverOperatingSystem = servers[6][i];


                //IMPORTANT! Expects the servers to be in sorted order!
                vConnFile.addServer(serverName,
                   serverIp,
                   serverRole,
                   serverOperatingSystem,
                   serverProduct,
                   serverDatacenter,
                   serverFarm);

                Console.Write("\r" + i + "/" + servers[0].Length + " (" + ((double)((i * 100) / servers[0].Length)).ToString("#.####") + "%)");
            }


                Console.WriteLine("\rExporting to settings.xml file...");

            //DEBUG!
            String connectionsFilePath = "settings.xml";

            //Write to connectionsFilePath
            XElement xml = vConnFile.exportConnectionsFile();
            System.IO.File.WriteAllText(@connectionsFilePath, xml.ToString());


            Console.WriteLine("Finished!");

        }

        //Translates the product, datacenter, and farm values into what we want the user to
        // see... then backfills these "display" versions into the servers array.
        static void backFillValuesToServersArray(ref String[][] servers,
            ref String[][] products,
            ref String[][] datacenters,
            ref String[][] farms)
        {
            for (int i = 0; i < servers[0].Length; i++)
            {
                String serverProduct = servers[0][i];
                String serverName = servers[1][i];
                String serverIp = servers[2][i];
                String serverDatacenter = servers[3][i];
                String serverFarm = servers[4][i];
                String serverRole = servers[5][i];
                String serverOperatingSystem = servers[6][i];

                String theProduct = "";
                String theDatacenter = "";
                String theFarm = "";

                for (int j = 0; j < products[0].Length; j++)
                {
                    String productDisplayName = products[0][j];
                    String productFullName = products[1][j];
                    String productId = products[2][j];
                    String productName = products[3][j];
                    if (serverProduct == productDisplayName || serverProduct == productName)
                    {
                        theProduct = productDisplayName + " : " + productFullName.Replace("&", "");
                        break;
                    }
                }
                if (theProduct == "")
                    theProduct = serverProduct;

                for (int j = 0; j < datacenters[0].Length; j++)
                {
                    String datacenterId = datacenters[0][j];
                    String datacenterName = datacenters[1][j];
                    if (serverDatacenter == datacenterId)
                    {
                        theDatacenter = datacenterName.Replace("&", "");
                        break;
                    }
                }
                if (theDatacenter == "")
                    theDatacenter = "(empty datacenter)";

                for (int j = 0; j < farms[0].Length; j++)
                {
                    String farmName = farms[0][j];
                    String farmDescription = farms[1][j];
                    if (serverFarm == farmName)
                    {
                        theFarm = farmName.Replace("&", "") + " : " + farmDescription.Replace("&", "");
                        break;
                    }
                }
                if (theFarm == "")
                    theFarm = "(empty farm)";


                //Push values back into the servers array...
                servers[0][i] = theProduct;
                servers[3][i] = theDatacenter;
                servers[4][i] = theFarm;

                Console.Write("\r" + i + "/" + servers[0].Length + " (" + ((double)((i * 100) / servers[0].Length)).ToString("#.####") + "%)");

            }
        }

        /******************* Functions to grab info from MSM (REST) *********************************/
        static void getServers(ref String[][] array)
        {
            String URL = "https://msmadmin.saas.hp.com/Rest2/Server/All/";
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/xml");
            webClient.Headers.Add("Authorization", "Basic ZmFybS5tbmcuYXBpLnVzZXJAaHAuY29tOldlbGNvbWVBUEkx");

            try
            {
                Console.WriteLine("Querying MSM for servers...");
                Stream stream = webClient.OpenRead(URL);
                StreamReader reader = new StreamReader(stream);
                String request = reader.ReadToEnd();
                reader.Close();
                stream.Close();

                Console.WriteLine("Parsing servers XML...");
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(request);
                XmlNodeList servers = xml.DocumentElement.SelectNodes("/Response/Servers/Server");

                //DEBUG! PPM only!
                //XmlNodeList servers = xml.DocumentElement.SelectNodes("/Response/Servers/Server[productID='4']");

                int i = 0;
                array[0] = new String[servers.Count];
                array[1] = new String[servers.Count];
                array[2] = new String[servers.Count];
                array[3] = new String[servers.Count];
                array[4] = new String[servers.Count];
                array[5] = new String[servers.Count];
                array[6] = new String[servers.Count];


                foreach (XmlNode server in servers)
                {
                    String serverProduct;
                    String serverName;
                    String serverIp;
                    String serverDatacenter;
                    String serverFarm;
                    String serverRole;
                    String serverOperatingSystem;


                    try
                    {
                        serverProduct = server.SelectSingleNode("productName").InnerText;
                        if (serverProduct == "")
                            serverProduct = "EMPTY";
                        serverName = server.SelectSingleNode("name").InnerText;
                         serverIp = server.SelectSingleNode("managementIP").InnerText;
                        if (serverIp == "")
                            serverIp = serverName;
                        serverDatacenter = server.SelectSingleNode("dataCenterId").InnerText;
                        if (serverDatacenter == "")
                            serverDatacenter = "EMPTY";
                        serverFarm = server.SelectSingleNode("farmName").InnerText;
                        if (serverFarm == "")
                            serverFarm = "EMPTY";
                        serverRole = server.SelectSingleNode("serverRole").InnerText;
                        try
                        {
                            //Apparently, it's possible to NOT have an "operatingSystem" attribute...
                            // so we have to check for this...
                            serverOperatingSystem = server.SelectSingleNode("operatingSystem").InnerText;
                        }
                        catch
                        {
                            serverOperatingSystem = "";
                        }
                    }
                    catch
                    {
                        serverProduct = server.SelectSingleNode("serverRole").InnerText;
                        if (serverProduct == "")
                            serverProduct = "EMPTY";
                        serverName = server.SelectSingleNode("name").InnerText;
                        serverIp = server.SelectSingleNode("managementIP").InnerText;
                        if (serverIp == "")
                            serverIp = serverName;
                        serverDatacenter = server.SelectSingleNode("dataCenterId").InnerText;
                        if (serverDatacenter == "")
                            serverDatacenter = "EMPTY";
                        serverFarm = server.SelectSingleNode("farmName").InnerText;
                        if (serverFarm == "")
                            serverFarm = "EMPTY";
                        serverRole = server.SelectSingleNode("serverRole").InnerText;
                        try
                        {
                            //Apparently, it's possible to NOT have an "operatingSystem" attribute...
                            // so we have to check for this...
                            serverOperatingSystem = server.SelectSingleNode("operatingSystem").InnerText;
                        }
                        catch
                        {
                            serverOperatingSystem = "";
                        }
                    }

                    array[0][i] = serverProduct;
                    array[1][i] = serverName;
                    array[2][i] = serverIp;
                    array[3][i] = serverDatacenter;
                    array[4][i] = serverFarm;
                    array[5][i] = serverRole;
                    array[6][i] = serverOperatingSystem;

                   
                    i++;
                    Console.Write("\r" + i + "/" + servers.Count + " (" + ((double)((i * 100) / servers.Count)).ToString("#.####") + "%)");
                }
                Console.Write("\r");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        static void getProducts(ref String[][] array)
        {
            String URL = "https://msmadmin.saas.hp.com/Rest2/Product/All/";
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/xml");
            webClient.Headers.Add("Authorization", "Basic ZmFybS5tbmcuYXBpLnVzZXJAaHAuY29tOldlbGNvbWVBUEkx");

            try
            {
                Console.WriteLine("Querying MSM for products...");
                Stream stream = webClient.OpenRead(URL);
                StreamReader reader = new StreamReader(stream);
                String request = reader.ReadToEnd();
                reader.Close();
                stream.Close();

                Console.WriteLine("Parsing products XML...");
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(request);
                XmlNodeList products = xml.DocumentElement.SelectNodes("/Response/Products/Product");

                int i = 0;
                array[0] = new String[products.Count];
                array[1] = new String[products.Count];
                array[2] = new String[products.Count];
                array[3] = new String[products.Count];


                foreach (XmlNode product in products)
                {
                    String productDisplayName;
                    String productFullName;
                    String productId;
                    String productName;

                     productDisplayName = product.SelectSingleNode("displayName").InnerText;
                        productFullName = product.SelectSingleNode("fullName").InnerText;
                        productId = product.SelectSingleNode("ID").InnerText;
                        productName = product.SelectSingleNode("name").InnerText;

                        array[0][i] = productDisplayName;
                        array[1][i] = productFullName;
                        array[2][i] = productId;
                        array[3][i] = productName;

                        i++;
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        static void getDatacenters(ref String[][] array)
        {
            String URL = "https://msmadmin.saas.hp.com/Rest2/DataCenter/All/";
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/xml");
            webClient.Headers.Add("Authorization", "Basic ZmFybS5tbmcuYXBpLnVzZXJAaHAuY29tOldlbGNvbWVBUEkx");

            try
            {
                Console.WriteLine("Querying MSM for datacenters...");
                Stream stream = webClient.OpenRead(URL);
                StreamReader reader = new StreamReader(stream);
                String request = reader.ReadToEnd();
                reader.Close();
                stream.Close();

                Console.WriteLine("Parsing datacenters XML...");
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(request);
                XmlNodeList datacenters = xml.DocumentElement.SelectNodes("/Response/DataCenters/DataCenter");

                int i = 0;
                array[0] = new String[datacenters.Count];
                array[1] = new String[datacenters.Count];


                foreach (XmlNode datacenter in datacenters)
                {
                    String datacenterId;
                    String datacenterName;

                    datacenterId = datacenter.SelectSingleNode("invID").InnerText;
                    datacenterName = datacenter.SelectSingleNode("name").InnerText;

                    array[0][i] = datacenterId;
                    array[1][i] = datacenterName;

                    i++;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        static void getFarms(ref String[][] array)
        {
            String URL = "https://msmadmin.saas.hp.com/Rest2/Farm/All/";
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/xml");
            webClient.Headers.Add("Authorization", "Basic ZmFybS5tbmcuYXBpLnVzZXJAaHAuY29tOldlbGNvbWVBUEkx");

            try
            {
                Console.WriteLine("Querying MSM for farms...");
                Stream stream = webClient.OpenRead(URL);
                StreamReader reader = new StreamReader(stream);
                String request = reader.ReadToEnd();
                reader.Close();
                stream.Close();

                Console.WriteLine("Parsing farms XML...");
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(request);
                XmlNodeList farms = xml.DocumentElement.SelectNodes("/Response/Farms/Farm");

                int i = 0;
                array[0] = new String[farms.Count];
                array[1] = new String[farms.Count];


                foreach (XmlNode farm in farms)
                {
                    String farmName;
                    String farmDescription;

                    farmName = farm.SelectSingleNode("farmName").InnerText;
                    farmDescription = farm.SelectSingleNode("description").InnerText;

                    array[0][i] = farmName;
                    array[1][i] = farmDescription;

                    i++;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        /******************** Sort functions for the "servers" array ************************************/
        // Quicksort: http://en.wikipedia.org/wiki/Quicksort
        private static void quickSort(ref String[][] array, int lo, int hi)
        {
            if (lo < hi)
            {
                int p = partition(ref array, lo, hi);
                quickSort(ref array, lo, p - 1);
                quickSort(ref array, p + 1, hi);
            }
        }

        private static int partition(ref String[][] array, int lo, int hi)
        {
            int pivotIndex = (hi - lo) / 2 + lo;

            String[] pivotValue = new String[7];
            for (int i = 0; i < 7; i++)
                pivotValue[i] = array[i][pivotIndex];

            swap(ref array, pivotIndex, hi);

            int storeIndex = lo;

            for (int i = lo; i <= hi - 1; i++)
            {
                String string1 = "";
                String string2 = "";

                // Let's compare (in order):
                //      product(0), datacenter(3), farm(4), serverName(1)
                string1 += array[0][i];
                string1 += array[3][i];
                string1 += array[4][i];
                string1 += array[1][i];
                string2 += pivotValue[0];
                string2 += pivotValue[3];
                string2 += pivotValue[4];
                string2 += pivotValue[1];

                if (String.Compare(string1, string2) < 0)
                {
                    swap(ref array, i, storeIndex);
                    storeIndex++;
                }
            }
            swap(ref array, storeIndex, hi);

            return storeIndex;
        }

        private static void swap(ref String[][] array, int index1, int index2)
        {
            String[] temp = new String[7];

            // temp = A[index1]
            for (int i = 0; i < 7; i++)
                temp[i] = array[i][index1];

            // A[index1] = A[index2]
            for (int i = 0; i < 7; i++)
                array[i][index1] = array[i][index2];

            // A[index2] = temp
            for (int i = 0; i < 7; i++)
                array[i][index2] = temp[i];
        }


    }
}
