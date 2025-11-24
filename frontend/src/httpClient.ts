class ApiClient {
    private baseURL = "http://localhost:8080/api";

    private getAuthHeaders(): Record<string, string> {
        const token = localStorage.getItem('accessToken');
        return {
            ...(token ? { "Authorization": `Bearer ${token}` } : {})
        }
    }

    private async handleResponse<T>(response: Response): Promise<T> {
        const contentType = response.headers.get("content-type") || "";
        const isJson = contentType.includes("application/json");
        const data = isJson ? await response.json().catch(() => null) : null;

        //console.log(data);

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

    // Custom fetch method that integrates with NSwag generated clients
    fetch = async (url: string, init?: RequestInit): Promise<Response> => {
        const authHeaders = this.getAuthHeaders();

        const headers = {
            ...authHeaders,
            ...(init?.headers || {})
        };

        // NSwag clients pass full URLs, so don't concatenate base URL
        return window.fetch(url, {
            ...init,
            headers
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
            body: JSON.stringify(data)
        });
        return this.handleResponse<T>(res);
    }

    async put<T>(endpoint: string, data: any): Promise<T> {
        const res = await this.fetch(`${this.baseURL}${endpoint}`, {
            method: 'PUT',
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
