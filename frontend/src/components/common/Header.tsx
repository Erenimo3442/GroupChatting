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
        <header className="h-16 bg-zinc-900 border-b border-zinc-800 flex justify-between items-center px-6">
            <div className="flex items-center gap-3">
                <div className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center">
                    <svg className="w-5 h-5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
                    </svg>
                </div>
                <h1 className="text-xl font-bold text-white tracking-tight">Chat App</h1>
            </div>

            <div className="flex items-center gap-4">
                <div className="flex items-center gap-3 px-3 py-1.5 bg-zinc-800 rounded-full border border-zinc-700">
                    <div className="w-6 h-6 bg-zinc-700 rounded-full flex items-center justify-center text-xs font-bold text-zinc-300">
                        {username.charAt(0).toUpperCase()}
                    </div>
                    <span className="text-sm font-medium text-zinc-300 pr-1">{username}</span>
                </div>
                <button
                    onClick={onLogout}
                    className="p-2 text-zinc-400 hover:text-red-400 hover:bg-red-400/10 rounded-lg transition-colors"
                    title="Logout"
                >
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                    </svg>
                </button>
            </div>
        </header>
    );
};