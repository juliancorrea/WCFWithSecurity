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
      static string ServerName = ConfigurationManager.AppSettings["Server"];
      static int  Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);

    static void Main(string[] args)
    {
      System.Threading.Thread.Sleep(2000);
      IClientChannel proxy = CreateChannel(true);
      bool success = false;
      try
      {
          //Console.ReadKey();
        Console.WriteLine(((ITeste)proxy).GetString());
        proxy.Close();
        success = true;
      }
      catch(Exception ex)
      {
          Console.WriteLine("ERRO : " + ex.Message);

          Exception inner = ex.InnerException;
          while (inner != null)
          {
              Console.WriteLine("ERRO INNER: " + inner.Message);
              inner = inner.InnerException;
          }
      }
      finally
      {
        if (!success)
        {
          proxy.Abort();
        }
      }
      Console.ReadKey();
    }

    private static IClientChannel CreateChannel(bool secure)
    {
      //http://web.archive.org/web/20100703123454/http://old.iserviceoriented.com/blog/post/Indisposable+-+WCF+Gotcha+1.aspx

        //WSHttpBinding binding = new WSHttpBinding();
        //if (secure)
        //{
        //    binding.Security.Mode = SecurityMode.Message;
        //    binding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.TripleDes;
        //    binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
        //}
        //else
        //    binding.Security.Mode = SecurityMode.None;

        NetTcpBinding binding = new NetTcpBinding();
        if (secure)
        {
            binding.Security.Mode = SecurityMode.Message;
            binding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.TripleDes;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
        }
        else
            binding.Security.Mode = SecurityMode.None;
      binding.MaxBufferPoolSize = Int32.MaxValue;
      binding.MaxBufferSize = Int32.MaxValue;
      binding.MaxReceivedMessageSize = Int32.MaxValue;
      binding.MaxConnections = 500;
      binding.ListenBacklog = 500;

      XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
      quotas.MaxArrayLength = Int32.MaxValue;
      quotas.MaxBytesPerRead = Int32.MaxValue;
      quotas.MaxDepth = Int32.MaxValue;
      quotas.MaxNameTableCharCount = Int32.MaxValue;
      quotas.MaxStringContentLength = Int32.MaxValue;
      binding.ReaderQuotas = quotas;
      binding.CloseTimeout = TimeSpan.MaxValue;
      binding.OpenTimeout = TimeSpan.MaxValue;
      binding.ReceiveTimeout = TimeSpan.MaxValue;
      binding.SendTimeout = TimeSpan.MaxValue;
      ChannelFactory<ITeste> channelFactory = new ChannelFactory<ITeste>(binding, string.Format("net.tcp://{0}:{1}/{2}", ServerName, Port, "TesteServer"));
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
