using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Net.NetworkInformation;
using System.Net;

namespace NetworkCommon
{
    public class IpHelper
    {
        /// <summary>
        /// 设置IP地址, 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="submask"></param>
        /// <param name="getway"></param>
        /// <param name="dns"></param>
        private static bool SetIPAddress(string[] ip, string[] submask, string[] getway, string[] dns, string macAddress)
        {
            ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = wmi.GetInstances();
            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;
            string str = "";
            foreach (ManagementObject mo in moc)
            {

                if (!(bool)mo["IPEnabled"])
                    continue;


                if (mo["MacAddress"].ToString().Replace(':','-') == macAddress)
                {
                    if (ip != null && submask != null)
                    {
                        string caption = mo["Caption"].ToString(); //描述
                        inPar = mo.GetMethodParameters("EnableStatic");
                        inPar["IPAddress"] = ip;
                        inPar["SubnetMask"] = submask;
                        outPar = mo.InvokeMethod("EnableStatic", inPar, null);
                        str = outPar["returnvalue"].ToString();
                        return (str == "0" || str == "1") ? true : false;
                        //获取操作设置IP的返回值， 可根据返回值去确认IP是否设置成功。 0或1表示成功 
                        // 返回值说明网址： https://msdn.microsoft.com/en-us/library/aa393301(v=vs.85).aspx
                    }
                    if (getway != null)
                    {
                        inPar = mo.GetMethodParameters("SetGateways");
                        inPar["DefaultIPGateway"] = getway;
                        outPar = mo.InvokeMethod("SetGateways", inPar, null);
                        str = outPar["returnvalue"].ToString();
                        return (str == "0" || str == "1") ? true : false;
                    }
                    if (dns != null)
                    {
                        inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                        inPar["DNSServerSearchOrder"] = dns;
                        outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                        str = outPar["returnvalue"].ToString();
                        return (str == "0" || str == "1") ? true : false;
                    }

                }
            }
            return false;
        }

        /// <summary>
        /// 设置IP
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="submask"></param>
        /// <param name="macAddress"></param>
        /// <returns></returns>
        public  static bool SetIPAddress(string[] ip, string[] submask, string macAddress)
        {
            return SetIPAddress(ip, submask, null, null, macAddress);
        }

        /// <summary>
        /// 设置网关
        /// </summary>
        /// <param name="getway"></param>
        /// <param name="macAddress"></param>
        /// <returns></returns>
        public static bool SetGetway(string[] getway, string macAddress)
        {
            return SetIPAddress(null, null, getway, null, macAddress);
        }

        /// <summary>
        /// 设置DNS
        /// </summary>
        /// <param name="dns"></param>
        /// <param name="macAddress"></param>
        /// <returns></returns>
        public static bool SetDNS(string[] dns, string macAddress)
        {
            return SetIPAddress(null, null, null, dns, macAddress);
        }


        // <summary>
        /// 启用DHCP服务
        /// </summary>
        public static void EnableDHCP(string macAddress)
        {

            ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = wmi.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (!(bool)mo["IPEnabled"])
                    continue;

                if (mo["MacAddress"].ToString().Replace(':', '-') == macAddress) // 
                {
                    mo.InvokeMethod("SetDNSServerSearchOrder", null);
                    mo.InvokeMethod("EnableDHCP", null);

                }
            }
        }

        /// <summary>
        /// 启用所有适配器
        /// </summary>
        /// <returns></returns>
        public static void EnableAllAdapters()
        {
            // ManagementClass wmi = new ManagementClass("Win32_NetworkAdapter");
            // ManagementObjectCollection moc = wmi.GetInstances();
            System.Management.ManagementObjectSearcher moc = new System.Management.ManagementObjectSearcher("Select * from Win32_NetworkAdapter where NetEnabled!=null ");
            foreach (System.Management.ManagementObject mo in moc.Get())
            {
                //if (!(bool)mo["NetEnabled"])
                //    continue;
                string capation = mo["Caption"].ToString();
                string descrption = mo["Description"].ToString();
                mo.InvokeMethod("Enable", null);
            }

        }

        /// <summary>
        /// 禁用所有适配器
        /// </summary>
        public static void DisableAllAdapters()
        {
            // ManagementClass wmi = new ManagementClass("Win32_NetworkAdapter");
            // ManagementObjectCollection moc = wmi.GetInstances();
            System.Management.ManagementObjectSearcher moc = new System.Management.ManagementObjectSearcher("Select * from Win32_NetworkAdapter where NetEnabled!=null ");
            foreach (System.Management.ManagementObject mo in moc.Get())
            {
                //if ((bool)mo["NetEnabled"])
                //    continue;
                string capation = mo["Caption"].ToString();
                string descrption = mo["Description"].ToString();
                mo.InvokeMethod("Disable", null);
            }

        }

        public static bool Ping(string ip)
        {
            bool online = false;
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(ip);
                if (pingReply.Status == IPStatus.Success)
                {
                    online = true;
                }
            }
            catch { }

            return online;
        }

        public static List<string> GetIP()
        {
            List<string> iplist = new List<string>();
            string hostName = Dns.GetHostName();//本机名   
            //System.Net.IPAddress[] addressList = Dns.GetHostByName(hostName).AddressList;//会警告GetHostByName()已过期，我运行时且只返回了一个IPv4的地址   
            System.Net.IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6   
            foreach (IPAddress ip in addressList)
            {
                iplist.Add(ip.ToString());
            }
            return iplist;
        }

        /// <summary>
        /// 获取指定的网络类型的IP
        /// </summary>
        /// <param name="netType"></param>
        /// <returns></returns>
        public static List<string> GetIP2(NetworkInterfaceType netType)
        {
            List<string> iplist = new List<string>();

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.NetworkInterfaceType == netType)
                {
                    //adapter.Name;     //网卡适配名称：“本地连接”
                    //adapter.Description;   //适配器描述信息
                    IPInterfaceProperties ip = adapter.GetIPProperties();     //IP配置信息
                    if (ip.UnicastAddresses.Count > 0)
                    {
                        iplist.Add(ip.UnicastAddresses[1].Address.ToString());   //IP地址
                        // ip.UnicastAddresses[0].IPv4Mask.ToString();  //子网掩码
                    }
                    //if (ip.GatewayAddresses.Count > 0)
                    //    ip.GatewayAddresses[0].Address.ToString();   //默认网关
                    //if (ip.DnsAddresses.Count > 0)
                    //{
                    //    ip.DnsAddresses[0].ToString();       //首选DNS服务器地址
                    //    if (ip.DnsAddresses.Count > 1)
                    //        ip.DnsAddresses[1].ToString();  //备用DNS服务器地址
                    //}
                }
            }

            return iplist;
        }

    }


}
