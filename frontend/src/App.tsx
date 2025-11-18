import { useState, useEffect } from 'react';
import Auth from './components/auth/Auth';
// Import your other components
import Header from './components/common/Header';
import Sidebar from './components/common/Sidebar';
import ChatWindow from './components/chat/ChatWindow';
import { clearTokens, hasTokens } from './utils/clearTokens';
import { isTokenValid, willTokenExpireSoon } from './utils/jwtUtils';
// import ChatWindow from './components/ChatWindow';

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}

export default function App() {
  // Read token from localStorage to stay logged in on page refresh
  const [token, setToken] = useState(localStorage.getItem('accessToken'));
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);
  const [authError, setAuthError] = useState<string | null>(null);

  // Check for authentication errors on mount and when token changes
  useEffect(() => {
    if (token && hasTokens()) {
      // Validate token client-side using JWT library
      const validateToken = () => {
        try {
          // Check if token is valid (not expired and properly formatted)
          if (!isTokenValid(token)) {
            setAuthError('Invalid or expired token. Please login again.');
            console.error('JWT Token is invalid or expired');
            return;
          }
          
          // Check if token will expire soon (within 5 minutes)
          if (willTokenExpireSoon(token)) {
            console.warn('Token will expire soon. Consider refreshing.');
            // Could implement token refresh logic here
          }                  
        } catch (error) {
          console.error('JWT Token validation error:', error);
          setAuthError('Token validation failed. Please login again.');
        }
      };
      
      validateToken();
    }
  }, [token]);

  const handleLoginSuccess = (data: LoginResponse) => {
    // Save tokens to localStorage
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    // Update the state to trigger a re-render
    setToken(data.accessToken);
  };

  const handleLogout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    setToken(null);
  };

  // If there's no token, show the Auth component
  if (!token) {
    return <Auth onLoginSuccess={handleLoginSuccess} />;
  }

  const handleSelectGroupId = (groupId: string) => {
    setSelectedGroupId(groupId);
  };

  const handleClearTokensAndReset = () => {
    clearTokens();
    setToken(null);
    setAuthError(null);
  };

  // If there is an auth error, show error screen
  if (authError) {
    return (
      <div className="flex items-center justify-center h-screen bg-gray-50">
        <div className="max-w-md w-full bg-white rounded-lg shadow-md p-6">
          <div className="text-center">
            <div className="text-red-500 text-6xl mb-4">⚠️</div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Authentication Error</h2>
            <p className="text-gray-600 mb-6">{authError}</p>
            <div className="space-y-3">
              <button
                onClick={handleClearTokensAndReset}
                className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 transition-colors"
              >
                Clear Tokens & Login Again
              </button>
              <button
                onClick={() => setAuthError(null)}
                className="w-full bg-gray-200 text-gray-800 py-2 px-4 rounded-md hover:bg-gray-300 transition-colors"
              >
                Continue Anyway
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // If there is a token, show the main chat application
  return (
    <div className="flex flex-col h-screen bg-gray-50">
      <Header onLogout={handleLogout} />

      <div className="flex flex-auto overflow-hidden">
        {/* Sidebar */}
        <div className="w-1/4 min-w-64 bg-white border-r border-gray-200 p-4 text-cyan-800">
          <Sidebar onSelectGroupId={handleSelectGroupId} />
        </div>

        {/* Chat Area */}
        {selectedGroupId && (
          <div className="flex-1 p-4 text-cyan-800">
            <ChatWindow groupId={selectedGroupId} />
          </div>
        )}
        {!selectedGroupId && (
          <div className="flex-1 p-4 text-cyan-800">
            <p>Please select a group to chat</p>
          </div>
        )}
      </div>
    </div>
  );
}
