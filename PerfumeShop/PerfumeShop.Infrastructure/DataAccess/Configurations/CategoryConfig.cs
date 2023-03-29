﻿namespace PerfumeShop.Infrastructure.DataAccess.Configurations;

public sealed class CategoryConfig : IEntityTypeConfiguration<CatalogCategory>
{
    public void Configure(EntityTypeBuilder<CatalogCategory> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
           .UseHiLo("category_hilo")
           .IsRequired();

        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);
    }
}
