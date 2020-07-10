namespace QRCodeMaker
{
    class Setting
    {
        /// <summary>
        /// 微信生成二维码url
        /// </summary>
        public string WXQRUrl { get; set; }

        /// <summary>
        /// 获取token url
        /// </summary>
        public string GetAccessToken { get; set; }

        /// <summary>
        /// Appid
        /// </summary>
        public string Appid { get; set; }

        /// <summary>
        /// AppSecret
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// 生成数量
        /// </summary>
        public string Count { get; set; }

        /// <summary>
        /// 开始数值
        /// </summary>
        public string StartNum { get; set; }

    }
}
