using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json.Linq;
using Share.SystemModel;
using System.Reflection.Metadata.Ecma335;

namespace FAL.Utils
{
    public class StringExtention
    {
        public static string GetKeyByJson(string json)
        {
			try
			{
                JObject jsonObject = JObject.Parse(json);
                return jsonObject[GlobalVarians.SystermId].ToString();
            }
			catch (Exception)
			{

				throw;
			}
        }
    }
}
