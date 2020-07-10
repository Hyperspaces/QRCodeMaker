using System.ComponentModel.DataAnnotations.Schema;

namespace QRCodeMaker.Dao
{
    [Table("HomeLetter")]
    public class HomeLetter
    {
        public int Id { get; set; }

        public string QRCode { get; set; }

        public byte[] PictureBuffer { get; set; }

    }
}
