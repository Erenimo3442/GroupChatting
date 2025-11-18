// Utility to clear all authentication tokens
export const clearTokens = () => {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  console.log('🧹 All tokens cleared from localStorage');
};

// Utility to check if tokens exist and are potentially valid
export const hasTokens = () => {
  const accessToken = localStorage.getItem('accessToken');
  const refreshToken = localStorage.getItem('refreshToken');
  return !!(accessToken && refreshToken);
};

// Utility to clear tokens and reload the page for fresh start
export const resetAuthentication = () => {
  clearTokens();
  console.log('🔄 Resetting authentication - page will reload');
  window.location.reload();
};
