using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Model
{
    public enum RequestTypeEnum
    {
        TrainByImage = 0, //Train
        TrainByUrl = 1, //Train
        TrainByZip = 2, //Train
        TrainByFaceId = 3, //Train
        DisassociateFace = 4, //Train
        ResetUser = 5, //Train
        CheckIsTrained = 6, //Train
        GetWebhookResult, //Train
        Detect = 10, //Detect
        DetectImage = 7,//Detect
        DetectVideo = 8,//Detect
        CompareFace = 9,//Compare
    }
}
