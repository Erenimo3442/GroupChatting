import React, { useState } from 'react';

interface MessageProps {
    content: string;
    senderUsername: string;
    timestamp: Date | string;
    isEdited: boolean;
    isOwn?: boolean;
    hasFile?: boolean;        // ← ADD
    fileUrl?: string;         // ← ADD
    mimeType?: string;        // ← ADD
}

const Message = React.memo<MessageProps>(({
    content,
    senderUsername,
    timestamp,
    isEdited,
    isOwn = false,
    hasFile = false,          // ← ADD
    fileUrl,                  // ← ADD
    mimeType                  // ← ADD
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

    const isImage = mimeType?.startsWith('image/');
    const fullFileUrl = fileUrl?.startsWith('http') ? fileUrl : `http://localhost:8080${fileUrl}`;

    return (
        <div
            className={`flex w-full mb-4 ${isOwn ? 'justify-end' : 'justify-start'}`}
            role="article"
        >
            <div className={`flex max-w-[80%] ${isOwn ? 'flex-row-reverse' : 'flex-row'} gap-3`}>
                {/* Avatar */}
                <div className={`
                    flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold shadow-lg
                    ${isOwn
                        ? 'bg-indigo-600 text-white'
                        : 'bg-zinc-700 text-zinc-300'
                    }
                `}>
                    {senderUsername.charAt(0).toUpperCase()}
                </div>

                {/* Message Content */}
                <div className={`flex flex-col ${isOwn ? 'items-end' : 'items-start'}`}>
                    <div className="flex items-baseline gap-2 mb-1 px-1">
                        <span className={`text-xs font-medium ${isOwn ? 'text-indigo-400' : 'text-zinc-400'}`}>
                            {senderUsername}
                        </span>
                        <span className="text-[10px] text-zinc-600">
                            {formatTimestamp(timestamp)}
                        </span>
                    </div>

                    <div
                        className={`
                            relative px-4 py-2.5 rounded-2xl shadow-md text-sm leading-relaxed break-words
                            ${isOwn
                                ? 'bg-indigo-600 text-white rounded-tr-none'
                                : 'bg-zinc-800 text-zinc-200 rounded-tl-none border border-zinc-700'
                            }
                        `}
                    >

                        {/* File Attachment Rendering */}
                        {hasFile && fileUrl && (
                            <div className="mb-2">
                                {isImage ? (
                                    <a href={fullFileUrl} target="_blank" rel="noopener noreferrer">
                                        <img 
                                            src={fullFileUrl} 
                                            alt="Uploaded file"
                                            className="max-w-sm max-h-64 rounded-lg cursor-pointer hover:opacity-90 transition"
                                        />
                                    </a>
                                ) : (
                                    <a 
                                        href={fullFileUrl} 
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="flex items-center gap-2 p-3 bg-black/20 rounded-lg hover:bg-black/30 transition"
                                    >
                                        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                                        </svg>
                                        <span className="text-sm">View File</span>
                                    </a>
                                )}
                            </div>
                        )}
                        {content}
                        {isEdited && (
                            <span className="text-[10px] opacity-60 italic ml-2 block text-right">
                                (edited)
                            </span>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
});

Message.displayName = 'Message';

export default Message;
