using AuthenExamCompareFace.ExternalService;
using AuthenExamCompareFace.ExternalService.Recognition;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;

namespace AuthenExamCompareFace
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            var mainForm = serviceProvider.GetRequiredService<MainContainer>();

            var addImageForm = serviceProvider.GetRequiredService<AddImageSourceForm>();
            Application.Run(mainForm);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            try
            {
                services.AddMemoryCache();
                services.AddSingleton<AddImageSourceForm>();
                services.AddSingleton<ResultForm>();
                services.AddSingleton<SourceImageForm>();
                services.AddSingleton<MainContainer>();
                services.AddSingleton<ImageCaptureForm>();
                services.AddSingleton<SettingForm>();
                services.AddSingleton<AuthService>(provider =>
                    new AuthService(
                        "https://dev.demorecognition.click/api/Auth/login", 
                        "string",                  
                        "123456"                   
                    ));
                services.AddSingleton<FaceCompareService>(provider =>
                {
                    var authService = provider.GetRequiredService<AuthService>();
                    return new FaceCompareService(
                        authService,
                        "https://dev.demorecognition.click/api/Compare/compare/result" 
                    );
                });

                services.AddSingleton<IRecognitionRestClient>(r => new RecognitionRestClient(
                new RestClient(new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    }
                })
                { BaseAddress = new Uri("https://dev.demorecognition.click") })));

                #region register Rekognition service
                var recognitionService = typeof(BaseRecognitionServices<>).Assembly.ExportedTypes
                   .Where(a => a.FullName.EndsWith("AdapterRecognitionService"));
                foreach (Type implement in recognitionService)
                {
                    services.AddScoped(implement);
                }
                #endregion
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}