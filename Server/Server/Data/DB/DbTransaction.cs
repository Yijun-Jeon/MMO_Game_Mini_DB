﻿using Microsoft.EntityFrameworkCore;
using Server.DB;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data.DB
{
    public class DbTransaction : JobSerializer
    {
        // SingleTon
        public static DbTransaction Instance { get; } = new DbTransaction();

        // Me (GameRoom) -> You (DB) -> Me (GameRoom)
        // lambda 방식
        public static void SavePlayerStatus_AllInOne(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();

                    if (success)
                    {
                        // Me
                        room.Push(() =>
                        {
                            Console.WriteLine($"Hp Saved({playerDb.Hp})");
                        });
                    }
                }
            });
        }

        // Step by Step 방식
        public static void SavePlayerStatus_Step1(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;

            // Me (GameRoom) -> You (Db)
            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2,playerDb,room);
        }

        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
        {
            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;
                bool success = db.SaveChangesEx();

                // You (Db) -> Me (GameRoom)
                if (success)
                {
                    room.Push(SavePlayerStatus_Step3, playerDb.Hp);
                }
            }
        }

        public static void SavePlayerStatus_Step3(int hp)
        {
            Console.WriteLine($"Hp Saved({hp})");
        }
    }
}
