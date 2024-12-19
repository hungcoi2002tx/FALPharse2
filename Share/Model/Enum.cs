using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Model
{
    public enum TypeOfRequest
    {
        Training = 0,
        Tagging = 1,
    }

    public enum ContentType
    {
        Image = 0,
        Video = 1,
    }

    public enum TrainResult
    {
        Success = 0,
        Fail= 1,
        Error = 2,
    }
}
