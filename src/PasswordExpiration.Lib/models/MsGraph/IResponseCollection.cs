using System;
using System.Collections.Generic;

namespace PasswordExpiration.Lib.Models.Graph
{
    public interface IResponseCollection<T>
    {
        string OdataContext { get; set; }
        string OdataNextLink { get; set; }
        List<T> Value { get; set; }
    }
}