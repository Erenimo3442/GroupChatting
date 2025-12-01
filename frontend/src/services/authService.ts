import { Client, LoginDto, RegisterDto } from "../generated/api-client";
import { fetch } from "../httpClient";
import { API_BASE_URL } from "../config";

const authClient = new Client(API_BASE_URL, { fetch });

export const loginUser = async (username: string, password: string) => {
    const loginDto = new LoginDto({ username, password });
    const response = await authClient.login(loginDto);
    return response;
};

export const registerUser = async (username: string, password: string) => {
    const registerDto = new RegisterDto({ username, password });
    const response = await authClient.register(registerDto);
    return response;
};
