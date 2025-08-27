using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_movie_egen.Model.Enums
{
    /// <summary>
    /// Enum til film-status i biograf-systemet.
    /// - Bruges til at styre hvilke film der er tilgængelige, i forhold til license-aftaler
    /// - Aktive film kan bruges til forestillings-planlægning
    /// - Arkiverede film gemmes men bruges ikke aktivt
    /// - Serialiseres som tekst i JSON ("Active", "Archived")
    /// </summary>
    public enum FilmStatus
    {
        /// <summary>
        /// Film er aktiv og kan bruges til forestillings-planlægning. (Default)
        /// </summary>
        Active,

        /// <summary>
        /// Film er arkiveret og bruges ikke til nye forestillinger.
        /// Historiske forestillinger bevares stadig.
        /// </summary>
        Archived
    }
}
