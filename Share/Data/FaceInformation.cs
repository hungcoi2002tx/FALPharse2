using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Data
{
    public class FaceInformation
    {
        public string UserId { get; set; }
        public string FaceId { get; set; }
        public string ImageId { get; set; }
        //Truong nay luu gia tri key cua anh trong s3 nhe
        public string ExternalImageId { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
