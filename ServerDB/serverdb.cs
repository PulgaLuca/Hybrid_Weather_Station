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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SharedClasses;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;
using System.Globalization;
using System.Threading;

namespace ServerDB
{
    class serverdb
    {

        public static string cs = @"server=localhost;userid=root;password=root;";
        public static businessLayer businessLayer = new businessLayer(cs);
        public static List<string> alreadychosen = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public static void StartClient(char ch)
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[102400];

            try
            {
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 2222);

                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine($" Socket connected to: {sender.RemoteEndPoint.ToString()}");

                    if(ch == 'I') // rilevamento scelto da utente
                    {
                        byte[] msg = Encoding.ASCII.GetBytes(Serialize(DateAndTimeValidation()));
                        int bytesSent = sender.Send(msg); // INVIO AL SERVER DEL JSON
                        Console.WriteLine("1. RICHIESTA INVIATA");

                        int byteRec = sender.Receive(bytes); // CLIENT RICEVE
                        string received = Encoding.ASCII.GetString(bytes, 0, byteRec);

                        Deserialize(received); // deserialize del json in ricezione dei dati.
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close(); // chiusura della comunicazione.

                    }
                    else if(ch == 'N') // ultimo rilevamento effettuato.
                    {
                        Console.WriteLine(" Data: " + DateTime.Now.Date.ToShortDateString());
                        Console.WriteLine(" Ora: " + DateTime.Now.TimeOfDay.Hours + ":" + DateTime.Now.TimeOfDay.Minutes + ":" + DateTime.Now.TimeOfDay.Seconds);

                        DateTime dataOdierna = DateTime.ParseExact("0001-01-01 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None); //data standard per ottenere l'ultimo rilevamento.
                        DateRequest d = new DateRequest(dataOdierna, dataOdierna);

                        // Serializzazione e sending dei rilevamenti between le date specificate
                        byte[] msg = Encoding.ASCII.GetBytes(Serialize(d)); 
                        int bytesSent = sender.Send(msg); // INVIO AL SERVER DEL JSON
                        Console.WriteLine("1. RICHIESTA INVIATA");

                        int byteRec = sender.Receive(bytes); // CLIENT RICEVE i rilevamenti.
                        string received = Encoding.ASCII.GetString(bytes, 0, byteRec);

                        Deserialize(received); // Deserializzazione dei rilevamenti ricevuti.
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close(); // chiusura socket.
                    }
                }
                catch (ArgumentException ane)
                {
                    Console.WriteLine("ArgumentException: {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException: {0}", se.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected exception: {0}", ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #region Dates and hours validation.
        /// <summary>
        /// Dates and hours validation.
        /// </summary>
        /// <returns></returns>
        static public DateRequest DateAndTimeValidation()
        {
            DateTime dataInizio = DateTime.Now;
            bool chValidity = false;

            // Data di inizio
            Console.WriteLine("----------------DATA REQUEST------------------");
            while (!chValidity)
            {
                Console.WriteLine("\nDATA FORMAT & HOUR FORMAT: yyyy-MM-dd HH:mm:ss");
                Console.Write("Data di inizio: ");
                chValidity = DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dataInizio);
            }

            //Data fi fine
            DateTime dataFine = DateTime.Now;
            chValidity = false;
            while (!chValidity)
            {
                Console.WriteLine("\nDATA FORMAT & HOUR FORMAT: yyyy-MM-dd HH:mm:ss");
                Console.Write("Data di fine: ");
                chValidity = DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dataFine);
                
                //Controllo se la data di fine non sia precedente a quella di inizio
                int res = DateTime.Compare(dataInizio, dataFine);
                if (res >= 0)
                {
                    Console.WriteLine("La data di fine è precedente o uguale a quella di inizio.\n");
                    chValidity = false;
                }
            }
            Console.WriteLine("----------------DATA REQUEST------------------");

            DateRequest dateRequest = new DateRequest(dataInizio, dataFine); // Date oggetto che saranno poi inviate come richista
            return dateRequest;
        }
        #endregion

        #region Serialize data request.
        /// <summary>
        /// Serialize data request.
        /// </summary>
        /// <param name="dateTime">Data immesse dall'utente</param>
        /// <returns></returns>
        static public string Serialize(DateRequest dateTime)
        {
            Console.WriteLine("\nSERIALIZING...\n");

            string jsonSerialized = JsonConvert.SerializeObject(dateTime, Formatting.Indented);

            string pathOfJson = @"../../jsonrequest.json";
            //export data to json file. 
            using (TextWriter tw = new StreamWriter(pathOfJson))
            {
                tw.WriteLine(jsonSerialized);
            };

            return jsonSerialized;
        }
        #endregion

