using Microsoft.Extensions.Logging;
using Npgsql;

namespace SwedenStart;

public class RoadmapRepository : IRoadmapRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<RoadmapRepository> _logger;

    public RoadmapRepository(NpgsqlDataSource dataSource, ILogger<RoadmapRepository> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task<Roadmap> SaveAsync(Roadmap roadmap)
    {
        const string insertRoadmap = @"
            insert into roadmaps (
                id,
                user_id,
                created_at,
                origin,
                residence_permit,
                live_in_sweden,
                personnummer,
                applied_personnummer,
                id_card,
                bank_account,
                bank_id,
                housing,
                plan_to_drive,
                driving_licence_type,
                purpose,
                insurance
            ) values (
                @id,
                @user_id,
                @created_at,
                @origin,
                @residence_permit,
                @live_in_sweden,
                @personnummer,
                @applied_personnummer,
                @id_card,
                @bank_account,
                @bank_id,
                @housing,
                @plan_to_drive,
                @driving_licence_type,
                @purpose,
                @insurance
            );";

        const string insertTask = @"
            insert into roadmap_tasks (
                id,
                roadmap_id,
                title,
                description,
                completed
            ) values (
                @id,
                @roadmap_id,
                @title,
                @description,
                @completed
            );";

        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await using var roadmapCommand = connection.CreateCommand();
            roadmapCommand.Transaction = transaction;
            roadmapCommand.CommandText = insertRoadmap;
            roadmapCommand.Parameters.AddWithValue("id", roadmap.Id);
            roadmapCommand.Parameters.AddWithValue("user_id", roadmap.UserId);
            roadmapCommand.Parameters.AddWithValue("created_at", roadmap.CreatedAt);
            roadmapCommand.Parameters.AddWithValue("origin", roadmap.Origin);
            roadmapCommand.Parameters.AddWithValue("residence_permit", roadmap.ResidencePermit);
            roadmapCommand.Parameters.AddWithValue("live_in_sweden", roadmap.LiveInSweden);
            roadmapCommand.Parameters.AddWithValue("personnummer", roadmap.Personnummer);
            roadmapCommand.Parameters.AddWithValue("applied_personnummer", roadmap.AppliedPersonnummer);
            roadmapCommand.Parameters.AddWithValue("id_card", roadmap.IdCard);
            roadmapCommand.Parameters.AddWithValue("bank_account", roadmap.BankAccount);
            roadmapCommand.Parameters.AddWithValue("bank_id", roadmap.BankId);
            roadmapCommand.Parameters.AddWithValue("housing", roadmap.Housing);
            roadmapCommand.Parameters.AddWithValue("plan_to_drive", roadmap.PlanToDrive);
            roadmapCommand.Parameters.AddWithValue("driving_licence_type", roadmap.DrivingLicenceType);
            roadmapCommand.Parameters.AddWithValue("purpose", roadmap.Purpose);
            roadmapCommand.Parameters.AddWithValue("insurance", roadmap.Insurance);
            await roadmapCommand.ExecuteNonQueryAsync();

            foreach (var task in roadmap.Tasks)
            {
                await using var taskCommand = connection.CreateCommand();
                taskCommand.Transaction = transaction;
                taskCommand.CommandText = insertTask;
                taskCommand.Parameters.AddWithValue("id", task.Id == Guid.Empty ? Guid.NewGuid() : task.Id);
                taskCommand.Parameters.AddWithValue("roadmap_id", roadmap.Id);
                taskCommand.Parameters.AddWithValue("title", task.Title);
                taskCommand.Parameters.AddWithValue("description", task.Description);
                taskCommand.Parameters.AddWithValue("completed", task.Completed);
                await taskCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to save roadmap {RoadmapId}.", roadmap.Id);
            throw;
        }

        return roadmap;
    }
}
