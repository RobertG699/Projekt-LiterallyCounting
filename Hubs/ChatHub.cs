using Microsoft.AspNetCore.SignalR;
using MvcLoginApp.Services;

namespace MvcLoginApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly WordService _wordService;

        public ChatHub(WordService wordService)
        {
            _wordService = wordService;
        }

        public async Task SendMessage(string sessionId, string user, string message)
        {
            await Clients.Group(sessionId).SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendSolution(string sessionId, string user, string solution)
        {
            Console.WriteLine(sessionId + " " + user + " " + solution + " ");
        }

        public async Task JoinSession(string sessionId, string user, int points)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            if(_wordService.SessionExists(sessionId)){
                await _wordService.SendSessionState(sessionId);
            }
            else{
                _wordService.InitiateSession(sessionId);
                await _wordService.SendSessionState(sessionId);
            }

            _wordService.AddPlayerToSession(sessionId, user);
            await Clients.Group(sessionId).SendAsync("PlayerJoined", user, points);
        }

        public async Task StartSession(string sessionId)
        {
            if(_wordService.SessionExists(sessionId)){
                _wordService.StopTimer(sessionId);
            }
            
            _wordService.StartSession(sessionId);
        }

        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
            _wordService.StopSession(sessionId);
        }
    }
}