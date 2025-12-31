using GitOut.Application.Interfaces;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;
using GitOut.Infrastructure.Persistence.RoomFactories;

namespace GitOut.Infrastructure.Persistence;

/// <summary>
/// Loads rooms from individual factory classes
/// </summary>
public class RoomRepository : IRoomRepository
{
    private readonly Domain.Interfaces.IGitCommandExecutor _gitExecutor;
    private Dictionary<string, Room>? _cachedRooms;

    public RoomRepository(Domain.Interfaces.IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public async Task<Dictionary<string, Room>> LoadRoomsAsync()
    {
        if (_cachedRooms != null)
        {
            return _cachedRooms;
        }

        _cachedRooms = new Dictionary<string, Room>();

        // Create room factories
        var room01Factory = new Room01InitializationChamberFactory(_gitExecutor);
        var room02Factory = new Room02StagingAreaFactory(_gitExecutor);
        var room03Factory = new Room03HistoryArchiveFactory(_gitExecutor);
        var room04Factory = new Room04StatusChamberFactory(_gitExecutor);
        var room05Factory = new Room05BranchJunctionFactory(_gitExecutor);
        var room06Factory = new Room06MergeNexusFactory(_gitExecutor);
        var room07Factory = new Room07RestorationVaultFactory(_gitExecutor);
        var room08Factory = new Room08QuizMastersHallFactory(_gitExecutor);
        var room09Factory = new Room09ConflictCatacombsFactory(_gitExecutor);
        var room10Factory = new Room10StashSanctumFactory(_gitExecutor);
        var room11Factory = new Room11CherryPickGardenFactory(_gitExecutor);
        var room12Factory = new Room12RebaseRidgeFactory(_gitExecutor);
        var room13Factory = new Room13TagTowerFactory(_gitExecutor);
        var room14Factory = new Room14ReflogRuinsFactory(_gitExecutor);
        var room15Factory = new Room15RemoteRealmFactory(_gitExecutor);
        var room16Factory = new Room16BisectBattlefieldFactory(_gitExecutor);

        // Create rooms from factories
        _cachedRooms["room-1"] = await room01Factory.CreateAsync();
        _cachedRooms["room-2"] = await room02Factory.CreateAsync();
        _cachedRooms["room-3"] = await room03Factory.CreateAsync();
        _cachedRooms["room-4"] = await room04Factory.CreateAsync();
        _cachedRooms["room-5"] = await room05Factory.CreateAsync();
        _cachedRooms["room-6"] = await room06Factory.CreateAsync();
        _cachedRooms["room-7"] = await room07Factory.CreateAsync();
        _cachedRooms["room-8"] = await room08Factory.CreateAsync();
        _cachedRooms["room-9"] = await room09Factory.CreateAsync();
        _cachedRooms["room-10"] = await room10Factory.CreateAsync();
        _cachedRooms["room-11"] = await room11Factory.CreateAsync();
        _cachedRooms["room-12"] = await room12Factory.CreateAsync();
        _cachedRooms["room-13"] = await room13Factory.CreateAsync();
        _cachedRooms["room-14"] = await room14Factory.CreateAsync();
        _cachedRooms["room-15"] = await room15Factory.CreateAsync();
        _cachedRooms["room-16"] = await room16Factory.CreateAsync();

        return _cachedRooms;
    }

    public async Task<Room?> GetRoomByIdAsync(string roomId)
    {
        var rooms = await LoadRoomsAsync();
        rooms.TryGetValue(roomId, out var room);
        return room;
    }

    public async Task<Room?> GetStartRoomAsync()
    {
        var rooms = await LoadRoomsAsync();
        return rooms.Values.FirstOrDefault(r => r.IsStartRoom);
    }
}
