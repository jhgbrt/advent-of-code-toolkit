﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Net.Code.AdventOfCode.Toolkit.Data;

#nullable disable

namespace Net.Code.AdventOfCode.Toolkit.Migrations
{
    [DbContext(typeof(IAoCDbContext))]
    partial class AoCDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.2");

            modelBuilder.Entity("Net.Code.AdventOfCode.Toolkit.Core.DayResult", b =>
                {
                    b.Property<int>("Key")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Day")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("INTEGER")
                        .HasComputedColumnSql("Key%100");

                    b.Property<long>("Elapsed")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("INTEGER")
                        .HasComputedColumnSql("Part1_Elapsed + Part2_Elapsed");

                    b.Property<int>("Year")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("INTEGER")
                        .HasComputedColumnSql("Key/100");

                    b.HasKey("Key");

                    b.HasIndex("Year", "Day");

                    b.ToTable("Results");
                });

            modelBuilder.Entity("Net.Code.AdventOfCode.Toolkit.Core.Puzzle", b =>
                {
                    b.Property<int>("Key")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Day")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("INTEGER")
                        .HasComputedColumnSql("Key%100");

                    b.Property<string>("Input")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Year")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("INTEGER")
                        .HasComputedColumnSql("Key/100");

                    b.HasKey("Key");

                    b.HasIndex("Year", "Day");

                    b.ToTable("Puzzles");
                });

            modelBuilder.Entity("Net.Code.AdventOfCode.Toolkit.Core.DayResult", b =>
                {
                    b.OwnsOne("Net.Code.AdventOfCode.Toolkit.Core.Result", "Part1", b1 =>
                        {
                            b1.Property<int>("DayResultKey")
                                .HasColumnType("INTEGER");

                            b1.Property<long>("Elapsed")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("Status")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Value")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("DayResultKey");

                            b1.ToTable("Results");

                            b1.WithOwner()
                                .HasForeignKey("DayResultKey");
                        });

                    b.OwnsOne("Net.Code.AdventOfCode.Toolkit.Core.Result", "Part2", b1 =>
                        {
                            b1.Property<int>("DayResultKey")
                                .HasColumnType("INTEGER");

                            b1.Property<long>("Elapsed")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("Status")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Value")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("DayResultKey");

                            b1.ToTable("Results");

                            b1.WithOwner()
                                .HasForeignKey("DayResultKey");
                        });

                    b.Navigation("Part1")
                        .IsRequired();

                    b.Navigation("Part2")
                        .IsRequired();
                });

            modelBuilder.Entity("Net.Code.AdventOfCode.Toolkit.Core.Puzzle", b =>
                {
                    b.OwnsOne("Net.Code.AdventOfCode.Toolkit.Core.Answer", "Answer", b1 =>
                        {
                            b1.Property<int>("PuzzleKey")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("part1")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("part2")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("PuzzleKey");

                            b1.ToTable("Puzzles");

                            b1.WithOwner()
                                .HasForeignKey("PuzzleKey");
                        });

                    b.Navigation("Answer")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
