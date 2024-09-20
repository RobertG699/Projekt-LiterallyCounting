//Robert//
using Microsoft.AspNetCore.SignalR;
using MvcLoginApp.Hubs;
using System.Collections.Concurrent;
using MySQLiteApp.Game;
using MySQLiteApp;
namespace MvcLoginApp.Services
{
    public class SessionService
    {
        private readonly IHubContext<GameHub> _hubContext;
        private static ConcurrentDictionary<string, SessionState> sessions = new ConcurrentDictionary<string, SessionState>() ;

        public SessionService(IHubContext<GameHub> hubContext)
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

        public bool GameInProgress(string sessionId)
        {
            var state = sessions[sessionId];

            if(state.round == 0){
                return false;
            }
            else{
                return true;
            }
        }

        public void AddPlayerToSession(string sessionId, string user)
        {
            var state = sessions[sessionId];

            state.game.PlayerJoin(user);
        }

        public bool CheckSolution(string sessionId, string user, int solution)
        {
            var state = sessions[sessionId];
            return state.game.Answer(user, solution, state.countdownValue);
        }

        public bool PlayerCanSubmitAnwser(string sessionId, string user){
            var state = sessions[sessionId];

            Player player = state.game.GetPlayer(user);

            if(player.username == "" || player.answerd || player.fails >= 3){
                return false;
            }
            else{
                return true;
            }
        }

        public List<Player> GetAllPlayers(string sessionId){
            var state = sessions[sessionId];

            return state.game.GetAllPlayers();
        }

        public void StartSession(string sessionId)
        {
            var state = sessions[sessionId];

            state.game.StartGame();
            state.countdownValue = 20;
            state.selectedWord = SelectRandomWord(sessionId);
            //Displays word AND number of letters for testing purposes
            //state.selectedWord = state.selectedWord + " " + state.game.CountDidstinctLetters(state.selectedWord);
            state.round = 1;
            state.timer = new Timer(async _ =>
            {
                var sessionState = sessions[sessionId];
                sessionState.countdownValue--;

                if (sessionState.countdownValue == 0 && sessionState.round != 20){
                    sessionState.selectedWord = SelectRandomWord(sessionId);
                    //Displays word AND number of letters for testing purposes
                    //state.selectedWord = state.selectedWord + " " + state.game.CountDidstinctLetters(state.selectedWord);
                    sessionState.countdownValue = 20;
                    sessionState.round++;
                    sessionState.game.StartRound();
                }

                await SendSessionState(sessionId);

                if (sessionState.countdownValue == 0 && sessionState.round == 20){
                    string winner = sessionState.game.GetWinner().username;
                    await _hubContext.Clients.Group(sessionId).SendAsync("ReceiveGameEndInfo", winner);

                    UpdatePlayerPoints(sessionId);
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
            var state = sessions[sessionId];

            return state.game.GetWord();
        }

        private void UpdatePlayerPoints(string sessionId){
            var state = sessions[sessionId];
            List<Player> players = state.game.GetAllPlayers();

            foreach(Player player in players){
                UserDataAccess.updateUserPoints(player.username, player.score);
            }
        }

        public class SessionState
        {
            public int countdownValue { get; set; }
            public string selectedWord { get; set; }
            public Timer timer { get; set; }
            public int round { get; set; } = 0;
            public Game game { get; set; } = new Game();
        }
    }
}
