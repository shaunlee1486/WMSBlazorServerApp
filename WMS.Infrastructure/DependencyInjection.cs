using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Entities.Identity;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.Infrastructure.Identity;
using WMS.Infrastructure.Persistence;
using WMS.Infrastructure.Persistence.Repositories;
using WMS.Infrastructure.Services;

namespace WMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") 
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(EFRepository<>));
        services.AddScoped<IStockRepository, EFStockRepository>();
        services.AddScoped<IStockMovementRepository, EFStockMovementRepository>();
        services.AddScoped<IStockAdjustmentRepository, EFStockAdjustmentRepository>();
        services.AddScoped<IPurchaseOrderRepository, EFPurchaseOrderRepository>();
        services.AddScoped<IGoodsReceiptRepository, EFGoodsReceiptRepository>();
        services.AddScoped<ISalesOrderRepository, EFSalesOrderRepository>();
        services.AddScoped<IPickListRepository, EFPickListRepository>();
        services.AddScoped<IGoodsIssueRepository, EFGoodsIssueRepository>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}
