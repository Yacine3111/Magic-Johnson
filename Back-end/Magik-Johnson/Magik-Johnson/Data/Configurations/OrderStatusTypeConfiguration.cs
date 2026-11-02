using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Magik_Johnson.Data.Configurations;

public class OrderStatusTypeConfiguration : IEntityTypeConfiguration<OrderStatus>
{
    //IMPORTANT : NE PAS CHANGER LES IDs
    public void Configure(EntityTypeBuilder<OrderStatus> builder)
    {
        builder.HasData(
            new OrderStatus
            {
                Id = Guid.Parse("d8c70d58-a557-4ea2-9301-5bd6626eb585"),
                Name = "En attente",
            },
            new OrderStatus
            {
                Id = Guid.Parse("ee4bb5b8-94b0-4bfe-8c59-34a4c35ee052"),
                Name = "En préparation",
            },
            new OrderStatus
            {
                Id = Guid.Parse("2a8a04f8-ea2f-4669-949b-191940b9f489"),
                Name = "Expédiée",
            },
            new OrderStatus
            {
                Id = Guid.Parse("7fe741db-ec17-47f8-8a8f-548743984961"),
                Name = "Livrée",
            },
            new OrderStatus
            {
                Id = Guid.Parse("69d34e1a-1b99-4b56-aa53-da04da87940d"),
                Name = "Annulée",
            },
            new OrderStatus
            {
                Id = Guid.Parse("6f6a895d-67f0-4da5-a873-63588d7a1c73"),
                Name = "Remboursée",
            },
            new OrderStatus
            {
                Id = Guid.Parse("02792325-3b41-41e8-a1e8-a8f679b0d124"),
                Name = "Refusée",
            },
            new OrderStatus
            {
                Id = Guid.Parse("353fc37f-8ddc-49f1-80e8-338028896c4b"),
                Name = "Partiellement remboursée",
            }
        );
    }
}
