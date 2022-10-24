using System;
using System.Collections.Generic;
using Dapper;

namespace DailySqlRestoreRunner
{
    class Program
    {
        public class TableDetail
        {
            public string Database { get; set; }
            public string RestoredBy { get; set; }
            public string RestoreType { get; set; }
            public DateTime RestoreStarted { get; set; }
            public string RestoredFrom { get; set; }
            public string RestoredTo { get; set; }
        }
        static void Main()
        {
            List<string> firstList = new List<string>() {"'TestDb1'", "'TestDb2'", "'TestDb3'" };
            List<string> secondList = new List<string>() { "'TestDb4'", "'TestDb5'" };
            PrimaryQueries(firstList);
            SecondaryQueries(secondList);
            Console.ReadKey();
        }

        public static void PrimaryQueries(List<string> firstList)
        {
            firstList.ForEach(s =>
            {
                var sql = $@"SELECT TOP 5
                            rsh.destination_database_name AS [Database],
                            rsh.user_name AS [RestoredBy],
                            CASE
                                WHEN
                                    rsh.restore_type = 'D' THEN 'Database'
                                WHEN
                                    rsh.restore_type = 'F' THEN 'File'
                                WHEN
                                    rsh.restore_type = 'G' THEN 'Filegroup'
                                WHEN
                                    rsh.restore_type = 'I' THEN 'Differential'
                                WHEN
                                    rsh.restore_type = 'L' THEN 'Log'
                                WHEN
                                    rsh.restore_type = 'V' THEN 'Verifyonly'
                                WHEN
                                    rsh.restore_type = 'R' THEN 'Revert'
                                ELSE
                                    rsh.restore_type
                            END AS [RestoreType],
                            rsh.restore_date AS [RestoreStarted],
                            bmf.physical_device_name AS [RestoredFrom],
                            rf.destination_phys_name AS [RestoredTo]
                            FROM
                            msdb.dbo.restorehistory rsh
                            INNER JOIN
                            msdb.dbo.backupset bs ON rsh.backup_set_id = bs.backup_set_id
                            INNER JOIN
                            msdb.dbo.restorefile rf ON rsh.restore_history_id = rf.restore_history_id
                            INNER JOIN
                            msdb.dbo.backupmediafamily bmf ON bmf.media_set_id = bs.media_set_id
                            where
                            destination_database_name = {s} and restore_type = 'L'
                            ORDER BY rsh.restore_date DESC";
                using (var conn = ConnectToDB.DefaultConnection())
                {
                    var results = conn.Query<TableDetail>(sql);
                    foreach (var result in results)
                    {
                        var dayDiff = (DateTime.Now.Date - result.RestoreStarted.Date).Days;
                        if (dayDiff >= 1)
                        {
                            Console.WriteLine($"Day difference for TLS greater than 1 day.  DB in question: {result.Database}");
                        }
                    }
                }
            });
        }
        public static void SecondaryQueries(List<string> secondaryList)
        {
            secondaryList.ForEach(x =>
            {
                var sql = $@"SELECT TOP 5
                            rsh.destination_database_name AS [Database],
                            rsh.user_name AS [RestoredBy],
                            CASE
                                WHEN
                                    rsh.restore_type = 'D' THEN 'Database'
                                WHEN
                                    rsh.restore_type = 'F' THEN 'File'
                                WHEN
                                    rsh.restore_type = 'G' THEN 'Filegroup'
                                WHEN
                                    rsh.restore_type = 'I' THEN 'Differential'
                                WHEN
                                    rsh.restore_type = 'L' THEN 'Log'
                                WHEN
                                    rsh.restore_type = 'V' THEN 'Verifyonly'
                                WHEN
                                    rsh.restore_type = 'R' THEN 'Revert'
                                ELSE
                                    rsh.restore_type
                            END AS [RestoreType],
                            rsh.restore_date AS [RestoreStarted],
                            bmf.physical_device_name AS [RestoredFrom],
                            rf.destination_phys_name AS [RestoredTo]
                            FROM
                            msdb.dbo.restorehistory rsh
                            INNER JOIN
                            msdb.dbo.backupset bs ON rsh.backup_set_id = bs.backup_set_id
                            INNER JOIN
                            msdb.dbo.restorefile rf ON rsh.restore_history_id = rf.restore_history_id
                            INNER JOIN
                            msdb.dbo.backupmediafamily bmf ON bmf.media_set_id = bs.media_set_id
                            where
                            destination_database_name = {x} and restore_type = 'L'
                            ORDER BY rsh.restore_date DESC";
                using (var conn = ConnectToDB.OtherConnection())
                {
                    var results = conn.Query<TableDetail>(sql);
                    foreach (var result in results)
                    {
                        var dayDiff = (DateTime.Now.Date - result.RestoreStarted.Date).Days;
                        if (dayDiff >= 1)
                        {
                            Console.WriteLine($"Day difference for TLS greater than 1 day.  DB in question: {result.Database}");
                        }
                    }
                }
            });
        }
    }
}
