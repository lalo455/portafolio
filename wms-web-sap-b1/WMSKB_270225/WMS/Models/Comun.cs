using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Comun
    {
        public class Parametros {
            public string field { get; set; }
            public dynamic value { get; set; }
            public string symbol { get; set; }
        }

        public class Condiciones {
            public string cdnString { get; set; }
            public string cdnNumbers { get; set; }

            public Condiciones(string _num, string _str)
            {
                cdnNumbers = _num;
                cdnString = _str;
            }
        }

        public class Location {
            public int AbsEntry { get; set; }
            public string BinCode { get; set; }

            public Location() {
                AbsEntry = 0;
                BinCode = "";
            }
        }

    }
}