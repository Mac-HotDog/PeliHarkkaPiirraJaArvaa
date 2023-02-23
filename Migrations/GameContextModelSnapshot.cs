﻿// <auto-generated />
using System;
using Kuvarpa;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kuvarpa.Migrations
{
    [DbContext(typeof(GameContext))]
    partial class GameContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.14");

            modelBuilder.Entity("Kuvarpa.Player", b =>
                {
                    b.Property<int>("PlayerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConnectionId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDrawer")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayerName")
                        .HasColumnType("TEXT");

                    b.Property<int>("Points")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("RoomId")
                        .HasColumnType("INTEGER");

                    b.HasKey("PlayerId");

                    b.HasIndex("RoomId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("Kuvarpa.Room", b =>
                {
                    b.Property<int>("RoomId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GuessCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RightWordNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RoomName")
                        .HasColumnType("TEXT");

                    b.HasKey("RoomId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("Kuvarpa.Word", b =>
                {
                    b.Property<int>("WordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.HasKey("WordId");

                    b.ToTable("Words");

                    b.HasData(
                        new
                        {
                            WordId = 1,
                            Content = "banaani"
                        },
                        new
                        {
                            WordId = 2,
                            Content = "omena"
                        },
                        new
                        {
                            WordId = 3,
                            Content = "talo"
                        });
                });

            modelBuilder.Entity("Kuvarpa.Player", b =>
                {
                    b.HasOne("Kuvarpa.Room", "Room")
                        .WithMany("Players")
                        .HasForeignKey("RoomId");

                    b.Navigation("Room");
                });

            modelBuilder.Entity("Kuvarpa.Room", b =>
                {
                    b.Navigation("Players");
                });
#pragma warning restore 612, 618
        }
    }
}