        #region Deserialize data response.
        /// <summary>
        /// Deserialize data response.
        /// </summary>
        /// <param name="rec"></param>
        private static void Deserialize(string rec)
        {
            IList<DataResponse> JsonList = JsonConvert.DeserializeObject<IList<DataResponse>>(rec); // Deserializzazione del rilevamento.

            if(JsonList.Count != 0)
            {
                // Visualizzazione di ogni proprietà dell'oggetto.
                foreach (var elementOfJson in JsonList)
                {
                    Console.WriteLine("----------------DESERIALIZE-------------------");
                    Console.WriteLine("N. rilevezione: " + elementOfJson.numRilevazione);
                    Console.WriteLine("Ora: " + elementOfJson.ora);
                    Console.WriteLine("Temperatura: " + elementOfJson.temperatura);
                    Console.WriteLine("Umidità: " + elementOfJson.umidita);
                    Console.WriteLine("Direzione vento: " + elementOfJson.direzioneVento);
                    Console.WriteLine("Intensità vento: " + elementOfJson.intensitaVento);
                    Console.WriteLine("Pressione: " + elementOfJson.pressione);
                    Console.WriteLine("Pluviometro: " + elementOfJson.pluviometro);

                    businessLayer.Insert(Identificator(), elementOfJson.numRilevazione, elementOfJson.ora.ToString(), elementOfJson.intensitaVento, elementOfJson.direzioneVento, elementOfJson.pressione, elementOfJson.temperatura, elementOfJson.umidita, elementOfJson.pluviometro);
                    Thread.Sleep(10);
                }
            }
            else
            {
                Console.WriteLine("\nNESSUN RILEVAMENTO EFFETTUATO DURANTE LE DATE INSERITE.\n"); 
            }
            
        }
        #endregion

        static void Main(string[] args)
        {
            db();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ESERCIZIO CLIENT/SERVER PER TRASMISSIONE DATI METEO FORMATO JSON");
            Console.ResetColor();
            UserChoice(); // Scelta dell'utente nel menu
        }

        #region Menù.
        /// <summary>
        /// Visualizza il menù e richiede l'opzione.
        /// </summary>
        /// <returns>Scelta dell'utente.</returns>
        public static void MenuCheck(out char ch)
        {
            do // Legge e controlla l'opzione scelta.
            {
                ch = Console.ReadKey(true).KeyChar;
                ch = char.ToUpper(ch);
            }
            while (!((ch == 'I') || (ch == 'N') || (ch == 'E') || (ch == 'S'))); // Controllo scelta.
        }
        #endregion

        #region Scelta dell'utente.
        /// <summary>
        /// SCelta dell'utente.
        /// </summary>
        public static void UserChoice()
        {
            char opz = ' ';
            while (true)
            {
                BackgroundMenu();
                MenuCheck(out opz); // Richiede l'opzione.

                switch (opz) // Esegue l'opzione scelta.
                {
                    #region Scelta dell'utente
                    case 'I':
                        StartClient('I'); // Immissione manuale delle date da parte dell'utente.
                        break;
                    #endregion

                    #region Ultimo rilevamento.
                    case 'N':
                        StartClient('N');  // Immissione manuale di una data, per ottenere l'ultimo rilevamento.
                        break;
                    #endregion

                    #region Ultimo rilevamento.
                    case 'S':
                        businessLayer.SelectAll(); // visualizzazione elementi sul db
                        break;
                    #endregion

                    #region Uscita app.
                    case 'E':
                        Console.WriteLine("\n\tEXIT...");
                        Thread.Sleep(1500);
                        Environment.Exit(-1); // Chiusura app.
                        break;
                    #endregion
                }
            }
        }
        #endregion

        #region Background.
        /// <summary>
        /// Set del background con il fiume e il ponte.
        /// </summary>
        public static void BackgroundMenu()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n\n - [I] Choose a range");
            Console.WriteLine(" - [N] Actual value ");
            Console.WriteLine(" - [S] Database data");
            Console.WriteLine(" - [E] Exit from app\n"); // Esce dal programma.
            Console.ResetColor();
        }
        #endregion

        #region identificator for record.
        static int Identificator()
        {
            // Creating object of random class
            Random rand = new Random();
            string str = "";
            do
            {
                str = "";
                for (int j = 0; j < 8; j++)
                {
                    str += rand.Next(0, 9);
                }
            } while (alreadychosen.Contains(str));
            alreadychosen.Add(str);


            return int.Parse(str);
        }
        #endregion

        static void db()
        {
            string dbname = "serverdb";
            businessLayer.CreatingDB(dbname);
            businessLayer.connectionString = $"server=localhost;userid=root;password=root;database={dbname}";
            businessLayer.CreateTables("rilevamenti");  
        }
    }
}
