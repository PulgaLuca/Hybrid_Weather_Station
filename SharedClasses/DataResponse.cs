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
using System.Text;

namespace SharedClasses
{
    class DataResponse
    {
        public int numRilevazione { get; set; }

        public TimeSpan ora { get; set; }

        public string temperatura { get; set; }

        public string umidita { get; set; }

        public string direzioneVento { get; set; }

        public string intensitaVento { get; set; }

        public string pressione { get; set; }

        public string pluviometro { get; set; }

        public DataResponse() { }
        public DataResponse(int n, TimeSpan o, string t, string u, string dv, string iv, string pr, string pl)
        {
            numRilevazione = n;
            ora = o;
            intensitaVento = iv;
            direzioneVento = dv;
            pressione = pr;
            temperatura = t;
            umidita = u;
            pluviometro = pl;
        }

        public static DataResponse FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            DataResponse dailyValues = new DataResponse();
            dailyValues.numRilevazione = Convert.ToInt32(values[0]);
            dailyValues.ora = Convert.ToDateTime(values[1]).TimeOfDay;
            dailyValues.intensitaVento = Convert.ToString(values[2]);
            dailyValues.direzioneVento = Convert.ToString(values[3]);
            dailyValues.pressione = Convert.ToString(values[4]);
            dailyValues.temperatura = Convert.ToString(values[5]);
            dailyValues.umidita = Convert.ToString(values[6]);
            dailyValues.pluviometro = Convert.ToString(values[7]);
            return dailyValues;
        }
    }
}
