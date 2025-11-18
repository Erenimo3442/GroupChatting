import React, { useState } from 'react';

export default function MessageInput({ onSendMessage, disabled }: {
    onSendMessage: (content: string) => void
    disabled?: boolean
}) {
    const [message, setMessage] = useState('');

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (message.trim() && !disabled) {
            try {
                await onSendMessage(message.trim());
                setMessage('');
            } catch (err) {
                // Error is handled by the parent component
            }
        }
    };

    return (
        <form onSubmit={handleSubmit}
            className="flex gap-2">
            <input
                type="text"
                value={message}
                onChange={(e) =>
                    setMessage(e.target.value)}
                placeholder="Type a message..."
                disabled={disabled}
                className="flex-1 p-2 border
                    rounded-lg focus:outline-none focus:ring-2       
                    focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
            />
            <button
                type="submit"
                disabled={disabled || !message.trim()}
                className="px-4 py-2 bg-blue-500         
                    text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
            >
                {disabled ? (
                    <>
                        <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                        </svg>
                        Sending...
                    </>
                ) : (
                    'Send'
                )}
            </button>
        </form>
    );
}
