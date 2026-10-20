namespace SwedenStart;

public interface IVisitSwedenService
{
     Task<VisitSwedenSyncResult> SyncAttractionsServiceAsync(CancellationToken cancellationToken = default);
}

public record VisitSwedenUpsertResult(int Inserted, int Updated);

public record VisitSwedenSyncResult(
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
