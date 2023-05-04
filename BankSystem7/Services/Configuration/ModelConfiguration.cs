using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Configuration;

public abstract class ModelConfiguration
{
    public bool InitializeAccess { get; }

    protected ModelConfiguration()
    {
    }

    protected ModelConfiguration(bool initializeAccess)
    {
        InitializeAccess = initializeAccess;
    }
    public abstract void Invoke(ModelBuilder modelBuilder);
}