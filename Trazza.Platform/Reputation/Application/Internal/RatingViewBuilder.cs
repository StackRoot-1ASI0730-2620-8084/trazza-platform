using TrazzaPlatform.API.Execution.Interfaces.Acl;
using TrazzaPlatform.API.Iam.Interfaces.Acl;
using TrazzaPlatform.API.Reputation.Domain.Model.Aggregates;
using TrazzaPlatform.API.Reputation.Domain.Services;

namespace TrazzaPlatform.API.Reputation.Application.Internal;

public class RatingViewBuilder(IExecutionContextFacade executionContextFacade, IIamContextFacade iamContextFacade)
{
    /// <summary>The counterpart is the rated user for given ratings and the rater for received ones.</summary>
    public async Task<List<RatingView>> BuildAsync(IReadOnlyCollection<Rating> ratings, ulong userId,
        CancellationToken ct = default)
    {
        if (ratings.Count == 0) return [];
        var trips = (await executionContextFacade.ListTripsAsync(ratings.Select(rating => rating.TripId).ToList(), ct))
            .ToDictionary(trip => trip.Id);
        var contacts = new Dictionary<ulong, ContactSnapshot?>();
        var views = new List<RatingView>();
        foreach (var rating in ratings)
        {
            var counterpartId = rating.RaterUserId == userId ? rating.RatedUserId : rating.RaterUserId;
            if (!contacts.TryGetValue(counterpartId, out var contact))
            {
                contact = await iamContextFacade.FindContactAsync(counterpartId, ct);
                contacts[counterpartId] = contact;
            }

            views.Add(new RatingView(rating, trips.TryGetValue(rating.TripId, out var trip) ? trip.PublicId : Guid.Empty,
                contact?.PublicId ?? Guid.Empty, contact?.BusinessName ?? contact?.FullName ?? string.Empty));
        }

        return views;
    }
}