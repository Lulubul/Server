namespace NetworkTypes
{
    public enum Command
    {
        Connect,
        Login,
        Logout,
        Register,
        Create,
        Leave,
        Join,
        ChangeTeam,
        Lobby,
        Search,
        Start,
        Disconnect,
        Shoot,
        Die,
        Move,
        SyncRooms,
        SyncLobby,
        UpdateLobby,
        ChangeTurn,
        FinishAction,
        Turn,
        SyncHero,
        InitializeBoard
    };

    public enum Response
    {
        Succed,
        Fail
    }

    public enum Team
    {
        Red,
        Blue
    }

    public enum GameType
    {
        Classic,
        Fast
    }

    public enum State
    {
        None,
        Connect,
        Join,
        Lobby,
        Play
    }

    public enum HeroRace { Human, Orc };
    public enum HeroType { Might, Magic }
    public enum CreatureType { Melee, Range }
    public enum CreatureStatus { Alive, Death }

}
