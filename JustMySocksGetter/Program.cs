﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace JustMySocksGetter
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Program
    {
        private class V2RayServer
        {
            public string ps { get; set; } = null!;
            public string port { get; set; } = null!;
            public string id { get; set; } = null!;
            public int aid { get; set; }
            public string net { get; set; } = null!;
            public string type { get; set; } = null!;
            public string tls { get; set; } = null!;
            public string add { get; set; } = null!;
        }

        private class YamlServer
        {
            [YamlMember(Alias = "name", ApplyNamingConventions = false)]
            public string Name { get; set; } = "Name";

            [YamlMember(Alias = "server", ApplyNamingConventions = false)]
            public string Server { get; set; } = "Server";

            [YamlMember(Alias = "port", ApplyNamingConventions = false)]
            public int Port { get; set; }

            [YamlMember(Alias = "type", ApplyNamingConventions = false)]
            public string Type { get; set; } = "Type";

            [YamlMember(Alias = "uuid", ApplyNamingConventions = false)]
            public string Uuid { get; set; } = "UUID";

            [YamlMember(Alias = "alterId", ApplyNamingConventions = false)]
            public int AlterId { get; set; }

            [YamlMember(Alias = "cipher", ApplyNamingConventions = false)]
            public string Cipher { get; set; } = "auto";

            [YamlMember(Alias = "tls", ApplyNamingConventions = false)]
            public bool Tls { get; set; }

            [YamlMember(Alias = "skip-cert-verify", ApplyNamingConventions = false)]
            public bool SkipCertVerify { get; set; }

            [YamlMember(Alias = "udp", ApplyNamingConventions = false)]
            public bool Udp { get; set; }
        }

        private class ProxyGroup
        {
            [YamlMember(Alias = "name", ApplyNamingConventions = false)]
            public string Name { get; set; } = "GroupName";

            [YamlMember(Alias = "type", ApplyNamingConventions = false)]
            public string Type { get; set; } = "select";

            [YamlMember(Alias = "url", ApplyNamingConventions = false, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
            public string? Url { get; set; }

            [YamlMember(Alias = "interval", ApplyNamingConventions = false, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
            public int Interval { get; set; }

            [YamlMember(Alias = "proxies", ApplyNamingConventions = false)]
            public List<string> Proxies { get; set; } = new List<string>();
        }

        private class YamlConfig
        {
            [YamlMember(Alias = "port", ApplyNamingConventions = false)]
            public int Port { get; set; }

            [YamlMember(Alias = "socks-port", ApplyNamingConventions = false)]
            public int SocksPort { get; set; }

            [YamlMember(Alias = "allow-lan", ApplyNamingConventions = false)]
            public bool AllowLan { get; set; }

            [YamlMember(Alias = "mode", ApplyNamingConventions = false)]
            public string Mode { get; set; } = "Rule";

            [YamlMember(Alias = "log-level", ApplyNamingConventions = false)]
            public string LogLevel { get; set; } = "info";

            [YamlMember(Alias = "external-controller", ApplyNamingConventions = false)]
            public string ExternalController { get; set; } = ":9090";

            [YamlMember(Alias = "proxies", ApplyNamingConventions = false)]
            public List<YamlServer> Proxies { get; set; } = new List<YamlServer>();

            [YamlMember(Alias = "proxy-groups", ApplyNamingConventions = false)]
            public List<ProxyGroup> ProxyGroups { get; set; } = new List<ProxyGroup>();

            [YamlMember(Alias = "rules", ApplyNamingConventions = false)]
            public List<string> Rules { get; set; } = new List<string>();
        }

        private class AppConfig
        {
            [YamlMember(Alias = "输入配置", ApplyNamingConventions = false)]
            public string SamplePath { get; set; } = "./Sample.yml";

            [YamlMember(Alias = "输出配置", ApplyNamingConventions = false)]
            public string OutputPath { get; set; } = "./Output.yml";

            [YamlMember(Alias = "订阅链接", ApplyNamingConventions = false)]
            public string SubscribeLink { get; set; } = "https://";

            [YamlMember(Alias = "显示服务器信息", ApplyNamingConventions = false)]
            public bool DisplayServerInfo { get; set; }

            [YamlMember(Alias = "自动关闭", ApplyNamingConventions = false)]
            public bool AutoClose { get; set; }
        }

        public static async Task Main()
        {
            Console.WriteLine("开始运行...");

            // 获取订阅链接
            var samplePath = "./Sample.yml";
            var outputPath = "./Output.yml";
            var subscribeLink = "";
            var displayServerInfo = true;
            var autoClose = false;
            var appConfigPath = "./Config.yml";

            if (File.Exists(appConfigPath))
            {
                using var appConfigInputStream = new StreamReader(appConfigPath, Encoding.UTF8);
                var appConfigInput = await appConfigInputStream.ReadToEndAsync();
                appConfigInputStream.Close();
                var appConfigDeserializer = new Deserializer();
                var appConfig = appConfigDeserializer.Deserialize<AppConfig>(appConfigInput);
                samplePath = appConfig.SamplePath;
                outputPath = appConfig.OutputPath;
                subscribeLink = appConfig.SubscribeLink;
                autoClose = appConfig.AutoClose;
                displayServerInfo = appConfig.DisplayServerInfo;
            }
            else
            {
                // 创建一个空白设置文件
                var appConfig = new AppConfig()
                {
                    SamplePath = "./Sample.yml",
                    OutputPath = "./Output.yml",
                    SubscribeLink = "https://",
                    DisplayServerInfo = true,
                    AutoClose = false
                };
                var appConfigSerializer = new Serializer();
                await using StreamWriter appConfigWriter = new StreamWriter("./Config.yml", false, Encoding.UTF8);
                await appConfigWriter.WriteLineAsync(appConfigSerializer.Serialize(appConfig));

                return;
            }

            try
            {
                // http请求
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36");
                var serverContent = await client.GetStringAsync(subscribeLink);
                
                // 解码并拼接为字符串
                serverContent = Base64Decode(serverContent);
                var serverContentText = string.Concat(serverContent);

                // 分割服务器列表文本
                string[] serverList = serverContentText.Split("vmess://");

                // 删除空首行
                if (serverList[0] == "")
                {
                    List<string> list = serverList.ToList();
                    list.RemoveAt(0);
                    serverList = list.ToArray();
                }

                // 解码服务器信息
                for (int i = 0; i < serverList.Length; i++)
                {
                    serverList[i] = Base64Decode(serverList[i]);
                }
                
                // 反序列化
                var v2RayServers = serverList.Select(t => JsonSerializer.Deserialize<V2RayServer>(t)).ToList();
                
                // 显示服务器信息
                if (displayServerInfo)
                {
                    foreach (var server in v2RayServers)
                    {
                        PrintServerInfo(server);
                    }
                }
                
                // 反序列化配置
                if (!File.Exists(samplePath))
                {
                    Console.WriteLine("缺少Sample.yml");
                    Console.WriteLine("按任意键退出...");
                    Console.ReadKey();

                    return;
                }

                using var inputStream = new StreamReader(samplePath, Encoding.UTF8);
                var input = await inputStream.ReadToEndAsync();
                inputStream.Close();
                var inputDeserializer = new Deserializer();
                var yamlConfig = inputDeserializer.Deserialize<YamlConfig>(input);

                for (int j = 0; j < serverList.Length; j++)
                {
                    yamlConfig.Proxies[j].Name = "服务器" + (j + 1);
                    yamlConfig.Proxies[j].Server = v2RayServers[j]!.add;
                    yamlConfig.Proxies[j].Port = Convert.ToInt16(v2RayServers[j]!.port);
                    yamlConfig.Proxies[j].Uuid = v2RayServers[j]!.id;
                    yamlConfig.Proxies[j].AlterId = v2RayServers[j]!.aid;
                    yamlConfig.Proxies[j].Tls = v2RayServers[j]!.tls is "true" or "tls";
                    yamlConfig.Proxies[j].Udp = v2RayServers[j]!.net != "tcp";
                }
                
                // 序列化
                await using var streamWriter = new StreamWriter(outputPath, false, Encoding.UTF8);
                var serializer = new Serializer();
                await streamWriter.WriteLineAsync(serializer.Serialize(yamlConfig));
                streamWriter.Close();

                Console.WriteLine("运行完毕！");

                if (!autoClose)
                {
                    Console.WriteLine("按任意键退出...");
                    Console.ReadKey();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("网络错误！");
                Console.WriteLine("---------Start---------");
                Console.WriteLine(e);
                Console.WriteLine("---------End---------");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }

            // base64解码函数
            static string Base64Decode(string dataIn)
            {
                dataIn = dataIn.Trim()
                    .Replace(Environment.NewLine, "")
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace(" ", "");

                if (dataIn.Length % 4 > 0)
                {
                    dataIn = dataIn.PadRight(dataIn.Length + 4 - dataIn.Length % 4, '=');
                }

                byte[] data = Convert.FromBase64String(dataIn);
                return Encoding.UTF8.GetString(data);
            }

            // 打印服务器信息
            static void PrintServerInfo(V2RayServer? serverIn)
            {
                Console.WriteLine("---------Start---------");
                Console.WriteLine($"名称：{serverIn!.ps}");
                Console.WriteLine($"端口：{serverIn.port}");
                Console.WriteLine($"用户ID：{serverIn.id}");
                Console.WriteLine($"额外ID：{serverIn.aid}");
                Console.WriteLine($"传输协议：{serverIn.net}");
                Console.WriteLine($"伪装类型：{serverIn.type}");
                Console.WriteLine($"TLS：{serverIn.tls}");
                Console.WriteLine($"IP：{serverIn.add}");
                Console.WriteLine("---------End---------");
            }
        }
    }
}