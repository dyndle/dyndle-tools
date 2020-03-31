using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tridion.ContentManager.CoreService.Client;
using Environment = Dyndle.Tools.Core.Models.Environment;

namespace Dyndle.Tools.Core
{
    public static class CoreserviceClientFactory
    {
        private static SessionAwareCoreServiceClient _clientInstance;
        private static string _tridionCMUrl;
        private static string _username;
        private static string _password;
        private static string _domain;
        private static string _version;

        static CoreserviceClientFactory()
        {
            // the following settings are hard-coded for now, perhaps we will make this configuration later 
            _version = "201603";
            var trustAll = true;
            if (trustAll)
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(EasyCertCheck);
            }
        }

        public static void SetEnvironment(Environment environment)
        {
            _tridionCMUrl = environment.CMSUrl;
            _username = environment.Username;
            _domain = environment.UserDomain;
            _password = environment.Password;
        }

        public static SessionAwareCoreServiceClient GetClient()
        {
            if (_clientInstance == null)
            {
                if (_tridionCMUrl == null || _username == null || _password == null)
                {
                    throw new Exception("environment has not been set, always call SetEnvironment before GetClient");
                }

                Uri tridionCMUri = new Uri(_tridionCMUrl);
                _clientInstance = Wrapper.GetCoreServiceWsHttpInstance(tridionCMUri.Host, tridionCMUri.Port, tridionCMUri.Scheme, _username, _password, _domain, _version);
            }
            return _clientInstance;
        }

        public static bool EasyCertCheck(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        public static StreamUploadClient GetUploadClient()
        {
            return Wrapper.GetUploadClient(_tridionCMUrl, _username, _password, _domain, _version);
        }

    
    }

    public static class Wrapper
    {
        private static SessionAwareCoreServiceClient Instance { get; set; }

        public static SessionAwareCoreServiceClient GetCoreServiceInstance(string hostName, string username, string password, string domain, string version, bool trustAll)
        {
            var endpoint = version;
            SessionAwareCoreServiceClient coreServiceClient = new SessionAwareCoreServiceClient((Binding)new NetTcpBinding()
            {
                MaxReceivedMessageSize = (long)int.MaxValue,
                ReaderQuotas = new XmlDictionaryReaderQuotas()
                {
                    MaxStringContentLength = int.MaxValue,
                    MaxArrayLength = int.MaxValue
                }
            }, new EndpointAddress(string.Format("net.tcp://{0}:2660/CoreService/{1}/netTcp", (object)hostName, endpoint)));
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                ((ClientBase<ISessionAwareCoreService>)coreServiceClient).ClientCredentials.Windows.ClientCredential.UserName = username;
                ((ClientBase<ISessionAwareCoreService>)coreServiceClient).ClientCredentials.Windows.ClientCredential.Password = password;
            }
            if (!string.IsNullOrEmpty(domain))
                ((ClientBase<ISessionAwareCoreService>)coreServiceClient).ClientCredentials.Windows.ClientCredential.Domain = string.IsNullOrEmpty(domain) ? "." : domain;
            Wrapper.Instance = coreServiceClient;

            if (trustAll)
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(EasyCertCheck);
            }

            return Wrapper.Instance;
        }

        public static StreamUploadClient GetUploadClient(string url, string username, string password, string domain, string version)
        {
            StreamUploadClient uploadClient = new StreamUploadClient((Binding)new BasicHttpBinding()
            {
                MessageEncoding = WSMessageEncoding.Mtom,
                TransferMode = TransferMode.StreamedRequest
            }, new EndpointAddress(string.Format("{0}/webservices/CoreService{1}.svc/streamUpload_basicHttp", url, version)));
            // http://localhost/webservices/CoreService201701.svc/streamUpload_basicHttp
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                ((ClientBase<IStreamUpload>)uploadClient).ClientCredentials.Windows.ClientCredential.UserName = username;
                ((ClientBase<IStreamUpload>)uploadClient).ClientCredentials.Windows.ClientCredential.Password = password;
            }
            if (!string.IsNullOrEmpty(domain))
                ((ClientBase<IStreamUpload>)uploadClient).ClientCredentials.Windows.ClientCredential.Domain = string.IsNullOrEmpty(domain) ? "." : domain;

            return uploadClient;
        }

        private static Binding GetBinding(bool isHttps)
        {
            if (isHttps)
            {
                return new BasicHttpBinding()
                {
                    MaxReceivedMessageSize = (long)int.MaxValue,
                    ReaderQuotas = new XmlDictionaryReaderQuotas()
                    {
                        MaxStringContentLength = int.MaxValue,
                        MaxArrayLength = int.MaxValue
                    },
                    Security =
                    {
                        Mode = BasicHttpSecurityMode.Transport,
                        Transport =
                        {
                            ClientCredentialType = HttpClientCredentialType.Windows
                        }
                    }
                };
            }
            return new WSHttpBinding()
            {
                MaxReceivedMessageSize = (long)int.MaxValue,
                ReaderQuotas = new XmlDictionaryReaderQuotas()
                {
                    MaxStringContentLength = int.MaxValue,
                    MaxArrayLength = int.MaxValue
                },
                SendTimeout = new TimeSpan(0, 15, 0),
                OpenTimeout = new TimeSpan(0, 15, 0),
                CloseTimeout = new TimeSpan(0, 15, 0),
                ReceiveTimeout = new TimeSpan(0, 15, 0)
            };
        }
        public static bool EasyCertCheck(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        public static SessionAwareCoreServiceClient GetCoreServiceWsHttpInstance(string hostName, int port, string protocol, string username, string password, string domain, string version)
        {
            var binding = GetBinding(protocol == "https");

            SessionAwareCoreServiceClient coreServiceClient = new SessionAwareCoreServiceClient(binding, new EndpointAddress(string.Format("{3}://{0}:{1}/webservices/CoreService{2}.svc/wsHttp", (object)hostName, port, version, protocol)));
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                ((ClientBase<ISessionAwareCoreService>)coreServiceClient).ClientCredentials.Windows.ClientCredential.UserName = username;
                ((ClientBase<ISessionAwareCoreService>)coreServiceClient).ClientCredentials.Windows.ClientCredential.Password = password;
            }
            if (!string.IsNullOrEmpty(domain))
                ((ClientBase<ISessionAwareCoreService>)coreServiceClient).ClientCredentials.Windows.ClientCredential.Domain = string.IsNullOrEmpty(domain) ? "." : domain;
            Wrapper.Instance = coreServiceClient;
            return Wrapper.Instance;
        }
    }   
}

