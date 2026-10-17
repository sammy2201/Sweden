namespace SwedenStart;

public interface IVisitSwedenService
{
     Task<VisitSwedenSyncResult> SyncAttractionsAsync(CancellationToken cancellationToken = default);
}

public sealed record VisitSwedenUpsertResult(int Inserted, int Updated);

public sealed record VisitSwedenSyncResult(
     int PagesFetched,
     int RecordsFetched,
     int AttractionsMapped,
     int AttractionsInserted,
     int AttractionsUpdated,
     int SkippedMissingImage,
     int SkippedMissingCoordinates,
     int SkippedUnmappable)
{
     public int AttractionsStored => AttractionsInserted + AttractionsUpdated;
     public int SkippedRecords => Math.Max(0, RecordsFetched - AttractionsMapped);
}
