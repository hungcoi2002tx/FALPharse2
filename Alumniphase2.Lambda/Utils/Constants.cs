using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alumniphase2.Lambda.Utils
{
    public static class SystemConstants
    {
        public const string TYPE_OF_REQUEST = "TypeOfRequest";
        public const string CONTENT_TYPE = "ContentType";
        public const string ORIGINAL_FILE_NAME = "OriginalFileName";

        public const string VIDEO = "Video";
        public const string IMAGE = "Image";

        public const string USER_ID_ATTRIBUTE = "UserId";
        public const string FUALUMNI_RESPONSE_RESULT_TABLE = "fualumni-result";

        public const string USER_ID_ATTRIBUTE_DYNAMODB = "UserId";
        public const string ID_ATTRIBUTE_DYNAMODB = "Id";
        public const string FACE_ID_ATTRIBUTE_DYNAMODB = "FaceId";
        public const string DATA_ATTRIBUTE_DYNAMODB = "Data";
        public const string CREATE_DATE_ATTRIBUTE_DYNAMODB = "CreateDate";
        public const string FILE_NAME_ATTRIBUTE_DYNAMODB = "FileName";
        public const string SYSTEM_NAME_ATTRIBUTE_DYNAMODB = "SystemName";
        public const string FACEID_TABLE_DYNAMODB = "FaceInfo";
        public const string FACEID_INDEX_ATTRIBUTE_DYNAMODB = "SystemNameIndex";
        public const string RESULT_INFO_TABLE_DYNAMODB = "ResultInfo";

        public const string DATE_FORMAT = "MM/dd/yyyy h:mm:ss tt";
        public const string TIME_ZONE_VIET_NAM = "SE Asia Standard Time";
    }

}
