using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using Kuvarpa;
using System.Linq;
using System;

namespace Kuvarpa.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connections;

        public ChatHub(IDictionary<string, UserConnection> connections)
        {
            _botUser = "MyChat Bot";            
            _connections = connections;
        }


        // logic for what to do when players disconnect
        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                using (var db = new GameContext())
                {
                    //var rand = new Random();
                    var room = db.Rooms
                        .Where(p => p.RoomName == userConnection.Room).Single();
                    var arvaajat = db.Players
                        .Where(p => p.IsDrawer == false).ToList();
                    var players = db.Players
                        .Where(p => p.ConnectionId == Context.ConnectionId).Single();
                    var player = room.Players.Where(p => p.ConnectionId == Context.ConnectionId).Single();
                    var checkRoomCount = db.Players
                        .Where(p => p.Room.RoomName == userConnection.Room).Count();
                    if ((player.IsDrawer == true) && (checkRoomCount > 1))
                    {
                        db.Remove(player);
                        db.SaveChanges();
                        var uusiPiirtaja = room.Players.Where(p => p.IsDrawer == false).First();
                        uusiPiirtaja.IsDrawer = true;
                        Clients.Client(uusiPiirtaja.ConnectionId).SendAsync("ReceiveIsDrawer", true);
                        SendWord(room.RightWordNumber);                        
                        db.SaveChanges();
                        
                    }
                    else
                    {
                        db.Remove(player);
                        checkRoomCount -= 1;
                    }
                    

                    // removes room from the db when there are no players (to open up the roomnames for future use)
                    if (checkRoomCount < 1)
                    {
                        db.Remove(room);
                        db.SaveChanges();
                    }
                }
                _connections.Remove(Context.ConnectionId);
                
                Clients .Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has left");
                SendUsersConnected (userConnection.Room);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(UserConnection userConnection)
        {
            bool roomexists = true;

            // the logic to stop users from creating when joining non existant rooms
            using (var db = new GameContext())
            {
                try
                {
                    var query = db.Rooms.Where(p => p.RoomName == userConnection.Room).Single();
                    if (query != null)
                    {
                        roomexists = true;
                        var room = db.Rooms
                        .Where(p => p.RoomName == userConnection.Room).Single();
                        room.Players.Add(new Player { IsDrawer = false, PlayerName = userConnection.User, ConnectionId = Context.ConnectionId });
                        db.SaveChanges();
                    }
                }
                catch
                {
                    roomexists = false;
                    Context.Abort();
                }
                
            }
            if (roomexists == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);

                _connections[Context.ConnectionId] = userConnection;

                await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has joined {userConnection.Room}");

                await SendUsersConnected(userConnection.Room);
            }
            
        }

        public async Task CreateRoom(UserConnection userConnection)
        {
            var rand = new Random();
            bool roomexists = false;
            using (var db = new GameContext())
            {
                try
                {
                    var query = db.Rooms.Where(p => p.RoomName == userConnection.Room).Single();
                    if (query != null)
                    {
                        roomexists = true;
                        Context.Abort();
                    }
                }
                catch
                {
                    db.Add(new Room { RoomName = userConnection.Room });
                    db.SaveChanges();
                    var room = db.Rooms
                        .Where(p => p.RoomName == userConnection.Room).Single();
                    room.Players.Add(new Player { IsDrawer = true, PlayerName = userConnection.User, ConnectionId = Context.ConnectionId });

                    db.SaveChanges();
                    var sanojenMaara = db.Words.Count();
                    var oikeaSananNumero = rand.Next(1, sanojenMaara);
                    room.RightWordNumber = oikeaSananNumero;

                    db.SaveChanges();

                    var sana = db.Words
                        .Where(p => p.WordId == oikeaSananNumero).Single();
                    await Clients.Group(userConnection.Room).SendAsync("ReceiveWord", sana.Content);
                    await Clients.Caller.SendAsync("ReceiveWord", sana.Content);
                }

                
            }
            if (roomexists == false)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);

                _connections[Context.ConnectionId] = userConnection;

                await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has joined {userConnection.Room}");

                await SendUsersConnected(userConnection.Room);
            }

        }

        // sends messages to chat if they are wrong and displays a messaget that a user has guessed correctly if they did that
        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                if (MatchWords(message.ToLower(), userConnection).Result == true)
                {

                    using (var db = new GameContext())
                    {
                        var huone = db.Rooms
                            .Where(b => b.RoomName == userConnection.Room).Single();

                        huone.GuessCount = 0;
                        await SendUsersConnected(userConnection.Room);
                        await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.User, message, true);
                        
                        db.SaveChanges();

                    }
                }
                else
                {
                    using (var db = new GameContext())
                    {
                        var huone = db.Rooms
                            .Where(b => b.RoomName == userConnection.Room).Single();
                        var pelaajat = db.Players
                            .Where(p => p.Room.RoomName == userConnection.Room).ToList();
                        int koko = pelaajat.Count() * 3;
                        
                        if (huone.GuessCount >= (huone.Players.Count() * 3))
                        {
                            await GamePlayLoop(userConnection);
                        }
                        else
                        {

                            huone.GuessCount = huone.GuessCount + 1;
                            await SendUsersConnected(userConnection.Room);
                            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.User, message, false);
                            
                            db.SaveChanges();
                        }
                    }

                }


            }
        }

        // sends the savedata of the canvas
        public async Task SendCanvasData(string canvasData)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.Room).SendAsync("ReceiveData", canvasData);
            }
        }
        // sends the boolean state of players
        public async Task SendIsDrawer(bool isDrawer)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.OthersInGroup(userConnection.Room).SendAsync("ReceiveIsDrawer", !isDrawer);
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveIsDrawer", isDrawer);
            }
        }
        // sends the word that the drawer has to draw
        public async Task SendWord(int number)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
                
            {
                await using (var db = new GameContext())
                {
                    var sana = db.Words
                        .Where(p => p.WordId == number).Single();
                    await Clients.Group(userConnection.Room).SendAsync("ReceiveWord", sana.Content);
                    Console.WriteLine(sana.Content);
                }
            }
        }

        // logic for checking if the guessed word is correct and what to do after said guess
        public async Task<bool> MatchWords(string sana, UserConnection userConnection)
        {
            string rightWord = "default";
            using (var db = new GameContext())
            {
                var rightNumber = db.Rooms
                    .Where(b => b.RoomName == userConnection.Room).Single().RightWordNumber;
                var word = db.Words
                    .Where(b => b.WordId == rightNumber).Single();

                rightWord = word.Content;
            }

            if (sana == rightWord)
            {
                await GamePlayLoop(userConnection);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task SendCorrectAnswer(string answer, UserConnection userConnection)
        {
            await Clients.Group(userConnection.Room).SendAsync("ReceiveAnswer", answer);
        }
        // contains the logic that dictates what happens when somone guesses correctly
        public async Task GamePlayLoop(UserConnection userConnection)
        {
            var rand = new Random();
            await using (var db = new GameContext())
            {
                var room = db.Rooms.Where(p => p.RoomName == userConnection.Room).Single();
                var piirtaja = db.Players
                .Where(p => (p.IsDrawer == true) && (p.Room.RoomName == room.RoomName)).Single();


                var arvaaja = db.Players
                .Where(p => p.ConnectionId == Context.ConnectionId).Single();
                room.GuessCount = 0;

                if (arvaaja.PlayerName != piirtaja.PlayerName)
                {
                    if (room.GuessCount < (room.Players.Count() * 3))
                    {
                        arvaaja.Points += 1;
                    }

                    piirtaja.IsDrawer = false;
                    arvaaja.IsDrawer = true;
                    // chooses the next word to be guessed randomly from the database

                    var answer = db.Words.Where(b => b.WordId == room.RightWordNumber).Single();

                    await SendCorrectAnswer(answer.Content, userConnection);
                    room.RightWordNumber = rand.Next(1, db.Words.Count() + 1);
                    db.SaveChanges();

                    await SendIsDrawer(true);
                    await SendWord(room.RightWordNumber);

                }
                if (arvaaja.Points == 2)
                {
                    await Clients.Group(room.RoomName).SendAsync("GameWon", arvaaja.PlayerName, arvaaja.Points);
                    var pelaajat = db.Players
                    .Where(p => (p.Room.RoomName == room.RoomName)).ToList();

                    for (int i = 0; i < pelaajat.Count(); i++)
                    {
                        pelaajat[i].Points = 0;
                    }
                    db.SaveChanges();
                }
            }
        }

        public Task SendUsersConnected(string room)
        {
            /*var users = _connections.Values
                .Where(c => c.Room == room)
                .Select(c => c.User);*/
            using (var db = new GameContext())
            {
                var users2 = db.Players
                    .Where(b => b.Room.RoomName == room)
                    .Select(b => new { b.PlayerName, b.Points });
                
                List<string> users  = new List<string>(); 
                foreach (var item in users2)
                {
                    users.Add(item.PlayerName + " points:" + item.Points.ToString());
                }
                return Clients.Group(room).SendAsync("UsersInRoom", users);
            }
            
                
        }
        
    }
}
