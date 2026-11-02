using Magik_Johnson.Data.Configurations;
using Magik_Johnson.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Type = Magik_Johnson.Models.Type;

namespace Magik_Johnson.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        new ApplicationUserTypeConfiguration().Configure(modelBuilder.Entity<ApplicationUser>());

        //cartes
        new SetEntityTypeConfiguration().Configure(modelBuilder.Entity<Set>());

        new ArtistTypeConfiguration().Configure(modelBuilder.Entity<Artist>());

        new CardTypeConfiguration().Configure(modelBuilder.Entity<Card>());

        new LanguageTypeConfiguration().Configure(modelBuilder.Entity<Language>());

        new TypeTypeConfiguration().Configure(modelBuilder.Entity<Type>());

        new SubTypeTypeConfiguration().Configure(modelBuilder.Entity<SubType>());

        new CardTypeTypeConfiguration().Configure(modelBuilder.Entity<CardType>());

        new ArtistCardTypeConfiguration().Configure(modelBuilder.Entity<ArtistCard>());

        new CardSubTypeTypeConfiguration().Configure(modelBuilder.Entity<CardSubType>());

        new DiscountTypeConfiguration().Configure(modelBuilder.Entity<Discount>());

        //adresses
        new AddressTypeConfiguration().Configure(modelBuilder.Entity<Address>());

        new ProvinceTypeConfiguration().Configure(modelBuilder.Entity<Province>());

        new CountryTypeConfiguration().Configure(modelBuilder.Entity<Country>());
        new QuantityTypeConfiguration().Configure(modelBuilder.Entity<Quantity>());

        //magasin
        new CartTypeConfiguration().Configure(modelBuilder.Entity<Cart>());

        new CardCartTypeConfiguration().Configure(modelBuilder.Entity<CardCart>());

        //new StoreTypeConfiguration().Configure(modelBuilder.Entity<Store>());

        //Commandes
        new OrderStatusTypeConfiguration().Configure(modelBuilder.Entity<OrderStatus>());
        new OrderTypeConfiguration().Configure(modelBuilder.Entity<Order>());
        new CardOrderTypeConfiguration().Configure(modelBuilder.Entity<CardOrder>());
        new PendingPaymentTypeConfiguration().Configure(modelBuilder.Entity<PendingPayment>());

        new RefundTypeConfiguration().Configure(modelBuilder.Entity<Refund>());
    }

    //cartes
    public DbSet<Set> Sets { get; set; }

    public DbSet<Artist> Artists { get; set; }

    public DbSet<Card> Cards { get; set; }

    public DbSet<Language> Languages { get; set; }

    public DbSet<Type> Types { get; set; }

    public DbSet<SubType> SubTypes { get; set; }

    public DbSet<CardType> CardTypes { get; set; }

    public DbSet<ArtistCard> ArtistsCards { get; set; }

    public DbSet<CardSubType> CardSubTypes { get; set; }

    public DbSet<Quantity> Quantities { get; set; }
    public DbSet<Discount> Discounts { get; set; }

    //adresses
    public DbSet<Address> Address { get; set; }

    public DbSet<Province> Provinces { get; set; }

    public DbSet<Country> Countries { get; set; }

    //magasin
    public DbSet<Cart> Carts { get; set; }

    public DbSet<CardCart> CardsCarts { get; set; }

    //Commandes
    public DbSet<OrderStatus> OrderStatus { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<CardOrder> CardOrders { get; set; }
    public DbSet<Refund> Refunds { get; set; }
    public DbSet<PendingPayment> PendingPayments { get; set; }
}
