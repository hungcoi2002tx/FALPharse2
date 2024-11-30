using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Model
{
    public enum RequestTypeEnum
    {
        TrainByImage = 0,
        TrainByUrl = 1,
        TrainByZip = 2,
        TrainByFaceId = 3,
        DisassociateFace = 4,
        ResetUser = 5,
        CheckIsTrained = 6,
        GetWebhookResult,
        Detect = 10,
        DetectImage = 7,
        DetectVideo = 8,
        CompareFace = 9,
    }
}
