using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VisualRedis.Models;

namespace VisualRedis.Utils
{
    public class ConfigUtil
    {
        public static string ConfigFileName = AppContext.BaseDirectory + "connections.cfg";

        /// <summary>
        /// 配置文件是否存在
        /// </summary>
        /// <returns></returns>
        public static bool ConfigFileExitis()
             => File.Exists(ConfigFileName);

        /// <summary>
        /// 获取服务器列表
        /// </summary>
        /// <returns></returns>
        public static IList<Connection> Get()
        {
            CheckOrCreate("[]");

            using (var fileReader = File.OpenRead(ConfigFileName))
            {
                using (StreamReader reader = new StreamReader(fileReader))
                {
                    return JsonConvert.DeserializeObject<IList<Connection>>(reader.ReadToEnd());
                }
            }
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connStr"></param>
        public static void Update(IList<Connection> connDictCache)
        {
            File.Delete(ConfigFileName);

            CheckOrCreate("");

            using (var fileWriter = File.OpenWrite(ConfigFileName))
            {
                using (StreamWriter writer = new StreamWriter(fileWriter))
                {
                    writer.Write(JsonConvert.SerializeObject(connDictCache));
                }
            }
        }

        private static void CheckOrCreate(string text)
        {
            if (!ConfigFileExitis())
            {
                //如果文件不存在则创建
                using (var writer = File.CreateText(ConfigFileName))
                {
                    writer.Write(text);
                }
            }
        }
    }
}
