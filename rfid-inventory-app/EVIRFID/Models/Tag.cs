using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace EVIRFID.Models
{
    public class Tag
    {
        [AutoIncrement, PrimaryKey]

        public string epc { get; set; }
   
    }
}
