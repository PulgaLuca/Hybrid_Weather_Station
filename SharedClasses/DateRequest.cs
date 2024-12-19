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
    class DateRequest
    {
        public DateTime beginningDateTime { get; set; }
        public DateTime expiredDateTime { get; set; }

        public DateRequest(DateTime bg, DateTime ed)
        {
            beginningDateTime = bg;
            expiredDateTime = ed;
        }
    }
}
