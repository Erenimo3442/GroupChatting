
interface ErrorDisplayProps {
    error: string | null;
    onDismiss: () => void;
    onRetry?: () => void;
    type?: 'error' | 'warning' | 'info';
}

export default function ErrorDisplay({ 
    error, 
    onDismiss, 
    onRetry, 
    type = 'error' 
}: ErrorDisplayProps) {
    if (!error) return null;

    const getStyles = () => {
        switch (type) {
            case 'warning':
                return 'bg-yellow-50 border-yellow-200 text-yellow-800';
            case 'info':
                return 'bg-blue-50 border-blue-200 text-blue-800';
            default:
                return 'bg-red-50 border-red-200 text-red-800';
        }
    };

    const getIconColor = () => {
        switch (type) {
            case 'warning':
                return 'text-yellow-400';
            case 'info':
                return 'text-blue-400';
            default:
                return 'text-red-400';
        }
    };

    const getIcon = () => {
        switch (type) {
            case 'warning':
                return (
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                );
            case 'info':
                return (
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                );
            default:
                return (
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                );
        }
    };

    return (
        <div className={`mb-4 p-3 border rounded-lg ${getStyles()}`} role="alert" aria-live="polite">
            <div className="flex items-start">
                <svg className={`h-5 w-5 mr-2 mt-0.5 flex-shrink-0 ${getIconColor()}`} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    {getIcon()}
                </svg>
                <div className="flex-1">
                    <span className="text-sm font-medium">{error}</span>
                    <div className="mt-2 flex space-x-2">
                        <button
                            onClick={onDismiss}
                            className="text-xs underline hover:opacity-75 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-current rounded"
                            aria-label="Dismiss error"
                        >
                            Dismiss
                        </button>
                        {onRetry && (
                            <button
                                onClick={onRetry}
                                className="text-xs underline hover:opacity-75 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-current rounded"
                                aria-label="Retry operation"
                            >
                                Try Again
                            </button>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
