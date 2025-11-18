import React, { useState } from 'react';
import { loginUser, registerUser } from '../../services/authService';

interface AuthProps {
    onLoginSuccess: (data: { accessToken: string; refreshToken: string }) => void;
}


// Accept the onLoginSuccess function as a prop
export default function Auth({ onLoginSuccess }: AuthProps) {
    const [isLogin, setIsLogin] = useState(true);
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError('');

        try {
            if (isLogin) {
                const data = await loginUser(username, password);
                // Call the function passed down from App.jsx
                console.log('Auth.tsx :24', data);
                onLoginSuccess(data);
            } else {
                await registerUser(username, password);
                alert('Registration successful! Please log in.');
                setIsLogin(true);
            }
        } catch (err: any) {
            setError(err.message);
            console.error(err);
        }
    };

    return (
        <div className="auth-container">
            <form onSubmit={handleSubmit} className="auth-form">
                <h2>{isLogin ? 'Login' : 'Register'}</h2>
                {error && <p style={{ color: 'red' }}>{error}</p>}
                <div className="form-group">
                    <label htmlFor="username">Username</label>
                    <input
                        type="text"
                        id="username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="password">Password</label>
                    <input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                <button type="submit" className="auth-button">
                    {isLogin ? 'Login' : 'Register'}
                </button>
                <p className="toggle-auth" onClick={() => setIsLogin(!isLogin)}>
                    {isLogin ? 'Need an account? Register' : 'Have an account? Login'}
                </p>
            </form>
        </div>
    );
};