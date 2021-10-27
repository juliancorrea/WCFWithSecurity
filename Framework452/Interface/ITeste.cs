using System;
using System.Data;
using System.ServiceModel;

namespace Interface
{
  [ServiceContract()]
  public interface ITeste
  {
    [OperationContract()]
    string GetString();
  }
}
