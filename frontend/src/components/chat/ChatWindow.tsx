import React, { useEffect, useMemo } from "react";
import MessageInput from "./MessageInput";
import Message from "./Message";
import ErrorDisplay from "./ErrorDisplay";
import { useMessages } from "../../hooks/useMessages";
import { useScrollToBottom } from "../../hooks/useScrollToBottom";

interface ChatWindowProps {
    groupId: string;
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

    useEffect(() => {
        if (groupId) {
            loadMessages(groupId);
        }
    }, [groupId, loadMessages]);

    const handleSendMessage = async (content: string) => {
        await sendMessage(groupId, content);
        scrollToBottom();
    };

    const messageList = useMemo(() => {
        return messages.map(({ id, content, senderUsername, timestamp, isEdited }) => (
            <Message
                key={id}
                content={content}
                senderUsername={senderUsername}
                timestamp={timestamp}
                isEdited={isEdited}
            />
        ));
    }, [messages]);

    if (loading && messages.length === 0) {
        return (
            <div className="border border-gray-300 rounded-lg p-4 h-full flex flex-col">
                <div className="flex-grow flex items-center justify-center">
                    <div className="text-center">
                        <svg className="animate-spin h-8 w-8 text-blue-500 mx-auto mb-2" viewBox="0 0 24 24">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                        </svg>
                        <p className="text-gray-500">Loading messages...</p>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="border border-gray-300 rounded-lg p-4 h-full flex flex-col">
            <div 
                ref={containerRef}
                className="flex-grow overflow-y-auto space-y-2"
                role="log"
                aria-live="polite"
                aria-label="Messages"
            >
                {messageList.length > 0 ? (
                    messageList
                ) : (
                    <div className="text-center text-gray-500 py-8">
                        <p>No messages yet. Start the conversation!</p>
                    </div>
                )}
            </div>
            <div className="flex-shrink-0">
                <ErrorDisplay
                    error={error}
                    onDismiss={clearError}
                    onRetry={retryLastAction}
                />
                <MessageInput
                    onSendMessage={handleSendMessage}
                    disabled={sending}
                />
            </div>
        </div>
    );
});

ChatWindow.displayName = 'ChatWindow';

export default ChatWindow;
