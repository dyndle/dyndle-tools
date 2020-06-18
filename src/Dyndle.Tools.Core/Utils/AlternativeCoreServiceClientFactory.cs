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

namespace Dyndle.Tools.Core
{
    public static class AlternativeCoreServiceClientFactory
    {
        private static ICoreService _clientInstance;
        private static string _version;

        static AlternativeCoreServiceClientFactory()
        {
            _version = ConfigurationManager.AppSettings["coreservice-version"];
            var trustAll = ConfigurationManager.AppSettings["trust-all-ssl-certificates"];
            if ((!string.IsNullOrEmpty(trustAll)) && (trustAll.Equals("yes", StringComparison.InvariantCultureIgnoreCase) || trustAll.Equals("true", StringComparison.InvariantCultureIgnoreCase)))
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(EasyCertCheck);
            }
        }
        public static ICoreService GetClient()
        {
            return GetClient(EnvironmentManager.GetDefault());
        }
        public static ICoreService GetClient(string environmentName)
        {
            return GetClient(environmentName == null ? EnvironmentManager.GetDefault() : EnvironmentManager.Get(environmentName));
        }
        public static ICoreService GetClient(Dyndle.Tools.Core.Models.Environment environment)
        {
            Uri cmUri = new Uri(environment.CMSUrl);

            if (_clientInstance == null)
            {
                var binding = GetBinding(cmUri.Scheme == "https");

                var endpoint = _version;
                CoreServiceClient coreServiceClient = new CoreServiceClient((Binding)binding, new EndpointAddress(string.Format("{0}://{1}:{2}/webservices/CoreService{3}.svc/basicHttp", cmUri.Scheme, cmUri.Host, cmUri.Port, _version)));
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

