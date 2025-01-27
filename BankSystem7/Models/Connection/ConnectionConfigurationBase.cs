﻿using BankSystem7.Models.Credentials;
using BankSystem7.Services.Configuration;

namespace BankSystem7.Models.Connection;

public abstract class ConnectionConfigurationBase
{
    /// <summary>
    /// Manages database deletion during initializing library
    /// </summary>
    public bool EnsureDeleted { get; set; }

    /// <summary>
    /// Manages database creation during initializing library
    /// </summary>
    public bool EnsureCreated { get; set; } = true;

    /// <summary>
    /// Takes connection string for database
    /// </summary>
    public string? Connection { get; set; } = null;

    /// <summary>
    /// Takes name of database. If You don't want to change connection details, You can just specify it
    /// </summary>
    public string? DatabaseName { get; set; } = null;

    /// <summary>
    /// Manages type of Database management system that will be used
    /// </summary>
    public DatabaseManagementSystemType DatabaseManagementSystemType { get; init; } = DatabaseManagementSystemType.MicrosoftSqlServer;

    /// <summary>
    /// Takes credentials for establishing connection with database
    /// </summary>
    public CredentialsBase? Credentials { get; set; }
}