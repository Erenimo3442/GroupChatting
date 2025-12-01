import { API_BASE_PATH, API_BASE_URL } from './config';
import { clearTokens, getAccessToken, getRefreshToken, saveTokens } from './utils/clearTokens';

class ApiClient {
    private baseURL = API_BASE_PATH;
    private refreshEndpoint = `${API_BASE_URL}/api/Auth/refresh`;
    private refreshPromise: Promise<string | null> | null = null;

    private getAuthHeaders(): Record<string, string> {
        const token = getAccessToken();
        return {
            ...(token ? { "Authorization": `Bearer ${token}` } : {})
        }
    }

    private async handleResponse<T>(response: Response): Promise<T> {
        const contentType = response.headers.get("content-type") || "";
        const isJson = contentType.includes("application/json");
        const data = isJson ? await response.json().catch(() => null) : null;

        if (!response.ok) {
            const appError = new Error(JSON.stringify({
                name: "AppError",
                status: response.status,
                code: data?.code ?? "unknown_error",
                title: data?.title ?? response.statusText,
                detail: data?.detail ?? "Something went wrong."
            }));
            throw appError;
        }
        return data as T;
    }

    private async tryRefreshToken(): Promise<string | null> {
        if (this.refreshPromise) {
            return this.refreshPromise;
        }

        const refreshToken = getRefreshToken();
        if (!refreshToken) {
            return null;
        }

        this.refreshPromise = (async () => {
            try {
                const response = await window.fetch(this.refreshEndpoint, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ refreshToken })
                });

                if (!response.ok) {
                    clearTokens();
                    return null;
                }

                const data = await response.json();
                if (data?.accessToken && data?.refreshToken) {
                    saveTokens(data.accessToken, data.refreshToken);
                    return data.accessToken as string;
                }

                return null;
            }
            finally {
                this.refreshPromise = null;
            }
        })();

        return this.refreshPromise;
    }

    // Custom fetch method that integrates with NSwag generated clients
    fetch = async (url: string, init?: RequestInit): Promise<Response> => {
        const authHeaders = this.getAuthHeaders();

        const headers = {
            ...authHeaders,
            ...(init?.headers || {})
        };

        const initialResponse = await window.fetch(url, {
            ...init,
            headers
        });

        if (initialResponse.status !== 401) {
            return initialResponse;
        }

        const refreshedToken = await this.tryRefreshToken();
        if (!refreshedToken) {
            return initialResponse;
        }

        const retryHeaders = {
            ...headers,
            Authorization: `Bearer ${refreshedToken}`
        };

        return window.fetch(url, {
            ...init,
            headers: retryHeaders
        });
    }

    async get<T>(endpoint: string): Promise<T> {
        const res = await this.fetch(`${this.baseURL}${endpoint}`, {
            method: 'GET',
        });
        return this.handleResponse<T>(res);
    }

    async post<T>(endpoint: string, data: any): Promise<T> {
        const res = await this.fetch(`${this.baseURL}${endpoint}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        });
        return this.handleResponse<T>(res);
    }

    async put<T>(endpoint: string, data: any): Promise<T> {
        const res = await this.fetch(`${this.baseURL}${endpoint}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        });
        return this.handleResponse<T>(res);
    }

    async delete<T>(endpoint: string): Promise<T> {
        const res = await this.fetch(`${this.baseURL}${endpoint}`, {
            method: 'DELETE',
        });
        return this.handleResponse<T>(res);
    }
};

const apiClient = new ApiClient();
export default apiClient;

// Export the fetch method for NSwag clients
export const { fetch } = apiClient;
