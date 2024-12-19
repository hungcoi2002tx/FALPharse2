namespace FAL.FrontEnd.Models
{
    public static class FEGlobalVarians
    {

        public static readonly string BE_URL = "https://dev.demorecognition.click";
        //public static readonly string BE_URL = "https://localhost:7065";

        public static readonly string LOGIN_ENDPOINT = BE_URL + "/api/Auth/login";
        public static readonly string REGISTER_ENDPOINT = BE_URL + "/api/Auth/register";

        public static readonly string CHANGE_PASS_ENDPOINT = BE_URL + "/api/users/change-password";
        public static readonly string ACCOUNTS_ENDPOINT = BE_URL + "/api/Accounts";
        
        public static readonly string RESET_PASS_ENDPOINT = BE_URL + "/api/accounts/reset-password";
        public static readonly string USERS_ENDPOINT = BE_URL + "/api/users";
        public static readonly string USERS_ME_ENDPOINT = BE_URL + "/api/users/me";


        public static readonly string DETECT_CHART_STATS_ENDPOINT = "/api/Result/Detect/Chart";
        public static readonly string TRAIN_CHART_STATS_ENDPOINT = "/api/Result/Train/Chart";
        public static readonly string REQUEST_CHART_STATS_ENDPOINT = "/api/Result/Request/Chart";
        public static readonly string COLLECTION_CHART_STATS_ENDPOINT = "/api/Result/Collection/Chart";
    }
}
