using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Emulator
{
    class configip
    {
        static String physicalAddress(NetworkInterface adapter)
        {
            String physicalAddress = adapter.GetPhysicalAddress().ToString();
            if(physicalAddress.Length > 0)
            {
                int length = physicalAddress.Length;
                for(int i = 2; i < physicalAddress.Length; i = i+3)
                {
                    physicalAddress = physicalAddress.Substring(0, i) + "-" + physicalAddress.Substring(i);
                }
            }
            return physicalAddress;
        }
        static IPAddress DefaultGatewayAddress(IPInterfaceProperties properties)
        {
            GatewayIPAddressInformationCollection addresses = properties.GatewayAddresses;
            IPAddress result = null;
            if(addresses.Any())
            {
                result = addresses.FirstOrDefault().Address;
            }
            return result;
        }
        static String[] IPAddresses(NetworkInterface adapter, IPInterfaceProperties properties)
        {
            UnicastIPAddressInformationCollection addresses = properties.UnicastAddresses;
            String[] result = new String[5];
            String empty = " ";
            result[0] = empty;
            result[1] = empty;
            result[2] = empty;
            result[3] = empty;
            result[4] = empty;

            if(addresses.Any())
            {
                if(addresses.Count == 1)
                {
                    result[0] = addresses[0].Address.ToString();
                    result[2] = addresses[0].IPv4Mask.ToString();
                }
                else
                {
                    result[0] = addresses[0].Address.ToString();
                    result[1] = addresses[1].Address.ToString();
                    result[2] = addresses[1].IPv4Mask.ToString();
                }
                if(properties.DhcpServerAddresses.Count > 0)
                {
                    result[3] = properties.DhcpServerAddresses.FirstOrDefault().ToString();
                }
                if(properties.DnsAddresses.Count > 0)
                {
                    result[4] = properties.DnsAddresses.FirstOrDefault().ToString();
                }
            }
            return result;
        }
        
        static void UserInfo()
        {
            Boolean IPRouting = false;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            string hostname = properties.HostName;
            string primaryDnsSuffix = properties.DomainName;
            string nodeType = Convert.ToString(properties.NodeType);
            bool winsProxyEnabled = properties.IsWinsProxy;

            if (properties.GetIPv6GlobalStatistics().NumberOfRoutes > 0)
            {
                IPRouting = true;
            }
            Console.WriteLine("Host Name . . . . . . . . . . . . : {0}", hostname);
            Console.WriteLine("Primary Dns Suffix . . . . . . .  :  {0}", primaryDnsSuffix);
            Console.WriteLine("Node Type . . . . . . . . . . . . : {0}", nodeType);
            Console.WriteLine("IP Routing Enabled . . . . . . .  : {0}, placeholder", IPRouting);
            Console.WriteLine("WINS Proxy Enabled . . . . . . .  :{0} \r\n", winsProxyEnabled);
        }
        IPGlobalProperties Properties = IPGlobalProperties.GetIPGlobalProperties();
        static void displayResultsNoArgs()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            Console.WriteLine("Windows IP Configuration" + "\r\n\n");
            foreach(NetworkInterface adapter in nics)
            {
                NetworkInterfaceType type = adapter.NetworkInterfaceType;
                if(type.ToString().Equals("Loopback") || type.ToString().Equals("Tunnel"))
                {
                    continue;
                }
                IPInterfaceProperties properties = adapter.GetIPProperties();
                Console.WriteLine("{0} adapter {1}:\r\n", type, adapter.Name);
                
                if(adapter.OperationalStatus == OperationalStatus.Up)
                {
                    String[] IPSet = IPAddresses(adapter, properties);
                    Console.WriteLine("Connection-specific DNS Suffix  . : {0}", properties.DnsSuffix);
                    Console.WriteLine("Link-local IPv6 Address . . . . . : {0}", IPSet[0]);
                    Console.WriteLine("IPv4 Address . . . . . . . . . .  : {0}", IPSet[1]);
                    Console.WriteLine("Subnet Mask . . . . . . . . . . . : {0}", IPSet[2]);
                    Console.WriteLine("Default Gateway . . . . . . . . . : {0}", DefaultGatewayAddress(properties) + "\r\n");
                }
                else
                {
                    string status = "Media disconnected";
                    Console.WriteLine("   Media State . . . . . . . . . . . : {0}", status);
                }
            }
        }
        static void displayAllConfigInfo()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            Console.WriteLine("Windows IP Configuration" + "\r\n");
            UserInfo();

            foreach (NetworkInterface adapter in nics)
            {
                NetworkInterfaceType type = adapter.NetworkInterfaceType;
                if(type.ToString().Equals("Loopback"))
                {
                    continue;
                }
                IPInterfaceProperties properties = adapter.GetIPProperties();
                Boolean DHCPEnabled;
                Boolean AutoConfig;
                if (!adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    DHCPEnabled = false;
                    if(type.ToString().Equals("Tunnel"))
                    {
                        AutoConfig = true;
                    }
                    else
                    {
                        AutoConfig = false;
                    }
                }
                else
                {
                    DHCPEnabled = properties.GetIPv4Properties().IsDhcpEnabled;
                    AutoConfig = properties.GetIPv4Properties().IsAutomaticPrivateAddressingEnabled;
                }
                Console.WriteLine("{0} adapter {1}:\r\n", type, adapter.Name);

                if(adapter.OperationalStatus != OperationalStatus.Up)
                {
                    Console.WriteLine("Media State . . . . . . . . . . . : Media Disconnected");
                }
                Console.WriteLine("Connection-specific DNS Suffix  . : {0}", properties.DnsSuffix);
                Console.WriteLine("Description . . . . . . . . . . . : {0}", adapter.Description);
                Console.WriteLine("Physical Address. . . . . . . .  : {0}", physicalAddress(adapter));
                Console.WriteLine("DHCP Enabled . . . . . . . . . .  : {0}", DHCPEnabled);
                Console.WriteLine("Autoconfiguration Enabled . . . . : {0}", AutoConfig);

                if(adapter.OperationalStatus != OperationalStatus.Up)
                {
                    Console.WriteLine("");
                    continue;
                }
                String[] IPSet = IPAddresses(adapter, properties);
                Console.WriteLine("Link-local IPv6 Address. . . . .  : {0}<Preferred>", IPSet[0]);
                Console.WriteLine("IPv4 Address. . . . . . . . . . . : {0}<Preferred>", IPSet[1]);
                Console.WriteLine("Subnet Mask . . . . . . . . . . . : {0}", IPSet[2]);
                Console.WriteLine("Lease Obtained . . . . . . . . .  : {0} placeholder");
                Console.WriteLine("Default Gateway . . . . . . . . . : {0}", DefaultGatewayAddress(properties));
                Console.WriteLine("DHCP Server . . . . . . . . . . . : {0}", IPSet[3]);
                Console.WriteLine("DHCPv6 IAID . . . . . . . . . . . : {0}");
                Console.WriteLine("DHCPv6 Client DUID . . . . . . .  : {0}" + "\r\n");
                Console.WriteLine("DNS Servers  . . . . . . . . . .  : {0}", IPSet[4]);
                Console.WriteLine("NetBIOS over Tcpip . . . . . . .  : {0}" + "\r\n");
                }
        }
        static void displayUsageInfo()
        {
            Console.WriteLine("USAGE");
            Console.WriteLine("    ipconfig [/allcompartments] [/? :-h : help :/all] \r\n");
            Console.WriteLine("where");
            Console.WriteLine("   adapter           Connection name \r\n");
            Console.WriteLine("    Options:");
            Console.WriteLine("    nothing       Display basic configuration information");
            Console.WriteLine("    /all          Display full configuration information");
            Console.WriteLine("    ?, -h, help   Display this help message");
        }
        static string formatPhysicalAddress(string input)
        {
            try
            {
                return Regex.Replace(input,
                   @"^(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})$", "$1-$2-$3-$4-$5-$6", RegexOptions.None,
                   TimeSpan.FromMilliseconds(150));

            }
            catch (RegexMatchTimeoutException)
            {
                return input;
            }

        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                //displayResultsNoArgs();
                displayAllConfigInfo();
                return;
            }
            else if (args.Length > 1)
            {
                // display usage information
                Console.WriteLine("Invalid parameters");
                displayUsageInfo();
                return;
            }
            else
            {
                // here implement all configuration info
                String clp = args[0].ToLower();
                switch (clp)
                {
                    case "/all":
                        displayAllConfigInfo();
                        break;
                    case "?":
                    case "-h":
                    case "help":
                        displayUsageInfo();
                        break;
                    default:
                        Console.Write("Invaild Parameter");
                        displayUsageInfo();
                        break;
                }
            }
        }
    }
}
