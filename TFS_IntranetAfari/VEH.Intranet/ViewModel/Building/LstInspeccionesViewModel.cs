using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using VEH.Intranet.Models;
using VEH.Intranet.Helpers;
using VEH.Intranet.Controllers;
using System.Web.WebPages.Html;
using System.ComponentModel.DataAnnotations;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class LstInspeccionesViewModel : BaseViewModel
    {
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; }
        [Display(Name = "Fecha de Fin")]
        public DateTime FechaFin { get; set; }
        public List<Inspeccion> LstInspecciones { get; set; }
        public List<SelectListItem> LstPreguntas { get; set; }
        public Int32 PreguntaFiltrado { get; set; }
        public Dictionary<int, Dictionary<DateTime, int>> LstHistorialPreguntas { get; set; }
        public LstInspeccionesViewModel() { }
        public Int32 EdificioId { get; set; }
        public void Fill(CargarDatosContext datacontext, Int32 edificioId)
        {
            baseFill(datacontext);
            EdificioId = edificioId;
            //LstPreguntas = new List<SelectListItem>();
            //PreguntaFiltrado = 1;
            //LstPreguntas.Add(new SelectListItem { Value = "1", Text = "1. Estado del Jardín" });
            //LstPreguntas.Add(new SelectListItem { Value = "2", Text = "1. Estado del Jardín" });
            //LstPreguntas.Add(new SelectListItem { Value = "3", Text = "1. Estado del Jardín" });
            //LstPreguntas.Add(new SelectListItem { Value = "4", Text = "1. Estado del Jardín" });
            //LstPreguntas.Add(new SelectListItem { Value = "5", Text = "1. Estado del Jardín" });
            //LstPreguntas.Add(new SelectListItem { Value = "6", Text = "1. Estado del Jardín" });
            //LstPreguntas.Add(new SelectListItem { Value = "7", Text = "1. Estado del Jardín" });
            //LstPreguntas.Add(new SelectListItem { Value = "8", Text = "1. Estado del Jardín" });
            //LstPreguntas.Add(new SelectListItem { Value = "9", Text = "1. Estado del Jardín" });
            //LstPreguntas.Add(new SelectListItem { Value = "10", Text = "1. Estado del Jardín" });
            //LstPreguntas.Add(new SelectListItem { Value = "11", Text = "1. Estado del Jardín" });
            //LstPreguntas = new List<string>() 
            //{
            //    "1. Estado del Jardín",
            //    "2. Estado pintado fachadan",
            //    "3. Vestimenta de trabajadoresr",
            //    "4. Horario de trabajadores",
            //    "5. Limpieza de Lobby",
            //    "6. Estado pintado de Lobby",
            //    "7. Limpieza de ascensoresn",
            //    "8. Estado de ascensores",
            //    "9. Motor puertas garaje",
            //    "10. Resortes garaje",
            //    "11. Limpieza de puertas garaje",
            //    "12. Limpieza garaje",
            //    "13. Estado pintado garajes",
            //    "14. Limpieza paredes",
            //    "15. Limpieza pisos",
            //    "16. Estado pintado pisosnt",
            //    "17. Limpieza baño invitadosreg",
            //    "18. Estado cuarto de servicios",
            //    "19. Limpieza baño trabajadores",
            //    "20. Limpieza baños area social",
            //    "21. Limpieza area socialnt",
            //    "22. Limpieza Salon de usos multip",
            //    "23. Estado parrillas",
            //    "24. Limpieza escaleras",
            //    "25. Limpieza barandas escalera",
            //    "26. Limpieza lunas",
            //    "27. Limpieza piscina",
            //    "28. Bomba piscina",
            //    "29. Limpieza de gimnasiont",
            //    "30. Limpieza maquinas del gimnasio",
            //    "31. Cuarto de bombas",
            //    "32. Cuarto de maquinas",
            //    "33. Extractores de monoxido",
            //    "34. Camaras",
            //    "35. Estado Sauna",
            //    "36. Estado pintado areas comunes"
            //};
            LstInspecciones = datacontext.context.Inspeccion.Where(x => x.EdificioId == edificioId).OrderByDescending(x => x.Fecha).ToList();
            //Llenamos la estadistica de las preguntas por dia, remplazando la ultima hora del dia, si hubieran mas de una por dia
            //Es decir, puedes corregir algun mal envio de reporte durante el edia, se registraran todas, pero solo se tomara la ultima para el reporte
            LstHistorialPreguntas = new Dictionary<int, Dictionary<DateTime, int>>(); for (int i = 1; i <= 36; i++) LstHistorialPreguntas[i] = new Dictionary<DateTime, int>();
            foreach (var x in LstInspecciones)
            {
                DateTime key = LstHistorialPreguntas[1].Keys.FirstOrDefault(y => y.Date.CompareTo(x.Fecha.Date) == 0);

                if (key != DateTime.MinValue) //mismo dia, remplazamos todo-tomamos el ultimo
                {
                    //  var Inspeccion = datacontext.context.Inspeccion.FirstOrDefault( z => (z.Fecha.Date.CompareTo(x.Fecha.Date) == 0));
                    LstHistorialPreguntas[1][key] = x.Pregunta1;
                    LstHistorialPreguntas[2][key] = x.Pregunta2;
                    LstHistorialPreguntas[3][key] = x.Pregunta3;
                    LstHistorialPreguntas[4][key] = x.Pregunta4;
                    LstHistorialPreguntas[5][key] = x.Pregunta5;
                    LstHistorialPreguntas[6][key] = x.Pregunta6;
                    LstHistorialPreguntas[7][key] = x.Pregunta7;
                    LstHistorialPreguntas[8][key] = x.Pregunta8;
                    LstHistorialPreguntas[9][key] = x.Pregunta9;
                    LstHistorialPreguntas[10][key] = x.Pregunta10;
                    LstHistorialPreguntas[11][key] = x.Pregunta11;
                    LstHistorialPreguntas[12][key] = x.Pregunta12;
                    LstHistorialPreguntas[13][key] = x.Pregunta13;
                    LstHistorialPreguntas[14][key] = x.Pregunta14;
                    LstHistorialPreguntas[15][key] = x.Pregunta15;
                    LstHistorialPreguntas[16][key] = x.Pregunta16;
                    LstHistorialPreguntas[17][key] = x.Pregunta17;
                    LstHistorialPreguntas[18][key] = x.Pregunta18;
                    LstHistorialPreguntas[19][key] = x.Pregunta19;
                    LstHistorialPreguntas[20][key] = x.Pregunta20;
                    LstHistorialPreguntas[21][key] = x.Pregunta21;
                    LstHistorialPreguntas[22][key] = x.Pregunta22;
                    LstHistorialPreguntas[23][key] = x.Pregunta23;
                    LstHistorialPreguntas[24][key] = x.Pregunta24;
                    LstHistorialPreguntas[25][key] = x.Pregunta25;
                    LstHistorialPreguntas[26][key] = x.Pregunta26;
                    LstHistorialPreguntas[27][key] = x.Pregunta27;
                    LstHistorialPreguntas[28][key] = x.Pregunta28;
                    LstHistorialPreguntas[29][key] = x.Pregunta29;
                    LstHistorialPreguntas[30][key] = x.Pregunta30;
                    LstHistorialPreguntas[31][key] = x.Pregunta31;
                    LstHistorialPreguntas[32][key] = x.Pregunta32;
                    LstHistorialPreguntas[33][key] = x.Pregunta33;
                    LstHistorialPreguntas[34][key] = x.Pregunta34;
                    LstHistorialPreguntas[35][key] = x.Pregunta35;
                    LstHistorialPreguntas[36][key] = x.Pregunta36;
                }
                else
                {
                    LstHistorialPreguntas[1].Add(x.Fecha, x.Pregunta1);
                    LstHistorialPreguntas[2].Add(x.Fecha, x.Pregunta2);
                    LstHistorialPreguntas[3].Add(x.Fecha, x.Pregunta3);
                    LstHistorialPreguntas[4].Add(x.Fecha, x.Pregunta4);
                    LstHistorialPreguntas[5].Add(x.Fecha, x.Pregunta5);
                    LstHistorialPreguntas[6].Add(x.Fecha, x.Pregunta6);
                    LstHistorialPreguntas[7].Add(x.Fecha, x.Pregunta7);
                    LstHistorialPreguntas[8].Add(x.Fecha, x.Pregunta8);
                    LstHistorialPreguntas[9].Add(x.Fecha, x.Pregunta9);
                    LstHistorialPreguntas[10].Add(x.Fecha, x.Pregunta10);
                    LstHistorialPreguntas[11].Add(x.Fecha, x.Pregunta11);
                    LstHistorialPreguntas[12].Add(x.Fecha, x.Pregunta12);
                    LstHistorialPreguntas[13].Add(x.Fecha, x.Pregunta13);
                    LstHistorialPreguntas[14].Add(x.Fecha, x.Pregunta14);
                    LstHistorialPreguntas[15].Add(x.Fecha, x.Pregunta15);
                    LstHistorialPreguntas[16].Add(x.Fecha, x.Pregunta16);
                    LstHistorialPreguntas[17].Add(x.Fecha, x.Pregunta17);
                    LstHistorialPreguntas[18].Add(x.Fecha, x.Pregunta18);
                    LstHistorialPreguntas[19].Add(x.Fecha, x.Pregunta19);
                    LstHistorialPreguntas[20].Add(x.Fecha, x.Pregunta20);
                    LstHistorialPreguntas[21].Add(x.Fecha, x.Pregunta21);
                    LstHistorialPreguntas[22].Add(x.Fecha, x.Pregunta22);
                    LstHistorialPreguntas[23].Add(x.Fecha, x.Pregunta23);
                    LstHistorialPreguntas[24].Add(x.Fecha, x.Pregunta24);
                    LstHistorialPreguntas[25].Add(x.Fecha, x.Pregunta25);
                    LstHistorialPreguntas[26].Add(x.Fecha, x.Pregunta26);
                    LstHistorialPreguntas[27].Add(x.Fecha, x.Pregunta27);
                    LstHistorialPreguntas[28].Add(x.Fecha, x.Pregunta28);
                    LstHistorialPreguntas[29].Add(x.Fecha, x.Pregunta29);
                    LstHistorialPreguntas[30].Add(x.Fecha, x.Pregunta30);
                    LstHistorialPreguntas[31].Add(x.Fecha, x.Pregunta31);
                    LstHistorialPreguntas[32].Add(x.Fecha, x.Pregunta32);
                    LstHistorialPreguntas[33].Add(x.Fecha, x.Pregunta33);
                    LstHistorialPreguntas[34].Add(x.Fecha, x.Pregunta34);
                    LstHistorialPreguntas[35].Add(x.Fecha, x.Pregunta35);
                    LstHistorialPreguntas[36].Add(x.Fecha, x.Pregunta36);
                }
            }
         
        }
    }
}