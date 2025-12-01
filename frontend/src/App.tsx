import { useState, useEffect, useRef } from 'react';
import Auth from './components/auth/Auth';
import Header from './components/common/Header';
import Sidebar from './components/common/Sidebar';
import ChatWindow from './components/chat/ChatWindow';
import { TOKENS_UPDATED_EVENT, clearTokens, getAccessToken, hasTokens, saveTokens } from './utils/clearTokens';
import { isTokenValid, willTokenExpireSoon } from './utils/jwtUtils';
import { signalRService, SignalRService } from './services/signalr';

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}

export default function App() {
  const [token, setToken] = useState(getAccessToken());
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);
  const [authError, setAuthError] = useState<string | null>(null);
  const connectionRef = useRef<SignalRService | null>(null);

  useEffect(() => {
    if (!token || !hasTokens()) {
      return;
    }

    const validateToken = () => {
      try {
        if (!isTokenValid(token)) {
          setAuthError('Invalid or expired token. Please login again.');
          console.error('JWT Token is invalid or expired');
          return;
        }

        if (willTokenExpireSoon(token)) {
          console.warn('Token will expire soon. Attempting refresh automatically.');
        }
      } catch (error) {
        console.error('JWT Token validation error:', error);
        setAuthError('Token validation failed. Please login again.');
      }
    };

    validateToken();
  }, [token]);

  useEffect(() => {
    const handleTokensUpdated = (event: Event) => {
      const customEvent = event as CustomEvent<{ accessToken: string | null } | undefined>;
      const updatedToken = customEvent.detail?.accessToken ?? null;
      setToken(updatedToken);
    };

    window.addEventListener(TOKENS_UPDATED_EVENT, handleTokensUpdated as EventListener);
    return () => window.removeEventListener(TOKENS_UPDATED_EVENT, handleTokensUpdated as EventListener);
  }, []);

  useEffect(() => {
    if (!token ||
      !hasTokens() ||
      connectionRef.current) {
      return;
    }
    connectionRef.current = signalRService;

    signalRService.setToken(token);
    signalRService.startConnection();

    return () => {
      connectionRef.current?.stopConnection();
      connectionRef.current = null;
    };
  }, [token]);

  const handleLoginSuccess = (data: LoginResponse) => {
    saveTokens(data.accessToken, data.refreshToken);
    setToken(data.accessToken);
  };

  const handleLogout = () => {
    connectionRef.current?.stopConnection();
    clearTokens();
    setToken(null);
    setSelectedGroupId(null);
  };

  const handleSelectGroupId = (groupId: string) => {
    setSelectedGroupId(groupId);
  };

  const handleClearTokensAndReset = () => {
    clearTokens();
    setToken(null);
    setAuthError(null);
  };

  if (!token) {
    return <Auth onLoginSuccess={handleLoginSuccess} />;
  }

  if (authError) {
    return (
      <div className="flex items-center justify-center h-screen bg-zinc-950">
        <div className="max-w-md w-full bg-zinc-900 rounded-xl shadow-2xl border border-zinc-800 p-8 text-center">
          <div className="w-16 h-16 bg-red-500/10 rounded-full flex items-center justify-center mx-auto mb-6">
            <svg className="w-8 h-8 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
          <h2 className="text-2xl font-bold text-zinc-100 mb-2">Authentication Error</h2>
          <p className="text-zinc-400 mb-8">{authError}</p>
          <div className="space-y-3">
            <button
              onClick={handleClearTokensAndReset}
              className="w-full btn-primary"
            >
              Clear Tokens & Login Again
            </button>
            <button
              onClick={() => setAuthError(null)}
              className="w-full btn-secondary"
            >
              Continue Anyway
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-screen bg-zinc-950 text-zinc-100 overflow-hidden">
      <Header onLogout={handleLogout} />

      <div className="flex flex-1 overflow-hidden">
        <Sidebar onSelectGroupId={handleSelectGroupId} />

        <main className="flex-1 flex flex-col min-w-0 bg-zinc-950 relative">
          {selectedGroupId ? (
            <ChatWindow groupId={selectedGroupId} />
          ) : (
            <div className="flex-1 flex flex-col items-center justify-center text-zinc-500 p-8 text-center">
              <div className="w-24 h-24 bg-zinc-900 rounded-3xl flex items-center justify-center mb-6 transform rotate-6 shadow-2xl shadow-black/50">
                <svg className="w-12 h-12 text-zinc-700" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 8h2a2 2 0 012 2v6a2 2 0 01-2 2h-2v4l-4-4H9a1.994 1.994 0 01-1.414-.586m0 0L11 14h4a2 2 0 002-2V6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2v4l.586-.586z" />
                </svg>
              </div>
              <h2 className="text-2xl font-bold text-zinc-300 mb-2">Select a Group</h2>
              <p className="max-w-md text-zinc-600">
                Choose a group from the sidebar to start chatting or create a new one to invite your friends.
              </p>
            </div>
          )}
        </main>
      </div>
    </div>
  );
}
