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

namespace Server
{
  class Program
  {
    static void Main(string[] args)
    {
      //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
      Console.WriteLine(ServicePointManager.SecurityProtocol.ToString());
      RegisterServers(10000, true);
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
        string url = string.Format("net.tcp://127.0.0.1:{0}/{1}", port, "TesteServer");
        sh = new ServiceHost(typeof(Teste));
        ((ServiceDebugBehavior)sh.Description.Behaviors[typeof(ServiceDebugBehavior)]).IncludeExceptionDetailInFaults = true;
        sh.OpenTimeout = TimeSpan.MaxValue;
        sh.CloseTimeout = TimeSpan.MaxValue;
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
        tcp.MaxReceivedMessageSize = Int32.MaxValue;
        tcp.TransactionFlow = true;
        tcp.TransferMode = TransferMode.Buffered;
        tcp.MaxConnections = 500;
        tcp.ListenBacklog = 500;
        tcp.ReceiveTimeout = TimeSpan.MaxValue;
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
        ServiceEndpoint sed = sh.AddServiceEndpoint(typeof(ITeste), tcp, url);

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
        throw new Exception(string.Format("Não foi possível iniciar o serviço. {0}", e.Message));
      }
    }

    public static void UnregisterServers()
    {
      sh.Close();
    }
  }
}
