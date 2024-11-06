using System;
using System.IO;
using System.Net;

namespace CameraCaptureLibrary
{
    public class ApiClient
    {
        private readonly string apiUrl;

        public ApiClient(string apiUrl)
        {
            this.apiUrl = apiUrl;
        }

        public string RegisterCompare(string imagePath, string studentNo)
        {
            if (string.IsNullOrEmpty(imagePath) || string.IsNullOrEmpty(studentNo))
            {
                throw new ArgumentException("imagePath and studentNo cannot be null or empty.");
            }

            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("The specified image file does not exist.", imagePath);
            }

            var fullUrl = $"{apiUrl}?studentCode={studentNo}";

            var request = (HttpWebRequest)WebRequest.Create(fullUrl);
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=----WebKitFormBoundary";

            var boundary = "----WebKitFormBoundary";
            var boundaryBytes = System.Text.Encoding.ASCII.GetBytes("--" + boundary + "\r\n");

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                var header = $"Content-Disposition: form-data; name=\"targetImage\"; filename=\"{Path.GetFileName(imagePath)}\"\r\n" +
                             "Content-Type: image/jpeg\r\n\r\n";
                var headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
                requestStream.Write(headerBytes, 0, headerBytes.Length);

                using (var fileStream = File.OpenRead(imagePath))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }
                }


                var trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                requestStream.Write(trailer, 0, trailer.Length);
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                using (var errorResponse = (HttpWebResponse)ex.Response)
                using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                {
                    return $"Error: {reader.ReadToEnd()}";
                }
            }
        }

    }
}
