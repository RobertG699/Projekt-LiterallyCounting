using Microsoft.AspNetCore.SignalR;
using MvcLoginApp.Hubs;
using System.Collections.Concurrent;
using MySQLiteApp.Game;

namespace MvcLoginApp.Services
{
    public class WordService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private static ConcurrentDictionary<string, SessionState> sessions = new ConcurrentDictionary<string, SessionState>() ;
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
            var state = sessions[sessionId];
            if(state.timer == null){
                return;
            }

            await _hubContext.Clients.Group(sessionId).SendAsync(
                        "ReceiveSessionState",
                        sessions[sessionId].selectedWord,
                        sessions[sessionId].countdownValue,
                        sessions[sessionId].round);
        }

        public void InitiateSession(string sessionId)
        {
            var state = new SessionState();

            sessions[sessionId] = state;
        }

        public void StartSession(string sessionId)
        {
            var state = sessions[sessionId];

            state.countdownValue = 10;
            state.selectedWord = SelectRandomWord(sessionId);
            state.round = 1;
            state.timer = new Timer(async _ =>
            {
                var sessionState = sessions[sessionId];
                sessionState.countdownValue--;

                if (sessionState.countdownValue == 0 && sessionState.round != 3)
                {
                    sessionState.selectedWord = SelectRandomWord(sessionId);
                    sessionState.countdownValue = 10;
                    sessionState.round++;
                }

                await SendSessionState(sessionId);

                if (sessionState.countdownValue == 0 && sessionState.round == 3)
                {
                    string winner = "Tom";
                    await _hubContext.Clients.Group(sessionId).SendAsync(
                        "ReceiveGameEndInfo",
                        winner);
                    StopSession(sessionId);
                }
            }, null, 0, 1000);
        }

        public void StopTimer(string sessionId)
        {
            var state = sessions[sessionId];
            if(state.timer != null){
                state.timer.Dispose();
            }
        }

        public void StopSession(string sessionId)
        {
            if (sessions.TryRemove(sessionId, out var state))
            {
                state.timer.Dispose();
            }
        }

        private string SelectRandomWord(string sessionId)
        {
            //var state = sessions[sessionId];

            //return state.game.GetWord();
            int randomIndex = random.Next(words.Count);
            return words[randomIndex];
        }

        public class SessionState
        {
            public int countdownValue { get; set; }
            public string selectedWord { get; set; }
            public Timer timer { get; set; }
            public int round { get; set; }
            public Game game { get; set; } = new Game();
        }
    }
}
