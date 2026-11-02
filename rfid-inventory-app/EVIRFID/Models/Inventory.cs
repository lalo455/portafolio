using System;
using System.Collections.Generic;

namespace EVIRFID.Models
{
	public class InventoryRecord
	{
		public int idPersona { get; set; }
		public string nombre { get; set; }
		public List<Item> items {get;set;}
		//public Inventory()
		//{
		//}
	}
}

