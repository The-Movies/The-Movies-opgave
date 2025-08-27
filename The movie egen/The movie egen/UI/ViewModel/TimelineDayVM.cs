// System-navneområder (standard)
using System;                             // DateTime, Math, etc.
using System.Collections.ObjectModel;     // ObservableCollection<T> (til binding)
using System.Linq;                        // LINQ (GroupBy/OrderBy/Max/First)

namespace The_movie_egen.UI.ViewModel
{
    /// <summary>
    /// ViewModel til en dag i tidslinje-visningen.
    /// - Holder alle forestillinger for en specifik dato
    /// - Grupperer forestillinger pr. sal for visning
    /// - Beregner layout-positioner for at undgå overlap
    /// - Håndterer tilføjelse og fjernelse af forestillinger
    /// </summary>
    public class TimelineDayVM
    {
        /// <summary>
        /// Ctor: initialiserer dag med dato og tomme samlinger.
        /// </summary>
        public TimelineDayVM(DateTime date) => Date = date;

        // ─────────────────────────────────────────────────────────────────────────
        //  Publict API til View (bindings)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Datoen for denne dag (read-only).
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// Datoen formateret som tekst (dd-MM-yyyy) til visning i UI.
        /// </summary>
        public string DateText => Date.ToString("dd-MM-yyyy");

        /// <summary>
        /// Alle forestillinger for dagen i en flad liste (read-only).
        /// Bruges internt til at holde styr på alle forestillinger.
        /// </summary>
        public ObservableCollection<TimelineScreeningVM> Screenings { get; } = new();

        /// <summary>
        /// Forestillinger grupperet pr. sal til visning i UI.
        /// Hver gruppe indeholder forestillinger for én sal med beregnede layout-positioner.
        /// </summary>
        public ObservableCollection<AuditoriumGroupVM> AuditoriumGroups { get; } = new();

        // ─────────────────────────────────────────────────────────────────────────
        //  Public metoder
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tilføjer en forestilling til dagen hvis den matcher datoen.
        /// 1) Validerer at forestillingen tilhører denne dag
        /// 2) Tilføjer til flad liste
        /// 3) Opdaterer grupperede visning med nye layout-beregninger
        /// </summary>
        public void AddScreening(TimelineScreeningVM screening)
        {
            if (screening.Start.Date == Date)
            {
                Screenings.Add(screening);
                UpdateAuditoriumGroups();
            }
        }

        /// <summary>
        /// Rydder alle forestillinger og grupper for denne dag.
        /// Bruges når dagen skal nulstilles eller opdateres.
        /// </summary>
        public void Clear()
        {
            Screenings.Clear();
            AuditoriumGroups.Clear();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Private hjælpefunktioner
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Opdaterer AuditoriumGroups baseret på Screenings.
        /// 1) Grupperer forestillinger pr. sal
        /// 2) Sorterer forestillinger pr. sal efter starttidspunkt
        /// 3) Beregner layout-positioner for at undgå overlap
        /// 4) Opretter AuditoriumGroupVM for hver sal
        /// </summary>
        private void UpdateAuditoriumGroups()
        {
            AuditoriumGroups.Clear();

            // Grupper pr. sal
            var groups = Screenings
                .GroupBy(s => s.Auditorium.Id)
                .Select(g =>
                {
                    var sorted = g.OrderBy(s => s.Start).ToList();

                    // Placer i "baner" så de ikke overlapper vertikalt
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        var current = sorted[i];
                        int lane = 0;

                        while (true)
                        {
                            bool overlapsInLane = false;
                            double wantedTop = 5 + lane * 35; // 30px høj + lidt luft

                            foreach (var other in sorted.Take(i))
                            {
                                // Samme lane?
                                if (Math.Abs(other.TimelineTop - wantedTop) < 1)
                                {
                                    var curEnd = current.TimelinePosition + current.TimelineWidth;
                                    var othEnd = other.TimelinePosition + other.TimelineWidth;

                                    // Overlapper horisontalt?
                                    if (!(current.TimelinePosition >= othEnd || curEnd <= other.TimelinePosition))
                                    {
                                        overlapsInLane = true;
                                        break;
                                    }
                                }
                            }

                            if (!overlapsInLane)
                            {
                                current.SetTimelineTop(wantedTop);
                                break;
                            }

                            lane++;
                        }
                    }

                    // Beregn rækkehøjde (mindst 36px)
                    double maxTop = sorted.Count == 0 ? 5 : sorted.Max(s => s.TimelineTop);
                    double rowHeight = Math.Max(36, maxTop + 30 + 5); // 30 = blokhøjde

                    return new AuditoriumGroupVM
                    {
                        AuditoriumName = sorted.First().AuditoriumName,
                        Screenings = new ObservableCollection<TimelineScreeningVM>(sorted),
                        RowHeight = rowHeight
                    };
                })
                .ToList();

            foreach (var grp in groups)
                AuditoriumGroups.Add(grp);
        }
    }

    /// <summary>
    /// ViewModel til en sal-gruppe i tidslinje-visningen.
    /// - Holder forestillinger for én specifik sal
    /// - Indeholder beregnet rækkehøjde til layout
    /// - Bruges til at gruppere forestillinger pr. sal i UI
    /// </summary>
    public class AuditoriumGroupVM
    {
        /// <summary>
        /// Navn på salen (binder til UI).
        /// </summary>
        public string AuditoriumName { get; set; } = string.Empty;

        /// <summary>
        /// Forestillinger for denne sal (binder til UI).
        /// </summary>
        public ObservableCollection<TimelineScreeningVM> Screenings { get; set; } = new();

        /// <summary>
        /// Beregnet højde for rækken i pixels (binder til XAML).
        /// Minimum 36px, kan være højere hvis der er mange overlappende forestillinger.
        /// </summary>
        public double RowHeight { get; set; } = 36;
    }
}
