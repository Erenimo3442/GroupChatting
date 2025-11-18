import React, { useState } from 'react';

interface MessageProps {
    content: string;
    senderUsername: string;
    timestamp: Date | string;
    isEdited: boolean;
    isOwn?: boolean;
}

const Message = React.memo<MessageProps>(({ 
    content, 
    senderUsername,
    timestamp, 
    isEdited,
    isOwn = false
}) => {
    const [isExpanded, setIsExpanded] = useState(false);

    const formatTimestamp = (timestamp: Date | string) => {
        const date = new Date(timestamp);
        const now = new Date();
        const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

        if (diffInHours < 24) {
            return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        } else if (diffInHours < 24 * 7) {
            return date.toLocaleDateString([], { weekday: 'short', hour: '2-digit', minute: '2-digit' });
        } else {
            return date.toLocaleDateString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
        }
    };

    const handleMessageClick = () => {
        setIsExpanded(!isExpanded);
    };

    return (
        <div 
            className={`group relative mb-3 p-3 rounded-lg transition-all duration-200 ${
                isOwn 
                    ? 'bg-blue-50 ml-8 border-l-4 border-blue-400' 
                    : 'bg-gray-50 mr-8 border-l-4 border-gray-300'
            } hover:shadow-md`}
            role="article"
            aria-label={`Message from ${senderUsername} at ${formatTimestamp(timestamp)}`}
        >
            <div className="flex justify-between items-start mb-2">
                <div className="flex items-center gap-2">
                    <div className="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center text-xs font-semibold text-gray-600">
                        {senderUsername.charAt(0).toUpperCase()}
                    </div>
                    <span className={`font-semibold text-sm ${
                        isOwn ? 'text-blue-700' : 'text-gray-700'
                    }`}>
                        {senderUsername}
                    </span>
                </div>
                <div className="flex items-center gap-2">
                    <span className="text-xs text-gray-500">
                        {formatTimestamp(timestamp)}
                    </span>
                    {isEdited && (
                        <span className="text-xs text-gray-400 italic">(edited)</span>
                    )}
                </div>
            </div>
            
            <div 
                className={`text-sm leading-relaxed ${
                    isExpanded ? '' : 'line-clamp-3'
                } cursor-pointer`}
                onClick={handleMessageClick}
                title={isExpanded ? 'Click to collapse' : 'Click to expand'}
            >
                {content}
            </div>

            {content.length > 100 && (
                <button
                    onClick={handleMessageClick}
                    className="text-xs text-blue-500 hover:text-blue-700 mt-1 underline"
                    aria-expanded={isExpanded}
                    aria-controls="message-content"
                >
                    {isExpanded ? 'Show less' : 'Show more'}
                </button>
            )}

            <div className="absolute -right-2 top-2 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                <div className="flex gap-1">
                    <button
                        className="p-1 bg-gray-200 rounded hover:bg-gray-300 text-gray-600 text-xs"
                        title="React to message"
                        aria-label="React to message"
                    >
                        😊
                    </button>
                    <button
                        className="p-1 bg-gray-200 rounded hover:bg-gray-300 text-gray-600 text-xs"
                        title="Reply to message"
                        aria-label="Reply to message"
                    >
                        ↩️
                    </button>
                </div>
            </div>
        </div>
    );
});

Message.displayName = 'Message';

export default Message;
