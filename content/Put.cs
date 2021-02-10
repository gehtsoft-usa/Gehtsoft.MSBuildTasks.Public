using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Net;
using System.Collections.Specialized;

namespace Gehtsoft.Build.ContentDelivery
{
    public class PutContent : Microsoft.Build.Utilities.Task
    {
        public string File { get; set; }
        public string BaseUrl { get; set; }

        public string Key { get; set; }
        public override bool Execute()
        {
            Log.LogMessage("Pushing {0} to {1}...", File, BaseUrl);
            MultiPartFormUpload uploader = new MultiPartFormUpload();
            var res = uploader.Upload($"{BaseUrl}/api/upload.php?key={Key}", new NameValueCollection(), new NameValueCollection(), new List<FileInfo> { new FileInfo(File) });
            return res.HttpStatusCode == HttpStatusCode.OK;
        }

        public class MultiPartFormUpload
        {
            public class MimePart
            {
                NameValueCollection _headers = new NameValueCollection();
                byte[] _header;

                public NameValueCollection Headers
                {
                    get { return _headers; }
                }

                public byte[] Header
                {
                    get { return _header; }
                }

                public long GenerateHeaderFooterData(string boundary)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append("--");
                    stringBuilder.Append(boundary);
                    stringBuilder.AppendLine();
                    foreach (string key in _headers.AllKeys)
                    {
                        stringBuilder.Append(key);
                        stringBuilder.Append(": ");
                        stringBuilder.AppendLine(_headers[key]);
                    }
                    stringBuilder.AppendLine();

                    _header = Encoding.UTF8.GetBytes(stringBuilder.ToString());

                    return _header.Length + Data.Length + 2;
                }

                public Stream Data { get; set; }
            }

            public class UploadResponse
            {
                public UploadResponse(HttpStatusCode httpStatusCode, string responseBody)
                {
                    HttpStatusCode = httpStatusCode;
                    ResponseBody = responseBody;
                }

                public HttpStatusCode HttpStatusCode { get; set; }

                public string ResponseBody { get; set; }
            }

            public UploadResponse Upload(string url, NameValueCollection requestHeaders, NameValueCollection requestParameters, List<FileInfo> files)
            {
                using (WebClient client = new WebClient())
                {
                    List<MimePart> mimeParts = new List<MimePart>();

                    try
                    {
                        foreach (string key in requestHeaders.AllKeys)
                        {
                            client.Headers.Add(key, requestHeaders[key]);
                        }

                        foreach (string key in requestParameters.AllKeys)
                        {
                            MimePart part = new MimePart();

                            part.Headers["Content-Disposition"] = "form-data; name=\"" + key + "\"";
                            part.Data = new MemoryStream(Encoding.UTF8.GetBytes(requestParameters[key]));

                            mimeParts.Add(part);
                        }

                        foreach (FileInfo file in files)
                        {
                            MimePart part = new MimePart();
                            string name = file.Extension.Substring(1);
                            string fileName = file.Name;

                            part.Headers["Content-Disposition"] = "form-data; name=\"files[]\"; filename=\"" + fileName + "\"";
                            part.Headers["Content-Type"] = "application/octet-stream";
                            part.Data = new MemoryStream(System.IO.File.ReadAllBytes(file.FullName));
                            mimeParts.Add(part);
                        }

                        string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
                        client.Headers.Add(HttpRequestHeader.ContentType, "multipart/form-data; boundary=" + boundary);

                        long contentLength = 0;

                        byte[] _footer = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");

                        foreach (MimePart mimePart in mimeParts)
                        {
                            contentLength += mimePart.GenerateHeaderFooterData(boundary);
                        }

                        byte[] buffer = new byte[8192];
                        byte[] afterFile = Encoding.UTF8.GetBytes("\r\n");
                        int read;

                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            foreach (MimePart mimePart in mimeParts)
                            {
                                memoryStream.Write(mimePart.Header, 0, mimePart.Header.Length);

                                while ((read = mimePart.Data.Read(buffer, 0, buffer.Length)) > 0)
                                    memoryStream.Write(buffer, 0, read);

                                mimePart.Data.Dispose();

                                memoryStream.Write(afterFile, 0, afterFile.Length);
                            }

                            memoryStream.Write(_footer, 0, _footer.Length);
                            byte[] responseBytes = client.UploadData(url, memoryStream.ToArray());
                            string responseString = Encoding.UTF8.GetString(responseBytes);
                            return new UploadResponse(HttpStatusCode.OK, responseString);
                        }
                    }
                    catch (Exception ex)
                    {
                        foreach (MimePart part in mimeParts)
                            if (part.Data != null)
                                part.Data.Dispose();

                        if (ex.GetType().Name == "WebException")
                        {
                            WebException webException = (WebException)ex;
                            HttpWebResponse response = (HttpWebResponse)webException.Response;
                            string responseString;

                            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                            {
                                responseString = reader.ReadToEnd();
                            }

                            return new UploadResponse(response.StatusCode, responseString);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }
    }
}
