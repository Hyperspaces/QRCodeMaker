using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCodeMaker.Dao;
using QRCodeMaker.Response;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QRCodeMaker
{
    class Program
    {
        private static int StartNum { get; set; }

        private static int Count { get; set; }

        private static int Index { get; set; }

        private static object getObject { get; set; } = new object();
        private static object setObject { get; set; } = new object();

        static async Task Main(string[] args)
        {
            //获取配置
            var setting = GetSetting();

            Count = setting.Count == null ? 10000 : Convert.ToInt32(setting.Count);
            StartNum = setting.StartNum == null ? 1 : Convert.ToInt32(setting.StartNum);
            Index = StartNum;

            //var tokenResponse = await GetAccessTokenAsync(setting);
            var tokenResponse = "35_qPzOT7cNcCpT9jjqAyHsx8V_YIOcluS54MUeVGsUX-X7HFuzFuL9HZp8cy-JLtaG_J8TSCRzEpUWK-aJlWurjUU6p7uPq7aV0cENVJ2YsQOCe7iTHw6-f6slVHmLbLjlbxiohsLDJLGzSIdiBTQiAIAGYV";

            Queue q = new Queue(1000);

            //请求微信二维码图片buffer
            for (int i = 0; i < 1; i++)
            {
                await Task.Run(() => SavePictureBuffer(setting.WXQRUrl, tokenResponse, q, StartNum + Count));
            }

            Console.ReadKey();
            Console.WriteLine($"所有二维码已保存到数据库中，开始生成二维码");


            var context = new HLContext();
            var pictureList = context.HomeLetter.ToList();
            var img = Image.FromFile("QRCodeTemp.png");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int sumCount = 1;

            foreach (var buffer in pictureList)
            {
                Bitmap bmp = new Bitmap(img);
                Graphics graphics = Graphics.FromImage(bmp);
                if (buffer == null)
                {
                    Console.WriteLine($"第{sumCount}张二维码生成失败");
                    continue;
                }
                var QRcodePic = Image.FromStream(new MemoryStream(buffer.PictureBuffer));
                DrawImage(graphics, QRcodePic, buffer.Id);
                bmp.Save($"QRCode/QRCode-{buffer.Id}.png"); // 保存到原图
                graphics.Dispose(); // 图片处理过程完成，剩余资源全部释放
                bmp.Dispose();
                Console.WriteLine($"成功生成了第{sumCount++}张二维码");
            }


            //            while (true)
            //            {
            //                if (q.Count != 0)
            //                {
            //                    dynamic buffer = q.Dequeue();
            //                    Bitmap bmp = new Bitmap(img);
            //                    Graphics graphics = Graphics.FromImage(bmp);
            //                    if (buffer == null)
            //                    {
            //                        Console.WriteLine($"第{sumCount}张二维码生成失败");
            //                        continue;
            //                    }
            //
            //                    var QRcodePic = Image.FromStream(new MemoryStream(buffer.Buffer));
            //                    DrawImage(graphics, QRcodePic, buffer.Num);
            //                    bmp.Save($"QRCode/QRCode-{buffer.Num}.png"); // 保存到原图
            //                    graphics.Dispose(); // 图片处理过程完成，剩余资源全部释放
            //                    bmp.Dispose();
            //                    Console.WriteLine($"成功生成了第{sumCount++}张二维码");
            //                    if (sumCount == Count + 1)
            //                        break;
            //                }
            //            }

            sw.Stop();
            TimeSpan tsSum = sw.Elapsed;
            img.Dispose(); // 释放原图资源
            Console.WriteLine($"成功生成了{Count}张二维码,耗时{tsSum.TotalMilliseconds / 1000 / 60 }分钟");
            Console.ReadKey();
        }

        /// <summary>
        /// 获取小程序访问token
        /// </summary>
        /// <returns></returns>
        public static async Task<AccessTokenResponse> GetAccessTokenAsync(Setting setting)
        {
            var accessTokenUrl = $"{setting.GetAccessToken}?grant_type=client_credential&appid={setting.Appid}&secret={setting.Secret}";
            return await HttpHelper.Get<AccessTokenResponse>(accessTokenUrl).ConfigureAwait(false);
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        public static Setting GetSetting()
        {
            Setting config = new Setting();
            string FilePath = "setting.json";
            using (System.IO.StreamReader file = System.IO.File.OpenText(FilePath))
            {

                using (JsonTextReader reader = new JsonTextReader(file))
                {

                    var o = (JObject)JToken.ReadFrom(reader);
                    config = o.ToObject<Setting>();
                }
            }

            return config;
        }

        /// <summary>
        /// 绘画图片
        /// </summary>
        public static void DrawImage(Graphics graphics, Image qrCodePic, int index)
        {
            graphics.DrawImage(qrCodePic, 4181, 23, 237, 236);
            Font font = new Font("宋体", 18);
            SolidBrush sbrush = new SolidBrush(Color.Black);
            graphics.DrawString(index.ToString(), font, sbrush, new PointF(4430, 232));
        }

        /// <summary>
        /// 保存图片buffer到队列
        /// </summary>
        public static async void SavePictureBuffer(string url, string accessToken, Queue q, int finNum)
        {
            var context = new HLContext();

            do
            {
                var code = GetCode();
                var inputModel = new
                {
                    scene = code,
                    page = "letter/pages/Guide/Guide"
                };
                string postData = JsonConvert.SerializeObject(inputModel);
                var pictureBuffer = await HttpHelper.PostFile(postData, $"{url}?access_token={accessToken}")
                    .ConfigureAwait(false);
                if (pictureBuffer == null)
                {
                    Console.WriteLine($"第{Index}张二维码保存失败，再次尝试保存");
                    SavePictureBuffer(url, accessToken, q, finNum);
                    lock (getObject)
                    {
                        context.SaveChanges();
                    }
                }
                else
                {
                    lock (setObject)
                    {
                        context.HomeLetter.Add(new HomeLetter() { QRCode = code, PictureBuffer = pictureBuffer });
                        Console.WriteLine($"成功保存第{Index}张二维码");
                        Index++;
                    }
                }
            } while (Index <= finNum);

            lock (getObject)
            {
                context.SaveChanges();
            }
        }

        public static string GetCode()
        {
            var prefix = "HL";
            TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);//ToUniversalTime()转换为标准时区的时间,去掉的话直接就用北京时间
            //随机数
            var strRandomResult = NextRandom(1000, 1).ToString();

            var code = prefix + (long)ts.TotalMilliseconds + strRandomResult;//13位当前时间戳+4位随机数
            return code;
        }

        public static int NextRandom(int numSeeds, int length)
        {
            // Create a byte array to hold the random value.  
            byte[] randomNumber = new byte[length];
            // Create a new instance of the RNGCryptoServiceProvider.  
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            // Fill the array with a random value.  
            rng.GetBytes(randomNumber);
            // Convert the byte to an uint value to make the modulus operation easier.  
            uint randomResult = 0x0;
            for (int i = 0; i < length; i++)
            {
                randomResult |= ((uint)randomNumber[i] << ((length - 1 - i) * 8));
            }
            return (int)(randomResult % numSeeds) + 1;
        }

        private static int GetActiveThreadCount()
        {
            int MaxWorkerThreads, miot, AvailableWorkerThreads, aiot;

            //获得最大的线程数量
            ThreadPool.GetMaxThreads(out MaxWorkerThreads, out miot);

            AvailableWorkerThreads = aiot = 0;

            //获得可用的线程数量
            ThreadPool.GetAvailableThreads(out AvailableWorkerThreads, out aiot);

            //返回线程池中活动的线程数
            return MaxWorkerThreads - AvailableWorkerThreads;
        }
    }
}
