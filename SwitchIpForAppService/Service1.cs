using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.Threading;
using NetworkCommon;
using System.Net.NetworkInformation;

namespace SwitchIpForAppService
{
    public partial class Service1 : ServiceBase
    {
        string localIp;
        string serviceIp;
        string gatewayIp;
        string subnetMask;
        string macAddress;
        string[] apps;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                localIp = ConfigurationManager.AppSettings["LocalIp"];
                serviceIp = ConfigurationManager.AppSettings["ServiceIp"];
                gatewayIp = ConfigurationManager.AppSettings["Gateway"];
                subnetMask = ConfigurationManager.AppSettings["SubnetMask"];
                macAddress = ConfigurationManager.AppSettings["MacAddress"];
                apps = ConfigurationManager.AppSettings["AppList"].Split('|');

                Thread thread = new Thread(() =>
                {
                    // 修改唯一Ip的方式
                    //Dowork();
                    //添加虚拟Ip的方式 
                    Dowork2();
                });
                thread.IsBackground = true;
                thread.Start();

            }
            catch { }
        }

        protected override void OnStop()
        {
            StopApp(apps.ToList());
        }

        /// <summary>
        /// 这个逻辑是改唯一IP
        /// </summary>
        void Dowork()
        {
            bool firstStart = false;
            while (true)
            {
                try
                {
                    // ping 网关以检查自己是否掉线
                    bool isOnline = IpHelper.Ping(gatewayIp);
                    if (isOnline)
                    {
                        #region 在线
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
                        else if (IpHelper.GetIP2(NetworkInterfaceType.Ethernet).Contains(serviceIp) && IpHelper.Ping(serviceIp) && firstStart == false)
                        {
                            StopApp(apps.ToList());
                            StartApp(apps.ToList());
                            firstStart = true;
                        }

                        #endregion
                    }
                    else
                    {
                        #region 掉线
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

                        #endregion
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


        //这个逻辑是添加一个虚拟Ip
        void Dowork2()
        {
            bool firstStart = false;
            while (true)
            {
                try
                {
                    // ping 网关以检查自己是否掉线
                    bool isOnline = IpHelper.Ping(gatewayIp);
                    if (isOnline)
                    {
                        #region 在线
                        Console.WriteLine("在线...");
                        if (!IpHelper.GetIP2(NetworkInterfaceType.Ethernet).Contains(serviceIp) && IpHelper.Ping(serviceIp) == false)
                        {
                            if (IpHelper.SetIPAddress(new string[] { localIp, serviceIp }, new string[] { subnetMask }, macAddress))
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
                        else if (IpHelper.GetIP2(NetworkInterfaceType.Ethernet).Contains(serviceIp) && IpHelper.Ping(serviceIp) && firstStart == false)
                        {
                            StopApp(apps.ToList());
                            StartApp(apps.ToList());
                            firstStart = true;
                        }

                        #endregion
                    }
                    else
                    {
                        #region 掉线
                        Console.WriteLine("掉线...");
                        if (IpHelper.GetIP2(NetworkInterfaceType.Ethernet).Contains(serviceIp))
                        {
                            if (IpHelper.SetIPAddress(new string[] { localIp }, new string[] { subnetMask }, macAddress))
                            {
                                StopApp(apps.ToList());
                                Console.WriteLine("成功设置Ip:" + localIp);
                            }
                            else
                            {
                                Console.WriteLine("设置Ip:" + localIp + "失败，在无网线的情况下是正常现象");
                            }

                        }

                        #endregion
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
