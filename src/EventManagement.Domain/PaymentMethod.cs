using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace losol.EventManagement.Domain
{
	public class PaymentMethod
	{
		public int PaymentMethodId { get; set; }

		public string Code { get; set; }

		[Required]
		[StringLength(75)]
		[Display(Name = "Navn på betalingsmetode")]
		public string Name { get; set; }
		public string PaymentProvider { get; set; }

		public bool Active { get; set; } = false;
		public bool AdminOnly { get; set; } = false;
	}
}
