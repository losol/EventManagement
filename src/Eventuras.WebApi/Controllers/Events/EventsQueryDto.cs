using Eventuras.Domain;
using Eventuras.Services.Events;
using Eventuras.WebApi.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Eventuras.WebApi.Controllers.Events
{
    public class EventsQueryDto : PageQueryDto
    {
        public EventInfo.EventInfoType? Type { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public PeriodMatchingKind Period { get; set; } = PeriodMatchingKind.Match;

        [Range(1, int.MaxValue)]
        public int? CollectionId { get; set; }

        [Range(1, int.MaxValue)]
        public int? OrganizationId { get; set; }

        public EventInfoFilter ToEventInfoFilter()
        {
            var filter = new EventInfoFilter
            {
                TypeOneOf = Type.HasValue ? new[] { Type.Value } : null,
                StatusNoneOf = new[]
                {
                    EventInfo.EventInfoStatus.Draft
                },
                OrganizationId = OrganizationId,
                AccessibleOnly = false
            };

            if (CollectionId.HasValue) 
            {
                filter.CollectionIds = new int[CollectionId.Value];
            }

            if (!Start.HasValue && !End.HasValue)
            {
                return EventInfoFilter.UpcomingEvents(filter);
            }

            switch (Period)
            {
                case PeriodMatchingKind.Match:
                    filter.StartDate = Start;
                    filter.EndDate = End;
                    break;

                case PeriodMatchingKind.Intersect:
                    if (Start.HasValue)
                    {
                        filter.StartDateAfter = DateTime.MinValue; // to ensure event start date is set
                        filter.EndDateIsNullOrAfter = Start;
                    }
                    if (End.HasValue)
                    {
                        filter.StartDateBefore = End;
                    }
                    break;

                case PeriodMatchingKind.Contain:
                    if (Start.HasValue)
                    {
                        filter.StartDateAfter = Start;
                        filter.EndDateIsNullOrAfter = Start;
                    }
                    if (End.HasValue)
                    {
                        filter.StartDateBefore = End;
                        filter.EndDateIsNullOrBefore = End;
                    }
                    break;
            }

            return filter;
        }
    }

    public enum PeriodMatchingKind
    {
        /**
         * Check whether the filter start/end date are equal to the event start/end date.
         */
        Match,

        /**
         * Means to check if the given filter period is at least partially intersecting with event.
         */
        Intersect,

        /**
         * Means to check if the given filter period contains the whole event period.
         */
        Contain
    }
}
