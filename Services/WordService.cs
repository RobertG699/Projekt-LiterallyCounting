using Microsoft.AspNetCore.SignalR;
using MvcLoginApp.Hubs;
using System.Collections.Concurrent;
namespace MvcLoginApp.Services
{
    public class WordService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private static ConcurrentDictionary<string, SessionState> sessions = new ConcurrentDictionary<string, SessionState>();
        private static List<string> words = new List<string> { "apple", "banana", "cherry", "date", "elderberry" };
        private static Random random = new Random();

        public WordService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public bool SessionExists(string sessionId){
            bool res = false;

            foreach(string id in sessions.Keys){
                if(id == sessionId){
                    return true;
                }
            }
            return res;
        }

        public async Task SendSessionState(string sessionId){
            await _hubContext.Clients.Group(sessionId).SendAsync(
                        "ReceiveSessionState",
                        sessions[sessionId].selectedWord,
                        sessions[sessionId].countdownValue,
                        sessions[sessionId].round);
        }

        public void StartSession(string sessionId)
        {
            var state = new SessionState
            {
                countdownValue = 10,
                selectedWord = SelectRandomWord(),
                round = 1,
                timer = new Timer(async _ =>
                {
                    var sessionState = sessions[sessionId];
                    sessionState.countdownValue--;

                    if (sessionState.countdownValue == 0 && sessionState.round != 3)
                    {
                        sessionState.selectedWord = SelectRandomWord();
                        sessionState.countdownValue = 10;
                        sessionState.round++;
                    }

                    await SendSessionState(sessionId);

                    if(sessionState.countdownValue == 0 && sessionState.round == 3){
                        string winner = "Tom";
                        await _hubContext.Clients.Group(sessionId).SendAsync(
                            "ReceiveGameEndInfo",
                            winner);
                        StopSession(sessionId);
                    }
                }, null, 0, 1000)
            };

            sessions[sessionId] = state;
        }

        public void StopSession(string sessionId)
        {
            if (sessions.TryRemove(sessionId, out var state))
            {
                state.timer.Dispose();
            }
        }

        private string SelectRandomWord()
        {
            int randomIndex = random.Next(words.Count);
            return words[randomIndex];
        }

        private class SessionState
        {
            public int countdownValue { get; set; }
            public string selectedWord { get; set; }
            public Timer timer { get; set; }
            public int round { get; set; }
        }
    }
}
