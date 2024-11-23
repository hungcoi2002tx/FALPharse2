using CompareFaceExamDemo.ExternalService.Recognition;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;

namespace CompareFaceExamDemo
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
            Application.Run(mainForm);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            try
            {
                services.AddMemoryCache();
                services.AddSingleton<Main>();
                services.AddSingleton<MainContainer>();
                services.AddSingleton<ImageCaptureForm>();
                services.AddSingleton<ImageSourceForm>();
                services.AddSingleton<SettingForm>();
                services.AddSingleton<SourceImageForm>();
                services.AddSingleton<Test>();

                services.AddSingleton<IRecognitionRestClient>(r => new RecognitionRestClient(
                new RestClient(new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    }
                })
                { BaseAddress = new Uri("https://dev.demorecognition.click") })));


                services.AddSingleton<MainContainer>();
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