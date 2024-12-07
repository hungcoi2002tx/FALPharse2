using FAL.FrontEnd.Models;
using FAL.FrontEnd.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FAL.FrontEnd.Pages.Dashboard
{
    public class MainModel : PageModel
    {
		private readonly IBaseApiService _baseApiService;

        public MainModel(IBaseApiService baseApiService)
        {
            _baseApiService = baseApiService;
        }

        public async Task OnGetAsync(int year)
        {
			try
			{
                if(year == 0)
                {
                    year = DateTime.Now.Year;
                }
                var requestResponse = await _baseApiService.CallApiAsync(
                    FEGlobalVarians.REQUEST_CHART_STATS_ENDPOINT,
                    RestSharp.Method.Get,
                    new Dictionary<string, string>
                    {
                        { "year", year.ToString() }
                    }
                    );
                var detectResponse = await _baseApiService.CallApiAsync(
                    FEGlobalVarians.DETECT_CHART_STATS_ENDPOINT,
                    RestSharp.Method.Get,
                    new Dictionary<string, string>
                    {
                        { "year", year.ToString() }
                    }
                    );
                var trainResponse = await _baseApiService.CallApiAsync(
                    FEGlobalVarians.TRAIN_CHART_STATS_ENDPOINT,
                    RestSharp.Method.Get,
                    new Dictionary<string, string>
                    {
                        { "year", year.ToString() }
                    }
                    );
                var collectionResponse = await _baseApiService.CallApiAsync(
                    FEGlobalVarians.COLLECTION_CHART_STATS_ENDPOINT,
                    RestSharp.Method.Get,
                    new Dictionary<string, string>
                    {
                        { "year", year.ToString() }
                    }
                    );

            }
			catch (Exception ex)
			{
                Console.WriteLine(ex);
                throw;
			}
        }
    }
}
