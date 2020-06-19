using Dyndle.Tools.Core;
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
        private static ICoreService _clientInstance;
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

        public static ICoreService GetClient()
        {
            return GetClient(EnvironmentManager.GetDefault());
        }
        public static ICoreService GetClient(string environmentName)
        {
            return GetClient(environmentName == null ? EnvironmentManager.GetDefault() : EnvironmentManager.Get(environmentName));
        }
        public static ICoreService GetClient(Environment environment)
        {
            Uri cmUri = new Uri(environment.CMSUrl);

            if (_clientInstance == null)
            {
                var binding = GetBinding(cmUri.Scheme == "https");

                var endpoint = _version;
                CoreServiceClient coreServiceClient = new CoreServiceClient((Binding)binding,
                    new EndpointAddress($"{cmUri.Scheme}://{cmUri.Host}:{cmUri.Port}/webservices/CoreService{_version}.svc/basicHttp"));
                if (!string.IsNullOrEmpty(environment.Username) && !string.IsNullOrEmpty(environment.Password))
                {
                    ((ClientBase<ICoreService>)coreServiceClient).ClientCredentials.Windows.ClientCredential.UserName = environment.Username;
                    ((ClientBase<ICoreService>)coreServiceClient).ClientCredentials.Windows.ClientCredential.Password = environment.Password;
                }
                if (!string.IsNullOrEmpty(environment.UserDomain))
                    ((ClientBase<ICoreService>)coreServiceClient).ClientCredentials.Windows.ClientCredential.Domain = string.IsNullOrEmpty(environment.UserDomain) ? "." : environment.UserDomain;

                _clientInstance = coreServiceClient;
            }
            return _clientInstance;
        }

        public static StreamUploadClient GetUploadClient()
        {
            var binding = new BasicHttpBinding()
            {
                MessageEncoding = WSMessageEncoding.Mtom,
                TransferMode = TransferMode.StreamedRequest,
            };
            if (_tridionCMUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                binding.Security = new BasicHttpSecurity
                {
                    Mode = BasicHttpSecurityMode.Transport
                };
            }
            StreamUploadClient uploadClient = new StreamUploadClient(binding,
                new EndpointAddress($"{_tridionCMUrl}/webservices/CoreService{_version}.svc/streamUpload_basicHttp"));
            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                ((ClientBase<IStreamUpload>)uploadClient).ClientCredentials.Windows.ClientCredential.UserName = _username;
                ((ClientBase<IStreamUpload>)uploadClient).ClientCredentials.Windows.ClientCredential.Password = _password;
            }
            if (!string.IsNullOrEmpty(_domain))
                ((ClientBase<IStreamUpload>)uploadClient).ClientCredentials.Windows.ClientCredential.Domain = string.IsNullOrEmpty(_domain) ? "." : _domain;

            return uploadClient;
        }

        public static bool EasyCertCheck(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }


        private static Binding GetBinding(bool isHttps)
        {
            if (isHttps)
            {
                Binding binding = new BasicHttpBinding()
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
                return binding;
            }
            else
            {
                var binding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 10485760,
                    ReaderQuotas = new XmlDictionaryReaderQuotas
                    {
                        MaxStringContentLength = 10485760,
                        MaxArrayLength = 10485760
                    },
                    Security = new BasicHttpSecurity
                    {
                        Mode = BasicHttpSecurityMode.TransportCredentialOnly,
                        Transport = new HttpTransportSecurity
                        {
                            ClientCredentialType = HttpClientCredentialType.Windows
                        }
                    }
                };
                return binding;
            }
        }
    }
}

