import { jwtDecode } from 'jwt-decode';

interface HeaderProps {
    onLogout: () => void;
}

interface DecodedToken {
    unique_name: string;
    // Add other claims if needed
}

export default function Header({ onLogout }: HeaderProps) {
    const token = localStorage.getItem('accessToken');
    let username = 'Guest';

    if (token) {
        try {
            const decodedToken = jwtDecode<DecodedToken>(token);
            // The claim name in the token is 'unique_name'
            username = decodedToken.unique_name;
        } catch (error) {
            console.error("Failed to decode token", error);
        }
    }

    return (
        <header className="app-header">
            <h1>Chat App</h1>
            <div className="auth-info">
                <span>Welcome, {username}</span>
                <button onClick={onLogout} className="logout-button">Logout</button>
            </div>
        </header>
    );
};