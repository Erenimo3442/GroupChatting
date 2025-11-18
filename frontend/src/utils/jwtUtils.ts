import { jwtDecode } from 'jwt-decode';

interface JwtPayload {
  unique_name: string; // username (backend uses UniqueName claim)
  nameid: string; // user ID (backend uses NameId claim)
  exp: number; // expiration (auto-added by JWT library)
}

/**
 * Check if a JWT token is expired
 * @param token The JWT token to check
 * @returns boolean indicating if token is expired
 */
export const isTokenExpired = (token: string): boolean => {
  try {
    const decoded = jwtDecode<JwtPayload>(token);
    const currentTime = Math.floor(Date.now() / 1000);
    return decoded.exp < currentTime;
  } catch (error) {
    console.error('Error decoding JWT:', error);
    return true; // Assume expired if we can't decode it
  }
};

/**
 * Check if a JWT token is valid (not expired and properly formatted)
 * @param token The JWT token to validate
 * @returns boolean indicating if token is valid
 */
export const isTokenValid = (token: string): boolean => {
  if (!token || typeof token !== 'string') {
    return false;
  }
  
  try {
    const decoded = jwtDecode<JwtPayload>(token);
    const currentTime = Math.floor(Date.now() / 1000);
    
    // Check if token has required claims
    if (!decoded.exp || !decoded.unique_name) {
      return false;
    }
    
    // Check if token is expired
    return decoded.exp > currentTime;
  } catch (error) {
    console.error('Error validating JWT:', error);
    return false;
  }
};

/**
 * Get time until token expires (in seconds)
 * @param token The JWT token to check
 * @returns number of seconds until expiration, or 0 if expired/invalid
 */
export const getTimeUntilExpiration = (token: string): number => {
  try {
    const decoded = jwtDecode<JwtPayload>(token);
    const currentTime = Math.floor(Date.now() / 1000);
    return Math.max(0, decoded.exp - currentTime);
  } catch (error) {
    console.error('Error getting token expiration:', error);
    return 0;
  }
};

/**
 * Get username from JWT token
 * @param token The JWT token to decode
 * @returns username from token or null if invalid
 */
export const getUsernameFromToken = (token: string): string | null => {
  try {
    const decoded = jwtDecode<JwtPayload>(token);
    return decoded.unique_name || null;
  } catch (error) {
    console.error('Error extracting username from JWT:', error);
    return null;
  }
};

/**
 * Check if token will expire soon (within 5 minutes)
 * @param token The JWT token to check
 * @returns boolean indicating if token expires within 5 minutes
 */
export const willTokenExpireSoon = (token: string): boolean => {
  const timeUntilExpiration = getTimeUntilExpiration(token);
  return timeUntilExpiration < 300; // 5 minutes = 300 seconds
};
