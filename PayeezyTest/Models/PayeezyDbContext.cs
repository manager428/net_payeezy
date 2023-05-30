using Microsoft.EntityFrameworkCore;

namespace PayeezyTest.Models
{
    public class PayeezyDbContext : DbContext
    {
        public PayeezyDbContext()
        {
        }

        public PayeezyDbContext(DbContextOptions<PayeezyDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PatientCoPay> PatientCoPay { get; set; }
    }
}
