using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Interface;

namespace Server
{
  public class Teste : ITeste
  {
    public string GetString()
    {
      return "Texto de teste";
    }
  }
}
