/*
 * Developer: Pulga Luca
 * Class: 5^L
 * Start date: 
 * End date:
 * Scope: set up a client-server socket, send request json to server with beginning and expiring dates, and server will response with json data.
 *        In the project folder, fom more informatioc look at:
 *      - Esercizio_04-03.2022.docx
 *      - Esercizio di comunicazione in rete per dati meteo.docx
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using MySql.Data.MySqlClient;

namespace SharedClasses
{
    class businessLayer
    {
        static public string connectionString { get; set; }
        static private MySqlConnection mysqlconnection { get; set; }

        public businessLayer(string c)
        {
            connectionString = c;
        }

        public businessLayer(string connString, MySqlConnection mySqlConn)
        {
            connectionString = connString;
            mysqlconnection = mySqlConn;
        }

        public void CreatingDB(string dbname)
        {
            mysqlconnection = new MySqlConnection(connectionString);

            mysqlconnection.Open();
            
            MySqlCommand cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS {dbname}", mysqlconnection);
            cmd.ExecuteNonQuery();

            Console.WriteLine($"Database '{dbname}' created successfully");
            mysqlconnection.Close();
        }

        public void CreateTables(string nomeTabella)
        {
            try
            {
                mysqlconnection.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = mysqlconnection;
                cmd.CommandText = "USE serverdb";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {nomeTabella}(id INT PRIMARY KEY, numRilevazione INT, ora VARCHAR(30), intensitaVento VARCHAR(5), direzioneVento VARCHAR(5), pressione VARCHAR(5), temperatura VARCHAR(5), umidita VARCHAR(5), pluviometro VARCHAR(5))";
                cmd.ExecuteNonQuery();

                Console.WriteLine("Table rilevamenti created");
                mysqlconnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\tCREATING DATABASE ERROR: " + ex.Message);
            }
        }

        public void Insert(int id, int n, string o, string t, string u, string dv, string iv, string pr, string pl)
        {
            mysqlconnection.Open();
            var cmd = new MySqlCommand();
            cmd.Connection = mysqlconnection;

            cmd.CommandText = $"INSERT INTO rilevamenti(id, numRilevazione, ora, intensitaVento, direzioneVento, pressione, temperatura, umidita, pluviometro) VALUES({id}, {n},'{o}','{t}','{u}','{dv}','{iv}','{pr}','{pl}')";
            cmd.ExecuteNonQuery();  

            mysqlconnection.Close();
        }


        public void SelectAll()
        {
            mysqlconnection.Open();
            string sql = "SELECT * FROM rilevamenti";
            var cmd = new MySqlCommand(sql, mysqlconnection);

            MySqlDataReader rdr = cmd.ExecuteReader();
            Console.WriteLine("-----------   SELECT ALL DATA FROM SERVERDB   -----------");
            while (rdr.Read())
            {
                Console.WriteLine(" id: {0}\n n. rilevazione: {1}\n ora: {2}\n intensita vento: {3}\n direzione vento: {4}\n pressione: {5}\n temperatura: {6}\n umidita: {7}\n pluviometro: {8}\n", rdr.GetInt32(0), rdr.GetInt32(1), rdr.GetString(2), rdr.GetString(3), rdr.GetString(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7), rdr.GetString(8));
            }
            Console.WriteLine("-----------   SELECT ALL DATA FROM SERVERDB   -----------\n\n");
            mysqlconnection.Close();
        }


        public void DropTables(string[] tablesNames)
        {
            var cmd = new MySqlCommand();
            cmd.Connection = mysqlconnection;

            foreach (string tableName in tablesNames)
            {
                cmd.CommandText = $"DROP TABLE IF EXISTS {tableName}";
                cmd.ExecuteNonQuery();
            }
            Console.WriteLine($"DROPPED {tablesNames}");

            mysqlconnection.Close();
        }

        public void DropDB(string dbName)
        {
            var cmd = new MySqlCommand();
            cmd.Connection = mysqlconnection;

            cmd.CommandText = $"DROP DATABASE IF EXISTS {dbName}";
            cmd.ExecuteNonQuery();

            mysqlconnection.Close();
        }

        public void CloseDB()
        {
            try
            {
                if (mysqlconnection.State == ConnectionState.Open)
                {
                    Console.WriteLine($"\tCLOSING DATABASE...");
                    mysqlconnection.Close();
                    Console.ReadLine();
                }
                mysqlconnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\tOPENING & CONNECTING DATABASE ERROR: " + ex.Message);
            }
        }
    }
}
