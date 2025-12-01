export const TOKENS_UPDATED_EVENT = 'auth:tokens-updated';

export const getAccessToken = () => localStorage.getItem('accessToken');

export const getRefreshToken = () => localStorage.getItem('refreshToken');

export const hasTokens = () => {
  const accessToken = getAccessToken();
  const refreshToken = getRefreshToken();
  return !!(accessToken && refreshToken);
};

export const saveTokens = (accessToken: string, refreshToken: string) => {
  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('refreshToken', refreshToken);
  window.dispatchEvent(new CustomEvent(TOKENS_UPDATED_EVENT, { detail: { accessToken, refreshToken } }));
};

// Utility to clear all authentication tokens
export const clearTokens = () => {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  window.dispatchEvent(new CustomEvent(TOKENS_UPDATED_EVENT, { detail: { accessToken: null, refreshToken: null } }));
  console.log('🧹 All tokens cleared from localStorage');
};

// Utility to clear tokens and reload the page for fresh start
export const resetAuthentication = () => {
  clearTokens();
  console.log('🔄 Resetting authentication - page will reload');
  window.location.reload();
};
