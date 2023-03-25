﻿using eAndon_MVC.Models;
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public MyDbContext()
    {

    }

    public DbSet<Workcenter> WorkcenterList { get; set; }
    public DbSet<StatusDefinition> StatusDefinition { get; set; }
    public DbSet<AndonLog> AndonLog { get; set; }
    public DbSet<Localization> Localization { get; set; }
    public DbSet<Settings> Settings { get; set; }
}

