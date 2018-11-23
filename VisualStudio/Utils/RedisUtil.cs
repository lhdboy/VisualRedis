using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRedis.Utils
{
    public static class RedisUtil
    {
        public static double Ping(ConnectionMultiplexer connectionMultiplexer)
        {
            try
            {
                int testCount = 0;
                double testSumTimeout = 0;
                foreach (var ep in connectionMultiplexer.GetEndPoints())
                {
                    var server = connectionMultiplexer.GetServer(ep);

                    TimeSpan timeSpan = server.Ping();

                    testSumTimeout += timeSpan.TotalMilliseconds;
                    testCount++;
                }

                return testSumTimeout / testCount;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static IServer GetServer(ConnectionMultiplexer connectionMultiplexer)
        {
            foreach (var ep in connectionMultiplexer.GetEndPoints())
            {
                return connectionMultiplexer.GetServer(ep);
            }

            return null;
        }

        public static string TryGetValueJsonFormat(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            try
            {
                object obj = serializer.Deserialize(jtr);
                if (obj != null)
                {
                    StringWriter textWriter = new StringWriter();
                    JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 4,
                        IndentChar = ' '
                    };
                    serializer.Serialize(jsonWriter, obj);
                    return textWriter.ToString();
                }
                else
                {
                    return str;
                }
            }
            catch (Exception)
            {
                return str;
            }
        }
    }
}
