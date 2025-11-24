import { Client, LoginDto, RegisterDto } from "../generated/api-client";
import { fetch } from "../httpClient";

const authClient = new Client("http://localhost:8080", { fetch });

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
