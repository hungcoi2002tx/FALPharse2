using FAL.FrontEnd.Models;
using FAL.FrontEnd.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Share.DTO;
using System.Text.Json;

namespace FAL.FrontEnd.Pages.Dashboard
{
    public class MainModel : PageModel
    {
		private readonly IBaseApiService _baseApiService;
        public RequestStatsViewModel RequestStats { get; set; }
        public DetectChartStats DetectStats { get; set; }
        public TrainChartStats TrainStats { get; set; }
        public CollectionChartStats CollectionChartStats { get; set; }
        public int TotalDetect { get; set; } = 0;
        public int TotalTrain { get; set; } = 0;
        public int TotalRequest { get; set; } = 0;
        public int TotalCollection { get; set; } = 0;

        public int CurrentYear { get; set; } // Bi?n n?m hi?n t?i
        public List<int> Years { get; set; } // Danh sách các n?m

        public MainModel(IBaseApiService baseApiService)
        {
            _baseApiService = baseApiService;
        }

        public async Task OnGetAsync(int year)
        {
			try
			{
                Years = Enumerable.Range(DateTime.Now.Year - 3, 7).ToList();
                if (year == 0)
                {
                    year = DateTime.Now.Year;
                    CurrentYear = year;
                }
                else
                {
                    CurrentYear = year;
                }
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                #region stats Request
                var requestResponse = await _baseApiService.CallApiAsync(
                    FEGlobalVarians.REQUEST_CHART_STATS_ENDPOINT,
                    RestSharp.Method.Get,
                    new Dictionary<string, string>
                    {
                        { "year", year.ToString() }
                    }
                    );
                if (requestResponse.IsSuccessful)
                {
                    if (requestResponse.Content == null)
                    {
                        
                    }
                    else
                    {
                        RequestStats = JsonSerializer.Deserialize<RequestStatsViewModel>(requestResponse.Content);
                        TotalRequest = RequestStats.Detect + RequestStats.Compare + RequestStats.Train;
                    }
                }
                else
                {
                    RequestStats = new RequestStatsViewModel();
                    Console.WriteLine("Request failed!");
                }
                #endregion


                #region detect request
                var detectResponse = await _baseApiService.CallApiAsync(
                    FEGlobalVarians.DETECT_CHART_STATS_ENDPOINT,
                    RestSharp.Method.Get,
                    new Dictionary<string, string>
                    {
                        { "year", year.ToString() }
                    }
                    );

                if (detectResponse.IsSuccessful)
                {
                    if (detectResponse.Content == null)
                    {

                    }
                    else
                    {
                        DetectStats = JsonSerializer.Deserialize<DetectChartStats>(detectResponse.Content,options);
                        if (DetectStats?.MonthCounts != null)
                        {
                            TotalDetect = DetectStats.MonthCounts.Values.Sum();
                        }
                    }
                }
                else
                {
                    DetectStats = new DetectChartStats();
                    Console.WriteLine("Request failed!");
                }
                #endregion

                #region train 
                var trainResponse = await _baseApiService.CallApiAsync(
                   FEGlobalVarians.TRAIN_CHART_STATS_ENDPOINT,
                   RestSharp.Method.Get,
                   new Dictionary<string, string>
                   {
                        { "year", year.ToString() }
                   }
                   );

                if (trainResponse.IsSuccessful)
                {
                    if (trainResponse.Content == null)
                    {

                    }
                    else
                    {
                        TrainStats = JsonSerializer.Deserialize<TrainChartStats>(trainResponse.Content, options);
                        if (TrainStats?.MonthCounts != null)
                        {
                            TotalTrain = TrainStats.MonthCounts.Values.Sum();
                        }
                    }
                }
                else
                {
                    TrainStats = new TrainChartStats();
                    Console.WriteLine("Request failed!");
                }
                #endregion


                #region collection
                var collectionResponse = await _baseApiService.CallApiAsync(
                   FEGlobalVarians.COLLECTION_CHART_STATS_ENDPOINT,
                   RestSharp.Method.Get,
                   new Dictionary<string, string>
                   {
                        { "year", year.ToString() }
                   }
                   );
                if (collectionResponse.IsSuccessful)
                {
                    if (collectionResponse.Content == null)
                    {

                    }
                    else
                    {
                        CollectionChartStats = JsonSerializer.Deserialize<CollectionChartStats>(collectionResponse.Content, options);
                        TotalCollection = CollectionChartStats.UserCount + CollectionChartStats.FaceCount + CollectionChartStats.MediaCount;
                    }
                }
                else
                {
                    CollectionChartStats = new CollectionChartStats();
                    Console.WriteLine("Request failed!");
                }
                #endregion


            }
			catch (Exception ex)
			{
                Console.WriteLine(ex);
                throw;
			}
        }
    }
}
