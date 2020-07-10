namespace QRCodeMaker.Response
{
    public class AccessTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string errcode { get; set; }
        public string errmsg { get; set; }
    }
}
