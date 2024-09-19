using Microsoft.AspNetCore.SignalR;
using MvcLoginApp.Services;
using MySQLiteApp.Game;

namespace MvcLoginApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly WordService _wordService;

        public ChatHub(WordService wordService)
        {
            _wordService = wordService;
        }

        private async Task RecreatePlayerList(string sessionId){
            await Clients.Group(sessionId).SendAsync("ResetPlayerList");

            List<Player> players = _wordService.GetAllPlayers(sessionId);
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

            if (!_wordService.PlayerCanSubmitAnwser(sessionId, user)){
                msg = "Bitte warte auf die nächste Runde :)";
            }
            else{
                if (_wordService.CheckSolution(sessionId, user, Int32.Parse(solution))){
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
            if(_wordService.SessionExists(sessionId)){
                await _wordService.SendSessionState(sessionId);
                await Clients.Caller.SendAsync("HideStartGameButton");
            }
            else{
                _wordService.InitiateSession(sessionId);
                await _wordService.SendSessionState(sessionId);
            }

            _wordService.AddPlayerToSession(sessionId, user);
            await RecreatePlayerList(sessionId);
            await Clients.Group(sessionId).SendAsync("GameInProgressMessage", _wordService.GameInProgress(sessionId));
        }

        public async Task StartSession(string sessionId)
        {
            if(_wordService.SessionExists(sessionId)){
                _wordService.StopTimer(sessionId);
            }
            
            _wordService.StartSession(sessionId);
            await Clients.Group(sessionId).SendAsync("HideStartGameButton");
            await Clients.Group(sessionId).SendAsync("GameInProgressMessage", _wordService.GameInProgress(sessionId));

        }

        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
            _wordService.StopSession(sessionId);
        }
    }
}