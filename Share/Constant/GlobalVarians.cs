using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Constant
{
    public static class GlobalVarians
    {
        //public static readonly string SystermId = "testRekognition";
        public static readonly string SystermId = "systemName";
        public static readonly long MAXFILESIZE = 15L * 1024 * 1024 * 1024;
        public static readonly long PARTSIZE = 500 * 1024 * 1024;
        public static readonly long DIVIDESIZE = 500 * 1024 * 1024;

        public const string FACEID_TABLE_DYNAMODB = "FaceInfo";
        public const string RESULT_INFO_TABLE_DYNAMODB = "ResultInfo";
        public const string CLIENT_REQUESTS_TABLE_DYNAMODB = "ClientRequests";
        public const string SYSTEM_NAME_ATTRIBUTE_DYNAMODB = "SystemName";
        public const string FACEID_INDEX_ATTRIBUTE_DYNAMODB = "SystemNameIndex";
    }
}
