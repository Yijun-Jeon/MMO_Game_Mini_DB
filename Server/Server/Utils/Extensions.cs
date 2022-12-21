using Server.DB;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class Extensions
    {
        // 일반적인 Extension 메소드의 문법
        public static bool SaveChangesEx(this AppDbContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool SaveChangesEx(this SharedDbContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
