using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.Models
{
    /// Extends IdentityUser to add custom user properties while keeping all Identity features
    public class ApplicationUser : IdentityUser
    {
        // Custom properties for the e-commerce domain
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Navigation properties - Entity Framework will use these to create relationships
        // One user can have one shopping cart
        public ShoppingCart? ShoppingCart { get; set; }

        // One user can have many orders
        public ICollection<Order> Orders { get; set; } = new List<Order>();

    }
}