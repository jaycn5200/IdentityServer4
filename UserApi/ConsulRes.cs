using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Configuration;

namespace UserApi
{
    public static class ConsulRes
    {
        public static void RegistConsul(this IConfiguration configuration)
        {
            #region 注册consul
            string ip = "Localhost";
            //部署到不同服务器的时候不能写成127.0.0.1或者0.0.0.0,因为这是让服务消费者调用的地址
            //int port = int.Parse(configuration["Consul:ServicePort"]);//服务端口
            int port = 44393;
            ConsulClient client = new ConsulClient(obj =>
            {
                obj.Address = new Uri("http://127.0.0.1:8500");
                obj.Datacenter = "dc1";
            });
            //向consul注册服务
            Task<WriteResult> result = client.Agent.ServiceRegister(new AgentServiceRegistration()
            {
                ID = "servicename:44393",//服务编号，不能重复
                Name = "servicename",//服务的名字--将来调用时用的就是这个
                Address = ip,
                Port = port,
                Tags = new string[] { },//可以用来设置权重
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务停止多久后反注册
                    Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔，或者称为心跳间隔
                    HTTP = $"https://localhost:44393/api/Health",//健康检查地址,
                    Timeout = TimeSpan.FromSeconds(5)
                }
            });
            #endregion
        }
    }
}
