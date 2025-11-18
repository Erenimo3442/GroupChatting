import { useState, useCallback, useRef } from 'react';
import { fetchMessages, sendMessage as sendMessageService } from '../services/messageService';
import type { MessageResponseDto } from '../generated/api-client';

interface UseMessagesState {
    messages: MessageResponseDto[];
    loading: boolean;
    sending: boolean;
    error: string | null;
}

interface UseMessagesActions {
    loadMessages: (groupId: string) => Promise<void>;
    sendMessage: (groupId: string, content: string) => Promise<void>;
    clearError: () => void;
    retryLastAction: () => Promise<void>;
}

export function useMessages(): UseMessagesState & UseMessagesActions {
    const [state, setState] = useState<UseMessagesState>({
        messages: [],
        loading: false,
        sending: false,
        error: null,
    });

    const lastActionRef = useRef<{
        type: 'load' | 'send';
        params: any;
    } | null>(null);

    const clearError = useCallback(() => {
        setState(prev => ({ ...prev, error: null }));
    }, []);

    const loadMessages = useCallback(async (groupId: string) => {
        setState(prev => ({ ...prev, loading: true, error: null }));
        lastActionRef.current = { type: 'load', params: { groupId } };

        try {
            const fetchedMessages = await fetchMessages(groupId);
            setState(prev => ({
                ...prev,
                messages: fetchedMessages,
                loading: false,
            }));
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to load messages';
            setState(prev => ({
                ...prev,
                loading: false,
                error: errorMessage,
            }));
        }
    }, []);

    const sendMessage = useCallback(async (groupId: string, content: string) => {
        setState(prev => ({ ...prev, sending: true, error: null }));
        lastActionRef.current = { type: 'send', params: { groupId, content } };

        try {
            const saved = await sendMessageService({ groupId, content });
            setState(prev => ({
                ...prev,
                messages: [...prev.messages, saved],
                sending: false,
            }));
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to send message';
            setState(prev => ({
                ...prev,
                sending: false,
                error: errorMessage,
            }));
            throw err;
        }
    }, []);

    const retryLastAction = useCallback(async () => {
        const lastAction = lastActionRef.current;
        if (!lastAction) return;

        clearError();

        try {
            if (lastAction.type === 'load') {
                await loadMessages(lastAction.params.groupId);
            } else if (lastAction.type === 'send') {
                await sendMessage(lastAction.params.groupId, lastAction.params.content);
            }
        } catch (err) {
            // Error is already handled by the individual functions
        }
    }, [clearError, loadMessages, sendMessage]);

    return {
        ...state,
        loadMessages,
        sendMessage: sendMessage,
        clearError,
        retryLastAction,
    };
}
