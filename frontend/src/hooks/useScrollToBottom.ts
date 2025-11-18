import { useEffect, useRef } from 'react';

export function useScrollToBottom(dependencies: any[] = []) {
    const containerRef = useRef<HTMLDivElement>(null);

    const scrollToBottom = () => {
        if (containerRef.current) {
            containerRef.current.scrollTop = containerRef.current.scrollHeight;
        }
    };

    useEffect(() => {
        scrollToBottom();
    }, dependencies);

    return { containerRef, scrollToBottom };
}
