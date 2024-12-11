namespace FAL.FrontEnd.Models
{
    public static class FEGlobalVarians
    {

        public static readonly string BE_URL = "https://dev.demorecognition.click";
        //public static readonly string BE_URL = "https://localhost:7065";

        public static readonly string LOGIN_ENDPOINT = BE_URL + "/api/Auth/login";
        public static readonly string DETECT_CHART_STATS_ENDPOINT = "/api/Result/Detect/Chart";
        public static readonly string TRAIN_CHART_STATS_ENDPOINT = "/api/Result/Train/Chart";
        public static readonly string REQUEST_CHART_STATS_ENDPOINT = "/api/Result/Request/Chart";
        public static readonly string COLLECTION_CHART_STATS_ENDPOINT = "/api/Result/Collection/Chart";
    }
}
