import React, { useEffect, useMemo } from "react";
import MessageInput from "./MessageInput";
import Message from "./Message";
import ErrorDisplay from "./ErrorDisplay";
import { useMessages } from "../../hooks/useMessages";
import { useScrollToBottom } from "../../hooks/useScrollToBottom";
import { jwtDecode } from "jwt-decode";

interface ChatWindowProps {
    groupId: string;
}

interface DecodedToken {
    unique_name: string;
}

const ChatWindow = React.memo<ChatWindowProps>(({ groupId }) => {
    const {
        messages,
        loading,
        sending,
        error,
        loadMessages,
        sendMessage,
        clearError,
        retryLastAction,
    } = useMessages();

    const { containerRef, scrollToBottom } = useScrollToBottom([messages]);

    // Get current username to identify own messages
    const currentUsername = useMemo(() => {
        const token = localStorage.getItem('accessToken');
        if (token) {
            try {
                const decoded = jwtDecode<DecodedToken>(token);
                return decoded.unique_name;
            } catch {
                return null;
            }
        }
        return null;
    }, []);

    useEffect(() => {
        if (groupId) {
            loadMessages(groupId);
            import('../../services/signalr').then(({ signalRService }) => {
                signalRService.joinGroup(groupId);
            });

            return () => {
                import('../../services/signalr').then(({ signalRService }) => {
                    signalRService.leaveGroup(groupId);
                });
            };
        }
    }, [groupId, loadMessages]);

    const handleSendMessage = async (content: string) => {
        await sendMessage(groupId, content);
        scrollToBottom();
    };

    const handleFileUpload = async (file: File) => {

        console.log('File uploaded:', file.name);
        scrollToBottom();
    };

        const messageList = useMemo(() => {
            return messages.map(({ id, content, senderUsername, timestamp, isEdited, hasFile, fileUrl, mimeType }) => (
                <Message
                    key={id}
                    content={content}
                    senderUsername={senderUsername}
                    timestamp={timestamp}
                    isEdited={isEdited}
                    isOwn={currentUsername === senderUsername}
                    hasFile={hasFile}         
                    fileUrl={fileUrl}         
                    mimeType={mimeType}       
                />
            ));
        }, [messages, currentUsername]);

    if (loading && messages.length === 0) {
        return (
            <div className="h-full flex flex-col items-center justify-center bg-zinc-950/50">
                <div className="flex flex-col items-center gap-4">
                    <div className="relative w-16 h-16">
                        <div className="absolute inset-0 border-4 border-zinc-800 rounded-full"></div>
                        <div className="absolute inset-0 border-4 border-indigo-500 rounded-full border-t-transparent animate-spin"></div>
                    </div>
                    <p className="text-zinc-500 font-medium animate-pulse">Loading messages...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="h-full flex flex-col bg-zinc-950 relative overflow-hidden">
            {/* Background Pattern */}
            <div className="absolute inset-0 opacity-5 pointer-events-none bg-[radial-gradient(#6366f1_1px,transparent_1px)] [background-size:16px_16px]"></div>

            <div
                ref={containerRef}
                className="flex-grow overflow-y-auto px-4 py-6 space-y-2 relative z-10"
                role="log"
                aria-live="polite"
                aria-label="Messages"
            >
                {messageList.length > 0 ? (
                    messageList
                ) : (
                    <div className="h-full flex flex-col items-center justify-center text-zinc-600 gap-4">
                        <div className="w-16 h-16 bg-zinc-900 rounded-2xl flex items-center justify-center transform rotate-12">
                            <svg className="w-8 h-8 text-zinc-700" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                            </svg>
                        </div>
                        <p className="text-lg font-medium">No messages yet</p>
                        <p className="text-sm opacity-60">Be the first to say hello!</p>
                    </div>
                )}
            </div>

            <div className="flex-shrink-0 relative z-20">
                <ErrorDisplay
                    error={error}
                    onDismiss={clearError}
                    onRetry={retryLastAction}
                />
                <MessageInput
                    onSendMessage={handleSendMessage}
                    onSendFile={handleFileUpload}
                    disabled={sending}
                    groupId={groupId}
                />
            </div>
        </div>
    );
});

ChatWindow.displayName = 'ChatWindow';

export default ChatWindow;
