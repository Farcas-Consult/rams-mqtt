using System;
using System.Linq;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence
{
    public static class DbInitializer
    {
        public static void Initialize(ZebraDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any equipments.
            if (context.Equipments.Any(e => e.Name == "fx9600fd776c"))
            {
                return;   // Reader has been seeded
            }

            var reader = new Equipment
            {
                Name = "fx9600fd776c",
                Description = "Dock Door Reader",
                IsMobile = false,
                IsOnline = false,
                // You can set other default properties here if needed
                LastHeartbeat = DateTime.UtcNow
            };

            context.Equipments.Add(reader);
            context.SaveChanges();
        }
    }
}
