using System.Collections.Generic;

namespace BlazorFluentValidation.Models
{
    public class Order
    {
        public string Name { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}
