using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.IO;
using System.Data;
using System.Reflection;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

using Interface;

namespace Client
{
  class Program
  {
    static void Main(string[] args)
    {
      string text = "Tuffi Saliba Neto";
      Console.WriteLine(text);
      Console.WriteLine(Criptografia.Encrypt(text));
      Console.Write(Criptografia.Decrypt(Criptografia.Encrypt(text)));
      IClientChannel proxy = CreateChannel(true);
      bool success = false;
      try
      {
        Console.WriteLine(((ITeste)proxy).GetString());
        proxy.Close();
        success = true;
      }
      finally
      {
        if (!success)
        {
          proxy.Abort();
        }
      }
    }

    private static IClientChannel CreateChannel(bool secure)
    {
      //http://web.archive.org/web/20100703123454/http://old.iserviceoriented.com/blog/post/Indisposable+-+WCF+Gotcha+1.aspx
      NetTcpBinding tcp = new NetTcpBinding();
      if (secure)
      {
        tcp.Security.Mode = SecurityMode.Message;
        tcp.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.TripleDes;
        tcp.Security.Message.ClientCredentialType = MessageCredentialType.None;
      }
      else
        tcp.Security.Mode = SecurityMode.None;
      tcp.MaxBufferPoolSize = Int32.MaxValue;
      tcp.MaxBufferSize = Int32.MaxValue;
      tcp.MaxReceivedMessageSize = Int32.MaxValue;
      tcp.MaxConnections = 500;
      tcp.ListenBacklog = 500;

      XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
      quotas.MaxArrayLength = Int32.MaxValue;
      quotas.MaxBytesPerRead = Int32.MaxValue;
      quotas.MaxDepth = Int32.MaxValue;
      quotas.MaxNameTableCharCount = Int32.MaxValue;
      quotas.MaxStringContentLength = Int32.MaxValue;
      tcp.ReaderQuotas = quotas;
      tcp.CloseTimeout = TimeSpan.MaxValue;
      tcp.OpenTimeout = TimeSpan.MaxValue;
      tcp.ReceiveTimeout = TimeSpan.MaxValue;
      tcp.SendTimeout = TimeSpan.MaxValue;
      ChannelFactory<ITeste> channelFactory = new ChannelFactory<ITeste>(tcp, string.Format("net.tcp://{0}:{1}/{2}", "127.0.0.1", 10000, "TesteServer"));
      //channelFactory.Endpoint.Behaviors.Add(new VbiCustomBehavior());

      foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
      {
        DataContractSerializerOperationBehavior dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>() as DataContractSerializerOperationBehavior;
        if (dataContractBehavior != null)
          dataContractBehavior.MaxItemsInObjectGraph = Int32.MaxValue;
      }

      if (secure)
      {
        string path = Path.Combine(@"C:\Temp\TesteSSLFramework40", "VergaServer.cer");
        channelFactory.Credentials.ClientCertificate.Certificate = new X509Certificate2(path);
        channelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;

        //EndpointAddress epa = new EndpointAddress(channelFactory.Endpoint.ListenUri, EndpointIdentity.CreateDnsIdentity(channelFactory.Endpoint.ListenUri.Host), new AddressHeader[0]);
        EndpointAddress epa = new EndpointAddress(channelFactory.Endpoint.ListenUri, EndpointIdentity.CreateDnsIdentity("VergaServer"), new AddressHeader[0]);

        channelFactory.Endpoint.Address = epa;
      }

      return (IClientChannel)channelFactory.CreateChannel();
    }
  }
}
