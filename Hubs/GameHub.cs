//Robert Glowacki//
using Microsoft.AspNetCore.SignalR;
using MvcLoginApp.Services;
using MySQLiteApp.Game;

namespace MvcLoginApp.Hubs
{
    public class GameHub : Hub
    {
        private readonly SessionService _sessionService;

        public GameHub(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        private async Task RecreatePlayerList(string sessionId){
            await Clients.Group(sessionId).SendAsync("ResetPlayerList");

            List<Player> players = _sessionService.GetAllPlayers(sessionId);
            foreach(Player player in players){
                await Clients.Group(sessionId).SendAsync("NewPlayerEntry", player.username, player.score);
            }
        }

        public async Task SendMessage(string sessionId, string user, string message)
        {
            await Clients.Group(sessionId).SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendSolution(string sessionId, string user, string solution)
        {
            string msg;

            if (!_sessionService.PlayerCanSubmitAnwser(sessionId, user)){
                msg = "Bitte warte auf die nächste Runde :)";
            }
            else{
                if (_sessionService.CheckSolution(sessionId, user, Int32.Parse(solution))){
                    msg = "Die Antwort '" + solution + "' ist richtig :)";
                    await RecreatePlayerList(sessionId);
                }
                else{
                    msg = "Die Antwort '" + solution + "' ist falsch :(";
                }
            }

            await Clients.Caller.SendAsync("NotificationMessage", msg);
        }

        public async Task JoinSession(string sessionId, string user, int points)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            if(_sessionService.SessionExists(sessionId)){
                await _sessionService.SendSessionState(sessionId);
                await Clients.Caller.SendAsync("HideStartGameButton");
            }
            else{
                _sessionService.InitiateSession(sessionId);
                await _sessionService.SendSessionState(sessionId);
            }

            _sessionService.AddPlayerToSession(sessionId, user);
            await RecreatePlayerList(sessionId);
            await Clients.Group(sessionId).SendAsync("GameInProgressMessage", _sessionService.GameInProgress(sessionId));
        }

        public async Task StartSession(string sessionId)
        {
            if(_sessionService.SessionExists(sessionId)){
                _sessionService.StopTimer(sessionId);
            }
            
            _sessionService.StartSession(sessionId);
            await Clients.Group(sessionId).SendAsync("HideStartGameButton");
            await Clients.Group(sessionId).SendAsync("GameInProgressMessage", _sessionService.GameInProgress(sessionId));

        }

        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
            _sessionService.StopSession(sessionId);
        }
    }
}