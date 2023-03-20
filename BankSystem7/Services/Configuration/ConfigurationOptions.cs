﻿
namespace BankSystem7.Services;

public class ConfigurationOptions
{
    public bool EnsureDeleted { get; set; }
    public bool EnsureCreated { get; set; } = true;
    public string? Connection { get; set; } = null;
    public string? DatabaseName { get; set; } = null;
}