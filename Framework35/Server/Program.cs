using System;
using System.IO;
using System.Reflection;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;

using Interface;
using System.Configuration;

namespace Server
{
  class Program
  {
      static string ServerName = ConfigurationManager.AppSettings["Server"];
      static int  Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
    static void Main(string[] args)
    {
      Console.WriteLine("Server starting...");
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
      Console.WriteLine(ServicePointManager.SecurityProtocol.ToString());
      RegisterServers(Port, true);
      Console.WriteLine("Serviço inicializado!");
      Console.ReadKey();
      UnregisterServers();
      Console.WriteLine("Serviço finalizado!");
    }

    private static ServiceHost sh = null;

    public static void RegisterServers(int port, bool secure)
    {
      try
      {
          string url = string.Format("net.tcp://{0}:{1}/{2}", ServerName,port, "TesteServer");
        sh = new ServiceHost(typeof(Teste));
        ((ServiceDebugBehavior)sh.Description.Behaviors[typeof(ServiceDebugBehavior)]).IncludeExceptionDetailInFaults = true;
        sh.OpenTimeout = TimeSpan.MaxValue;
        sh.CloseTimeout = TimeSpan.MaxValue;
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
        binding.MaxReceivedMessageSize = Int32.MaxValue;
        binding.TransactionFlow = true;
        binding.TransferMode = TransferMode.Buffered;
        binding.MaxConnections = 500;
        binding.ListenBacklog = 500;
        binding.ReceiveTimeout = TimeSpan.MaxValue;
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
        ServiceEndpoint sed = sh.AddServiceEndpoint(typeof(ITeste), binding, url);

        if (secure)
        {
          sh.Credentials.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectName, "VergaServer");
          EndpointAddress epa = new EndpointAddress(new Uri(url), EndpointIdentity.CreateDnsIdentity("VergaServer"), new System.ServiceModel.Channels.AddressHeader[0]);
          sed.Address = epa;
        }

        ServiceThrottlingBehavior throttleBehavior = new ServiceThrottlingBehavior
        {
          MaxConcurrentCalls = 500,
          MaxConcurrentInstances = 500,
          MaxConcurrentSessions = 500,
        };
        sh.Description.Behaviors.Add(throttleBehavior);

        foreach (OperationDescription op in sed.Contract.Operations)
        {
          DataContractSerializerOperationBehavior dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>() as DataContractSerializerOperationBehavior;
          if (dataContractBehavior != null)
            dataContractBehavior.MaxItemsInObjectGraph = Int32.MaxValue;
        }

        sh.Open();
      }
      catch (Exception e)
      {
          Console.WriteLine(string.Format("Não foi possível iniciar o serviço. {0}", e.Message));

        Exception inner = e.InnerException;
        while (inner != null)
        {
            Console.WriteLine("ERRO INNER: " + inner.Message);
            inner = inner.InnerException;
        }
      }
    }

    public static void UnregisterServers()
    {
      sh.Close();
    }
  }
}
