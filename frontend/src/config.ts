const metaEnv = (typeof import.meta !== 'undefined' && (import.meta as any).env) ?? {};
const nodeEnv = (typeof process !== 'undefined' && process.env) ?? {};
const runtimeEnv = (typeof window !== 'undefined' && (window as any).__ENV__) ?? {};

const readEnv = (...keys: string[]) => {
    for (const key of keys) {
        const value = (metaEnv as Record<string, string | undefined>)[key]
            ?? (nodeEnv as Record<string, string | undefined>)[key]
            ?? (runtimeEnv as Record<string, string | undefined>)[key];
        if (value) {
            return value;
        }
    }
    return undefined;
};

const defaultApiBaseUrl = typeof window !== 'undefined' ? window.location.origin : 'http://localhost:8080';

export const API_BASE_URL = readEnv('API_BASE_URL', 'VITE_API_BASE_URL') ?? defaultApiBaseUrl;
export const API_BASE_PATH = readEnv('API_BASE_PATH', 'VITE_API_BASE_PATH') ?? `${API_BASE_URL}/api`;
export const SIGNALR_HUB_URL = readEnv('SIGNALR_HUB_URL', 'VITE_SIGNALR_HUB_URL') ?? `${API_BASE_URL}/chathub`;
