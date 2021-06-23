using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyTestApp5536.Models;

namespace MyTestApp5536.Data
{
    public class MyTestApp5536Context : DbContext
    {
        public MyTestApp5536Context (DbContextOptions<MyTestApp5536Context> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<MyTestApp5536.Models.TestModel> TestModel { get; set; }
    }
}
