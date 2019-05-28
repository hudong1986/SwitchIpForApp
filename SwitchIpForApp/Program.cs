using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net;
using System.Management;
using System.Configuration;
using NetworkCommon;
using System.Diagnostics;

namespace SwitchIpForApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string localIp = ConfigurationManager.AppSettings["LocalIp"];
                string serviceIp = ConfigurationManager.AppSettings["ServiceIp"];
                string gatewayIp = ConfigurationManager.AppSettings["Gateway"];
                string subnetMask = ConfigurationManager.AppSettings["SubnetMask"];
                string macAddress = ConfigurationManager.AppSettings["MacAddress"];
                string[] apps = ConfigurationManager.AppSettings["AppList"].Split('|');
                bool firstStart = false;
                while (true)
                {
                    try
                    {
                        // ping 网关以检查自己是否掉线
                        bool isOnline = IpHelper.Ping(gatewayIp);
                        if (isOnline)
                        {
                            Console.WriteLine("在线...");
                            if (!IpHelper.GetIP2(NetworkInterfaceType.Ethernet).Contains(serviceIp) && IpHelper.Ping(serviceIp) == false)
                            {
                                if (IpHelper.SetIPAddress(new string[] { serviceIp }, new string[] { subnetMask }, macAddress))
                                {
                                    DateTime dt1 = DateTime.Now;
                                    bool netstate = true; //修改后的网络状态
                                    while (IpHelper.Ping(gatewayIp) == false)
                                    {
                                        Console.WriteLine("IP初始化...");
                                        Thread.Sleep(1000);
                                        if (DateTime.Now.Subtract(dt1).TotalSeconds >= 10)
                                        {
                                            netstate = false;
                                            break;
                                        }
                                    }

                                    if (netstate)
                                    {
                                        // IpHelper.SetGetway(new string[] { gatewayIp }, macAddress);
                                        Thread.Sleep(2000);
                                        StopApp(apps.ToList());
                                        StartApp(apps.ToList());
                                        Console.WriteLine("成功设置Ip:" + serviceIp);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("设置Ip:" + serviceIp + "失败");
                                }
                            }
                            else if (IpHelper.GetIP2(NetworkInterfaceType.Ethernet).Contains(serviceIp) && IpHelper.Ping(serviceIp) && firstStart==false)
                            {
                                StopApp(apps.ToList());
                                StartApp(apps.ToList());
                                firstStart = true;
                            }
                        }
                        else
                        {
                            Console.WriteLine("掉线...");
                            if (!IpHelper.GetIP2(NetworkInterfaceType.Ethernet).Contains(localIp))
                            {
                                if (IpHelper.SetIPAddress(new string[] { localIp }, new string[] { subnetMask }, macAddress))
                                {
                                    DateTime dt1 = DateTime.Now;
                                    bool netstate = true; //修改后的网络状态
                                    while (IpHelper.Ping(gatewayIp) == false)
                                    {
                                        Console.WriteLine("IP初始化...");
                                        Thread.Sleep(1000);
                                        if (DateTime.Now.Subtract(dt1).TotalSeconds >= 10)
                                        {
                                            netstate = false;
                                            break;
                                        }
                                    }

                                    if (netstate)
                                    {
                                        // IpHelper.SetGetway(new string[] { gatewayIp }, macAddress);
                                        StopApp(apps.ToList());
                                        Console.WriteLine("成功设置Ip:" + localIp);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("设置Ip:" + localIp + "失败，在无网线的情况下是正常现象");
                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadKey();

        }


        static void StartApp(List<string> apps)
        {
            try
            {
                foreach (string path in apps)
                {
                    Process.Start(path);
                }
            }
            catch
            {

            }
        }

        static void StopApp(List<string> apps)
        {
            try
            {
                foreach (string path in apps)
                {
                    string appName = System.IO.Path.GetFileNameWithoutExtension(path);
                    Process[] process = Process.GetProcessesByName(appName);
                    foreach (Process pro in process)
                    {
                        pro.Kill();
                    }
                }
            }
            catch
            {

            }
        }



    }
}
