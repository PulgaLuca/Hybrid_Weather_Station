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

using CsvHelper;
using Newtonsoft.Json;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MeteoRaspberry
{
    class raspberry
    {
        #region Communication client-server.
        // incoming data from client.
        public static string data;

        /// <summary>
        ///  Starting communication between cliente and server
        /// </summary>
        public static void StartServer()
        {
            // Data buffer for incoming data.
            byte[] bytes;

            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 2222); // socket con ip client e porta

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint); // binding del client
                listener.Listen(10); // Ascolta

                while (true)
                {
                    Console.WriteLine("\n\n--------------CONNECTION---------------------");
                    Console.WriteLine("\nSERVER: waiting for a connection...");
                    Socket handler = listener.Accept();
                    data = null;

                    while (true)
                    {
                        bytes = new byte[102400]; // byte da inviare
                        int bytesRec = handler.Receive(bytes); // SERVER RICEVE
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        break;
                    }

                    Console.WriteLine("2. RICHIESTA RICEVUTA\n");

                    byte[] msg = Encoding.ASCII.GetBytes(Serialize(ReadingFromCsv(data))); // SERVER INVIA LA STRINGA DEL FILE JSON

                    handler.Send(msg); // send dei dati, serializzati anche in json
                    Console.WriteLine("3. RISPOSTA INVIATA");
                    handler.Shutdown(SocketShutdown.Both); 
                    handler.Close(); // chiude la connessione
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadKey();
        }
        #endregion

        #region Deserialize data request.
        /// <summary>
        /// Deserializzazione dei dati provenienti dal json client.
        /// </summary>
        /// <param name="rec">stringa da deserializzare proveniente dal json</param>
        /// <returns>Oggetto della richiesta client</returns>
        static DateRequest Deserialize(string rec)
        {
            DateRequest request = JsonConvert.DeserializeObject<DateRequest>(rec);
            Console.WriteLine("\n--------------DESERIALIZE---------------------");
            Console.WriteLine($"Beginning date & hour: {request.beginningDateTime}");
            Console.WriteLine($"Expiring date & hour: {request.expiredDateTime}");
            Console.WriteLine("--------------DESERIALIZE---------------------\n");
            return request;
        }
        #endregion

        #region Serialize data response.
        /// <summary>
        /// Serialize data response dei dati da inviare al client.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        static public string Serialize(List<DataResponse> dateTime)
        {
            Console.WriteLine("\nSERIALIZING...\n");

            string jsonSerialized = JsonConvert.SerializeObject(dateTime, Formatting.Indented); // Serializzazione di tutti i rilevamenti

            string pathOfJson = @"../../jsonresponse.json";
            //export data to json file. 
            using (TextWriter tw = new StreamWriter(pathOfJson))
            {
                tw.WriteLine(jsonSerialized);
            };

            return jsonSerialized;
        }
        #endregion

        #region Lettura dei dati csv.
        /// <summary>
        /// Lettura dei dati dal csv.
        /// </summary>
        /// <param name="s">stringa da deserializzare</param>
        /// <returns>Lista di oggetti delle rilevazioni provenienti dal csv</returns>
        public static List<DataResponse> ReadingFromCsv(string s)
        {
            string filename;
            Dictionary<string, List<DataResponse>> keyValuePairs = new Dictionary<string, List<DataResponse>>(); // key=Data, values = rilevamenti all'interno del file fornito nella key
            List<DataResponse> rilevamentiPerGiornata = new List<DataResponse>(); // rilevamenti in una singola giornata
            List<DataResponse> rilevamentiDaInviare = new List<DataResponse>(); //rilevazioni da inviare
            List<string> allFiles = GetFilesFromPath(@"../../../docs/");
            string PureFilename = "";
            
            DateRequest d = Deserialize(s); // Deserializzazione della richiesta dell'utente.
            Console.WriteLine("------ FILE TROVATI -----");
            foreach (string f in allFiles)
            {
                Console.WriteLine(f);
                filename = Path.GetFileName(f);   // ottengo il nome del file 2022-02-02.csv
                PureFilename = filename.Replace(".csv", "");     // ottengo la data pura senza .csv
                DateTime myDatePickedFromCsv = DateTime.ParseExact(PureFilename, "yyyy-MM-dd", CultureInfo.InvariantCulture);  // converto la data stringa in Datetime
                rilevamentiPerGiornata = File.ReadAllLines(f).Skip(0).Select(v => DataResponse.FromCsv(v)).ToList(); // leggo tutti i rilevamenti del giorno.
                keyValuePairs.Add(PureFilename, rilevamentiPerGiornata);  // leggo tutti i valori del file, trasformo in oggetto e aggiungo alla dictionary
            }
            Console.WriteLine("------ FILE TROVATI -----");

            foreach (string f in allFiles)// guardo tutti i file presenti letti precedentemente dalla folder
            {
                string i = f.Substring(14, f.Length-18); // taglio data
                DateTime curr = new DateTime(int.Parse(i.Split('-')[0]), int.Parse(i.Split('-')[1]), int.Parse(i.Split('-')[2]));
                DateTime dataOdierna = DateTime.ParseExact("0001-01-01 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);

                if (curr == d.beginningDateTime.Date && curr == d.expiredDateTime.Date) // Se la data di inizio e quella di fine sono uguali.
                {
                    foreach (DataResponse r in keyValuePairs[i]) // Ogni record, (rilevamento), nel file specificato
                    {
                        if (r.ora >= d.beginningDateTime.TimeOfDay && r.ora <= d.expiredDateTime.TimeOfDay) // prendo rilevazioni fatte dopo ora specificata
                            rilevamentiDaInviare.Add(r);
                    }
                }
                else if (d.beginningDateTime == dataOdierna) // fornisce l'ultimo rilevamento.
                {
                    filename = Path.GetFileName(allFiles.Last());
                    PureFilename = filename.Replace(".csv", "");
                    rilevamentiDaInviare.Add(rilevamentiPerGiornata.LastOrDefault());
                    return rilevamentiDaInviare;
                }
                else if (curr == d.beginningDateTime.Date) //primo giorno richiesto di rilevazioni
                {
                    foreach (DataResponse r in keyValuePairs[i])
                    {
                        if (r.ora >= d.beginningDateTime.TimeOfDay) //prendo rilevazioni effettuate after l'ora specificata dal client
                            rilevamentiDaInviare.Add(r);
                    }
                }
                else if (curr == d.expiredDateTime.Date) //ultimo giorno richiesto di rilevazioni
                {
                    foreach (DataResponse r in keyValuePairs[i])
                    {
                        if (r.ora <= d.expiredDateTime.TimeOfDay) //prendo rilevazioni fatte prima dell'ora specificata
                            rilevamentiDaInviare.Add(r);
                    }
                }
                else if (curr > d.beginningDateTime.Date && curr < d.expiredDateTime.Date) // rilevazioni between le due date
                {
                    rilevamentiDaInviare.AddRange(keyValuePairs[i]); // prendo tutte le rilevazioni.
                }
            }

            return rilevamentiDaInviare; // valori da inviare al client
        }
        #endregion

        #region File da leggere.
        /// <summary>
        /// Rilevazione dei file da leggere.
        /// </summary>
        /// <param name="path">path contenenti i file csv da leggere</param>
        /// <returns>lista di file da leggere</returns>
        static List<string> GetFilesFromPath(string path)
        {
            List<string> files = new List<string>();
            try
            {
                files.AddRange(Directory.GetFiles(path)); // aggiunge tutti i file presenti nella path fornita

                foreach (string dir in Directory.GetDirectories(path))
                {
                    string filename = Path.GetFileName(dir);
                    files.AddRange(GetFilesFromPath(dir));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRORE NELLA LETTURA DEI FILE DALLA PATH\n" + ex.Message);
            }
            return files;
        }
        #endregion

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            StartServer();
        }
    }
}
