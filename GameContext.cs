using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Kuvarpa
{
    public class GameContext : DbContext
    {
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<Player> Players { get; set; }

        public string DbPath { get; }

        // Tämä lokaalia kehitystä varten, muuta azurea varten
        public GameContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "game.db");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Word>().HasData(
                new { WordId =  1, Content = "banaani" },
                new { WordId = 2, Content = "omena" },
                new { WordId = 3, Content = "talo" }
                );
        }
    }

    public class Room
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        
        public List<Player> Players { get; } = new();
        public int RightWordNumber { get; set; }
        public int GuessCount { get; set; }
    }
    public class Word
    {
        public int WordId { get; set; }
        public string Content { get;  set; }

    }

    public class Player
    {
        public int PlayerId { get; set; }
        public string ConnectionId { get; set; }
        public string PlayerName { get; set; }
        public bool IsDrawer { get; set; }
        public int Points { get; set; }
        public Room Room { get; set; }
    }
}
