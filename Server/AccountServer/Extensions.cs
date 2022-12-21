using AccountServer.DB;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class Extensions
{
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
