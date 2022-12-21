using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
	class SessionManager
	{
		static SessionManager _session = new SessionManager();
		public static SessionManager Instance { get { return _session; } }

		int _sessionId = 0;
		Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
		object _lock = new object();

        // 연결된 세션들로 몇 백명이 접속중인지 반환
        public int GetBusyScore()
        {
            int count = 0;

            lock (_lock)
            {
                count = _sessions.Count;
            }

            return count / 100;
        }

        public List<ClientSession> GetSessions()
        {
			List<ClientSession> sessions = new List<ClientSession>();
			
			lock(_lock)
            {
				sessions = _sessions.Values.ToList();
            }
			return sessions;
        }

		public ClientSession Generate()
		{
			lock (_lock)
			{
				int sessionId = ++_sessionId;

				ClientSession session = new ClientSession();
				session.SessionId = sessionId;
				_sessions.Add(sessionId, session);

				Console.WriteLine($"Connected ({_sessions.Count}) Players");

				return session;
			}
		}

		public ClientSession Find(int id)
		{
			lock (_lock)
			{
				ClientSession session = null;
				_sessions.TryGetValue(id, out session);
				return session;
			}
		}

		public void Remove(ClientSession session)
		{
			lock (_lock)
			{
				_sessions.Remove(session.SessionId);
                Console.WriteLine($"Connected ({_sessions.Count}) Players");
			}
		}
	}
}
